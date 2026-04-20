using System.Text.Json;
using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Dtos;
using AcademicPlanner.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Api.Services;

public interface IRoadmapRecommendationService
{
    Task<RecommendationResponseDto?> GetNextTermAsync(
        string studentId,
        int targetTermCode,
        int minCredits,
        int maxCredits,
        string strategy,
        CancellationToken cancellationToken
    );
}

/// <summary>
/// Builds a ranked course recommendation list using three purely rule-based
/// heuristic dimensions (no ML):
///
///   1. mandatory_score  (+50) — course belongs to a mandatory knowledge_block
///   2. unlock_score     (+10 per course unlocked) — courses that act as prereqs
///      for other courses in the curriculum (unlock_score = count of courses whose
///      prereq_rule contains this course code)
///   3. cohort_track_score (+20 / -10) — whether the course keeps the student
///      on-pace with their cohort's expected timeline
///
/// Total = base_type_score + mandatory_score + unlock_score + cohort_track_score
/// </summary>
public class RoadmapRecommendationService : IRoadmapRecommendationService
{
    private readonly AppDbContext _db;
    private readonly IStudentAuditService _auditService;
    private readonly IPrerequisiteEvaluator _prerequisiteEvaluator;

    public RoadmapRecommendationService(
        AppDbContext db,
        IStudentAuditService auditService,
        IPrerequisiteEvaluator prerequisiteEvaluator)
    {
        _db = db;
        _auditService = auditService;
        _prerequisiteEvaluator = prerequisiteEvaluator;
    }

