using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Dtos;
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
        if (audit is null)
        {
            return null;
        }

        var student = await _db.students.AsNoTracking().FirstAsync(x => x.student_id == studentId, cancellationToken);
        var completed = await _db.v_latest_attempts.AsNoTracking()
            .Where(x => x.student_id == studentId && (x.is_completed == true || (x.credits ?? 0) > 0))
            .Select(x => x.course_code!)
            .ToListAsync(cancellationToken);
        var completedSet = completed.Select(x => x.ToUpperInvariant()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var requirements = student.cohort_id.HasValue
            ? await _db.curriculum_requirements.AsNoTracking().Where(x => x.cohort_id == student.cohort_id.Value).ToListAsync(cancellationToken)
            : [];

        var plannedCodes = await _db.student_plans.AsNoTracking()
            .Where(x => x.student_id == studentId && x.term_code == targetTermCode)
            .Select(x => x.course_code)
            .ToListAsync(cancellationToken);

        var offerings = await _db.course_offerings.AsNoTracking()
            .Where(x => x.term_code == targetTermCode && x.is_open)
            .ToListAsync(cancellationToken);
        var offeredCodes = offerings.Select(x => x.course_code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var hasOfferingData = offerings.Count > 0;

        var candidates = new Dictionary<string, CourseRecommendationDto>(StringComparer.OrdinalIgnoreCase);
        foreach (var req in requirements)
        {
            if (!string.IsNullOrWhiteSpace(req.course_code))
            {
                AddCandidate(req.course_code!, req.kind == "course" && req.is_required ? "required_core" : "major_core", req.prereq_rule);
            }

            foreach (var allowed in req.allowed_courses ?? [])
            {
                AddCandidate(allowed, "elective_to_fill_bucket", req.prereq_rule);
            }
        }

        var concentrationEntry = await BuildConcentrationCandidates(studentId, completedSet, cancellationToken);
        foreach (var item in concentrationEntry)
        {
            candidates[item.CourseCode] = item;
        }

        var ordered = candidates.Values
            .OrderByDescending(x => x.PriorityScore)
            .ThenBy(x => x.CourseCode)
            .ToList();

        decimal credits = 0;
        var recommended = new List<CourseRecommendationDto>();
        var relevant = new List<CourseRecommendationDto>();
        foreach (var candidate in ordered)
        {
            if (completedSet.Contains(candidate.CourseCode) || plannedCodes.Contains(candidate.CourseCode))
            {
                continue;
            }

            if (hasOfferingData && !offeredCodes.Contains(candidate.CourseCode))
            {
                relevant.Add(candidate with { CanRegister = false, Warnings = ["Khong mo lop hoc ky muc tieu"] });
                continue;
            }

            if (credits + candidate.Credits > maxCredits)
            {
                relevant.Add(candidate with { Warnings = candidate.Warnings.Append("Vuot maxCredits").ToList() });
                continue;
            }

            recommended.Add(candidate);
            credits += candidate.Credits;
            if (credits >= minCredits)
            {
                break;
            }
        }

        var blockers = new List<string>();
        var advisories = new List<string>();
        if (!audit.Summary.Eligibility300To400.IsEligible)
        {
            blockers.AddRange(audit.Summary.Eligibility300To400.MissingReasons);
            advisories.Add("Ban chua du dieu kien dang ky level 300-400.");
        }

        if (!audit.Summary.Concentration.IsSelected)
        {
            advisories.Add("Ban chua chon concentration nen he thong gioi han goi y concentration.");
        }

        if (recommended.Any(x => x.CourseCode.Equals("BUS 450", StringComparison.OrdinalIgnoreCase)))
        {
            advisories.Add("BUS 450 (start) can dang ky qua email huong dan cua Khoa.");
        }

        if (recommended.Any(x => x.CourseCode.Equals("BUS 496", StringComparison.OrdinalIgnoreCase)))
        {
            advisories.Add("BUS 496 la capstone, nen chi hoc them 1-2 mon.");
        }

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

        void AddCandidate(string code, string type, string? prereqRule)
        {
            var normalized = code.ToUpperInvariant();
            if (completedSet.Contains(normalized))
            {
                return;
            }

            if (!candidates.ContainsKey(normalized))
            {
                candidates[normalized] = new CourseRecommendationDto(
                    normalized,
                    normalized,
                    3,
                    type,
                    Score(type),
                    ["Tu curriculum requirement"],
                    [],
                    normalized == "BUS 450" ? "manual_email" : "portal",
                    true
                );
            }

            var evaluatorResult = _prerequisiteEvaluator.Evaluate(prereqRule, new PrerequisiteContext(completedSet, student.english_level, student.ielts_score));
            if (!evaluatorResult.IsSatisfied)
            {
                var existing = candidates[normalized];
                candidates[normalized] = existing with
                {
                    PriorityScore = -1000,
                    CanRegister = false,
                    Warnings = evaluatorResult.BlockingReasons
                };
            }
        }
    }

    private async Task<List<CourseRecommendationDto>> BuildConcentrationCandidates(
        string studentId,
        HashSet<string> completedSet,
        CancellationToken cancellationToken)
    {
        var studentConcentration = await _db.student_concentrations.AsNoTracking()
            .Where(x => x.student_id == studentId && x.status == "active")
            .OrderByDescending(x => x.created_at)
            .FirstOrDefaultAsync(cancellationToken);
        if (studentConcentration is null)
        {
            return [];
        }

        var rows = await _db.concentration_courses.AsNoTracking()
            .Include(x => x.course_codeNavigation)
            .Where(x => x.concentration_id == studentConcentration.concentration_id)
            .OrderByDescending(x => x.is_entry_course)
            .ThenBy(x => x.sort_order)
            .ToListAsync(cancellationToken);

        return rows
            .Where(x => !completedSet.Contains(x.course_code))
            .Select(x => new CourseRecommendationDto(
                x.course_code,
                x.course_codeNavigation.course_name,
                x.course_codeNavigation.credits ?? 3,
                x.is_entry_course ? "concentration_entry" : "concentration_required",
                x.is_entry_course ? 90 : 85,
                [x.is_entry_course ? "Mon mo khoa concentration" : "Mon bat buoc concentration"],
                [],
                "portal",
                true
            ))
            .ToList();
    }

    private static int Score(string recommendationType) => recommendationType switch
    {
        "required_core" => 100,
        "concentration_entry" => 90,
        "concentration_required" => 85,
        "major_core" => 80,
        "elective_to_fill_bucket" => 50,
        _ => 20
    };
}