    public async Task<RecommendationResponseDto?> GetNextTermAsync(
        string studentId,
        int targetTermCode,
        int minCredits,
        int maxCredits,
        string strategy,
        CancellationToken cancellationToken)
    {
        var audit = await _auditService.GetAuditAsync(studentId, cancellationToken);
        if (audit is null) return null;

        var student = await _db.students.AsNoTracking()
            .FirstAsync(x => x.student_id == studentId, cancellationToken);

        // ── Completed courses ────────────────────────────────────────────────
        var completedAttempts = await _db.v_latest_attempts.AsNoTracking()
            .Where(x => x.student_id == studentId && (x.is_completed == true || (x.credits ?? 0) > 0))
            .ToListAsync(cancellationToken);

        var completedSet = completedAttempts
            .Select(x => x.course_code!.ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        decimal totalCompletedCredits = completedAttempts.Sum(x => x.credits ?? 0);
        decimal gpa = audit.Summary.GpaComputed();

        // ── Curriculum requirements ──────────────────────────────────────────
        var requirements = student.cohort_id.HasValue
            ? await _db.curriculum_requirements.AsNoTracking()
                .Where(x => x.cohort_id == student.cohort_id.Value)
                .ToListAsync(cancellationToken)
            : new List<curriculum_requirement>();

        // ── Curricula (ORDBMS: knowledge_block[] + course_mapping JSONB) ────
        curriculum? curricula = null;
        if (student.program_id.HasValue && student.cohort_id.HasValue)
        {
            curricula = await _db.curricula.AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.program_id == student.program_id.Value &&
                    c.cohort_id  == student.cohort_id.Value,
                    cancellationToken);
        }

        // Build mandatory block set from knowledge_block[] ORDBMS data
        var mandatoryBlockNames = curricula?.structure
            .Where(b => b.IsMandatory)
            .Select(b => b.BlockName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Build course → block name mapping from JSONB course_mapping
        var courseToBlock = BuildCourseToBlock(curricula?.course_mapping);

        // ── Unlock score: count how many other requirements reference each code ──
        var unlockScores = BuildUnlockScores(requirements);

        // ── Cohort timeline: map category → expected term sequence ───────────
        var cohortTimeline = BuildCohortTimeline(curricula);

        // ── Student's current term sequence (how many terms they've attended) ─
        int studentTermSeq = completedAttempts.Select(a => a.term_code).Distinct().Count();

        // ── Planned courses for target term ──────────────────────────────────
        var plannedCodes = await _db.student_plans.AsNoTracking()
            .Where(x => x.student_id == studentId && x.term_code == targetTermCode)
            .Select(x => x.course_code)
            .ToListAsync(cancellationToken);

        // ── Available offerings ───────────────────────────────────────────────
        var offerings = await _db.course_offerings.AsNoTracking()
            .Where(x => x.term_code == targetTermCode && x.is_open)
            .ToListAsync(cancellationToken);
        var offeredCodes = offerings.Select(x => x.course_code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var hasOfferingData = offerings.Count > 0;

        // ── Advisories ────────────────────────────────────────────────────────
        var prereqCtx = new PrerequisiteContext(completedSet, student.english_level, student.ielts_score, totalCompletedCredits, gpa);

        var candidates = new Dictionary<string, CourseRecommendationDto>(StringComparer.OrdinalIgnoreCase);

        // Add candidates from curriculum requirements
        foreach (var req in requirements)
        {
            if (!string.IsNullOrWhiteSpace(req.course_code))
            {
                var baseType = req.kind == "course" && req.is_required ? "required_core" : "major_core";
                AddCandidate(req.course_code!, baseType, req.prereq_rule, req.is_required);
            }
            foreach (var allowed in req.allowed_courses ?? [])
                AddCandidate(allowed, "elective_to_fill_bucket", req.prereq_rule, false);
        }

        // Add concentration candidates
        var concentrationCandidates = await BuildConcentrationCandidates(studentId, completedSet, prereqCtx, cancellationToken);
        foreach (var item in concentrationCandidates)
            candidates[item.CourseCode] = item;

        // ── Sort by heuristic total score ─────────────────────────────────────
        var ordered = candidates.Values
            .OrderByDescending(x => x.PriorityScore)
            .ThenBy(x => x.CourseCode)
            .ToList();

        decimal credits = 0;
        var recommended = new List<CourseRecommendationDto>();
        var relevant    = new List<CourseRecommendationDto>();

        foreach (var candidate in ordered)
        {
            if (completedSet.Contains(candidate.CourseCode) || plannedCodes.Contains(candidate.CourseCode))
                continue;

            if (hasOfferingData && !offeredCodes.Contains(candidate.CourseCode))
            {
                relevant.Add(candidate with { CanRegister = false, Warnings = ["Not offered this term"] });
                continue;
            }

            if (!candidate.CanRegister)
            {
                relevant.Add(candidate);
                continue;
            }

            if (credits + candidate.Credits > maxCredits)
            {
                relevant.Add(candidate with { Warnings = candidate.Warnings.Append("Exceeds max credit limit").ToList() });
                continue;
            }

            recommended.Add(candidate);
            credits += candidate.Credits;
            if (credits >= minCredits) break;
        }

        // ── Blockers & advisory notes ─────────────────────────────────────────
        var blockers   = new List<string>();
        var advisories = new List<string>();

        if (!audit.Summary.Eligibility300To400.IsEligible)
        {
            blockers.AddRange(audit.Summary.Eligibility300To400.MissingReasons);
            advisories.Add("You have not yet met the requirements to register for 300-400 level courses.");
        }

        if (!audit.Summary.Concentration.IsSelected)
            advisories.Add("You have not selected a concentration. Concentration course recommendations are limited.");

        if (recommended.Any(x => x.CourseCode.Equals("BUS 450", StringComparison.OrdinalIgnoreCase)))
            advisories.Add("BUS 450 (Internship start) requires registration via faculty email — check with your advisor.");

        if (recommended.Any(x => x.CourseCode.Equals("BUS 496", StringComparison.OrdinalIgnoreCase)))
            advisories.Add("BUS 496 (Capstone) — limit your schedule to 1-2 additional courses.");

        return new RecommendationResponseDto(
            studentId,
            targetTermCode,
            strategy,
            credits,
            recommended,
            relevant,
            blockers.Distinct().ToList(),
            advisories.Distinct().ToList()
        );

        // ── Local function: add or update a candidate in the dictionary ───────
        void AddCandidate(string code, string baseType, string? prereqRule, bool isRequired)
        {
            var normalized = code.ToUpperInvariant();
            if (completedSet.Contains(normalized)) return;

            // Evaluate prerequisites
            var prereqResult = _prerequisiteEvaluator.Evaluate(prereqRule, prereqCtx);

            // ── Three-dimension heuristic score ────────────────────────────
            int baseScore       = BaseTypeScore(baseType);
            int mandatoryScore  = IsMandatory(normalized) ? 50 : 0;
            int unlockScore     = unlockScores.GetValueOrDefault(normalized, 0) * 10;
            int cohortScore     = CohortTrackScore(normalized);
            int totalScore      = prereqResult.IsSatisfied
                ? baseScore + mandatoryScore + unlockScore + cohortScore
                : -1000;

            var reasons = new List<string>();
            if (mandatoryScore > 0) reasons.Add("Mandatory block course");
            if (unlockScore > 0) reasons.Add($"Unlocks {unlockScores.GetValueOrDefault(normalized, 0)} other course(s)");
            if (cohortScore > 0) reasons.Add("On-pace with cohort");
            if (reasons.Count == 0) reasons.Add("From curriculum");

            var regChannel = normalized == "BUS 450" ? "manual_email" : "portal";

            if (!candidates.ContainsKey(normalized))
            {
                candidates[normalized] = new CourseRecommendationDto(
                    normalized,
                    normalized,
                    3,
                    baseType,
                    totalScore,
                    reasons,
                    prereqResult.IsSatisfied ? [] : prereqResult.BlockingReasons.ToList(),
                    regChannel,
                    prereqResult.IsSatisfied
                );
            }
            else if (totalScore > candidates[normalized].PriorityScore)
            {
                // Keep the higher-scoring classification
                var existing = candidates[normalized];
                candidates[normalized] = existing with
                {
                    RecommendationType = baseType,
                    PriorityScore      = totalScore,
                    Reasons            = reasons,
                    Warnings           = prereqResult.IsSatisfied ? [] : prereqResult.BlockingReasons.ToList(),
                    CanRegister        = prereqResult.IsSatisfied,
                };
            }
        }

        bool IsMandatory(string courseCode) =>
            courseToBlock.TryGetValue(courseCode, out var blockName) && mandatoryBlockNames.Contains(blockName);

        int CohortTrackScore(string courseCode)
        {
            if (!cohortTimeline.TryGetValue(courseCode, out int expectedSeq)) return 0;
            int diff = studentTermSeq - expectedSeq;
            return diff switch
            {
                0  =>  20,   // exactly on-pace
                1  =>  15,   // one term behind — still important
                -1 =>  10,   // one term ahead — fine
                _ when diff >= 2 => -10, // significantly behind — lower priority vs on-track
                _ => 5
            };
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Concentration candidates
    // ────────────────────────────────────────────────────────────────────────

    private async Task<List<CourseRecommendationDto>> BuildConcentrationCandidates(
        string studentId,
        HashSet<string> completedSet,
        PrerequisiteContext prereqCtx,
        CancellationToken cancellationToken)
    {
        var sc = await _db.student_concentrations.AsNoTracking()
            .Where(x => x.student_id == studentId && x.status == "active")
            .OrderByDescending(x => x.created_at)
            .FirstOrDefaultAsync(cancellationToken);

        if (sc is null) return [];

        var rows = await _db.concentration_courses.AsNoTracking()
            .Include(x => x.course_codeNavigation)
            .Where(x => x.concentration_id == sc.concentration_id)
            .OrderByDescending(x => x.is_entry_course)
            .ThenBy(x => x.sort_order)
            .ToListAsync(cancellationToken);

        return rows
            .Where(x => !completedSet.Contains(x.course_code))
            .Select(x =>
            {
                var type  = x.is_entry_course ? "concentration_entry" : "concentration_required";
                var score = x.is_entry_course ? 90 : 85;
                var reasons = x.is_entry_course
                    ? new List<string> { "Unlocks concentration track" }
                    : new List<string> { "Required by concentration" };
                return new CourseRecommendationDto(
                    x.course_code,
                    x.course_codeNavigation?.course_name,
                    x.course_codeNavigation?.credits ?? 3,
                    type,
                    score,
                    reasons,
                    [],
                    "portal",
                    true
                );
            })
            .ToList();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Build unlock score map: courseCode → count of courses it unlocks
    // ────────────────────────────────────────────────────────────────────────

    private static Dictionary<string, int> BuildUnlockScores(IReadOnlyList<curriculum_requirement> requirements)
    {
        var scores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var req in requirements)
        {
            if (string.IsNullOrWhiteSpace(req.prereq_rule)) continue;
            try
            {
                using var doc = JsonDocument.Parse(req.prereq_rule);
                foreach (var code in ExtractCoursesFromRule(doc.RootElement))
                {
                    var upper = code.ToUpperInvariant();
                    scores[upper] = scores.GetValueOrDefault(upper, 0) + 1;
                }
            }
            catch { /* ignore malformed JSON */ }
        }
        return scores;
    }

    /// <summary>Recursively collect all COMPLETED course codes from a rule tree.</summary>
    private static IEnumerable<string> ExtractCoursesFromRule(JsonElement node)
    {
        // op/args format
        if (node.TryGetProperty("op", out var op))
        {
            var opStr = op.GetString()?.ToUpperInvariant();
            if (opStr == "COMPLETED" && node.TryGetProperty("course", out var codeEl))
            {
                var code = codeEl.GetString();
                if (!string.IsNullOrWhiteSpace(code)) yield return code;
                yield break;
            }

            if ((opStr == "AND" || opStr == "OR") && node.TryGetProperty("args", out var args))
            {
                foreach (var child in args.EnumerateArray())
                    foreach (var c in ExtractCoursesFromRule(child))
                        yield return c;
            }
            else if (opStr == "NOT" && node.TryGetProperty("arg", out var arg))
            {
                foreach (var c in ExtractCoursesFromRule(arg))
                    yield return c;
            }
            yield break;
        }

        // Legacy type format
        if (node.TryGetProperty("type", out var typeEl))
        {
            var type = typeEl.GetString()?.ToLowerInvariant();
            if (type == "course" && node.TryGetProperty("courses", out var courses))
            {
                foreach (var course in courses.EnumerateArray())
                    if (course.TryGetProperty("code", out var cEl))
                    {
                        var code = cEl.GetString();
                        if (!string.IsNullOrWhiteSpace(code)) yield return code;
                    }
            }
            else if ((type == "and" || type == "or") && node.TryGetProperty("rules", out var rules))
            {
                foreach (var rule in rules.EnumerateArray())
                    foreach (var c in ExtractCoursesFromRule(rule))
                        yield return c;
            }
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Build course → block name from JSONB course_mapping
    // ────────────────────────────────────────────────────────────────────────

    private static Dictionary<string, string> BuildCourseToBlock(string? courseMappingJson)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(courseMappingJson)) return result;
        try
        {
            using var doc = JsonDocument.Parse(courseMappingJson);
            foreach (var blockProp in doc.RootElement.EnumerateObject())
            {
                if (blockProp.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var code in blockProp.Value.EnumerateArray())
                    {
                        var c = code.GetString();
                        if (!string.IsNullOrWhiteSpace(c))
                            result[c.ToUpperInvariant()] = blockProp.Name;
                    }
                }
            }
        }
        catch { /* ignore */ }
        return result;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Build cohort timeline: course → expected term sequence (1-based)
    // Uses the order of blocks + courses in the JSONB course_mapping
    // ────────────────────────────────────────────────────────────────────────

    private static Dictionary<string, int> BuildCohortTimeline(curriculum? curricula)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (curricula is null) return result;

        try
        {
            // Use structure[] ordering for block sequence, then course_mapping for course list
            var blockOrder = curricula.structure
                .Select((b, i) => (b.BlockName, Index: i))
                .ToDictionary(x => x.BlockName, x => x.Index, StringComparer.OrdinalIgnoreCase);

            using var doc = JsonDocument.Parse(curricula.course_mapping);
            foreach (var blockProp in doc.RootElement.EnumerateObject())
            {
                if (!blockOrder.TryGetValue(blockProp.Name, out int blockIdx)) continue;
                // Each block maps roughly to 2 terms (semester 1-2, 3-4, …)
                int expectedTerm = blockIdx * 2 + 1;

                if (blockProp.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var code in blockProp.Value.EnumerateArray())
                    {
                        var c = code.GetString();
                        if (!string.IsNullOrWhiteSpace(c))
                            result[c.ToUpperInvariant()] = expectedTerm;
                    }
                }
            }
        }
        catch { /* ignore */ }
        return result;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    private static int BaseTypeScore(string recommendationType) => recommendationType switch
    {
        "required_core"          => 100,
        "concentration_entry"    => 90,
        "concentration_required" => 85,
        "major_core"             => 80,
        "elective_to_fill_bucket"=> 50,
        _ => 20
    };
}

// Extension to expose computed GPA from audit DTO
file static class AuditExtensions
{
    internal static decimal GpaComputed(this StudentAuditSummaryDto summary)
        => 0; // GPA not in summary DTO; evaluator uses it only if provided
}
