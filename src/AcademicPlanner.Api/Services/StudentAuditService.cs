using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Dtos;
using AcademicPlanner.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Api.Services;

public interface IStudentAuditService
{
    Task<StudentAuditDto?> GetAuditAsync(string studentId, CancellationToken cancellationToken);
    Task<StudentAuditSummaryDto?> GetAuditSummaryAsync(string studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>?> GetMissingCoursesAsync(string studentId, CancellationToken cancellationToken);
    Task<Eligibility300To400Dto?> GetEligibility300To400Async(string studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryProgressDto>?> GetProgressByCategoryAsync(string studentId, CancellationToken cancellationToken);
}

public class StudentAuditService : IStudentAuditService
{
    private static readonly HashSet<string> PassingGrades = ["A", "A-", "B+", "B", "B-", "C+", "C", "C-", "D+", "D", "D-", "P"];
    private readonly AppDbContext _db;

    public StudentAuditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<StudentAuditDto?> GetAuditAsync(string studentId, CancellationToken cancellationToken)
    {
        var summary = await GetAuditSummaryAsync(studentId, cancellationToken);
        if (summary is null)
        {
            return null;
        }

        var missing = await GetMissingCoursesAsync(studentId, cancellationToken) ?? [];
        var progress = await GetProgressByCategoryAsync(studentId, cancellationToken) ?? [];
        return new StudentAuditDto(summary, progress, missing);
    }

    public async Task<StudentAuditSummaryDto?> GetAuditSummaryAsync(string studentId, CancellationToken cancellationToken)
    {
        var snapshot = await BuildSnapshotAsync(studentId, cancellationToken);
        if (snapshot is null)
        {
            return null;
        }

        var missingCourses = ComputeMissingRequiredCourses(snapshot);
        var requiredCompleted = snapshot.RequiredCourseCodes.Count - missingCourses.Count;
        var electiveCompleted = ComputeElectiveCompletedCredits(snapshot);
        var electiveTarget = ComputeElectiveTargetCredits(snapshot);
        var eligibility = ComputeEligibility(snapshot);
        var warnings = BuildWarnings(snapshot, eligibility);
        var overallPercent = Math.Round(Math.Min(100m, snapshot.TotalCompletedCredits / 195m * 100m), 1);

        return new StudentAuditSummaryDto(
            snapshot.Student.student_id,
            snapshot.Program?.program_code,
            snapshot.Cohort?.cohort_code,
            snapshot.TotalCompletedCredits,
            snapshot.CompletedCredits100to200,
            snapshot.CompletedCredits300to400,
            requiredCompleted,
            missingCourses.Count,
            electiveCompleted,
            Math.Max(0, electiveTarget - electiveCompleted),
            snapshot.ConcentrationInfo,
            eligibility,
            overallPercent,
            warnings
        );
    }

    public async Task<IReadOnlyList<string>?> GetMissingCoursesAsync(string studentId, CancellationToken cancellationToken)
    {
        var snapshot = await BuildSnapshotAsync(studentId, cancellationToken);
        return snapshot is null ? null : ComputeMissingRequiredCourses(snapshot);
    }

    public async Task<Eligibility300To400Dto?> GetEligibility300To400Async(string studentId, CancellationToken cancellationToken)
    {
        var snapshot = await BuildSnapshotAsync(studentId, cancellationToken);
        return snapshot is null ? null : ComputeEligibility(snapshot);
    }

    public async Task<IReadOnlyList<CategoryProgressDto>?> GetProgressByCategoryAsync(string studentId, CancellationToken cancellationToken)
    {
        var snapshot = await BuildSnapshotAsync(studentId, cancellationToken);
        if (snapshot is null)
        {
            return null;
        }

        var result = snapshot.Requirements
            .GroupBy(x => x.category_id)
            .Select(group =>
            {
                var category = snapshot.Categories[group.Key];
                var requiredCredits = category.min_credits ?? 0;
                var requiredCourses = group.Where(x => x.kind == "course" && x.is_required && !string.IsNullOrWhiteSpace(x.course_code)).ToList();
                var requiredRemaining = requiredCourses.Count(req => !snapshot.CompletedCourseCodes.Contains(req.course_code!.ToUpperInvariant()));
                var requiredCompleted = requiredCourses.Count - requiredRemaining;
                var earnedCredits = ComputeCategoryEarnedCredits(group.ToList(), snapshot);
                return new CategoryProgressDto(
                    category.category_id,
                    category.category_name,
                    requiredCredits,
                    earnedCredits,
                    Math.Max(0, requiredCredits - earnedCredits),
                    requiredCompleted,
                    requiredRemaining
                );
            })
            .OrderBy(x => x.CategoryId)
            .ToList();

        return result;
    }

    private async Task<StudentSnapshot?> BuildSnapshotAsync(string studentId, CancellationToken cancellationToken)
    {
        var student = await _db.students.AsNoTracking().FirstOrDefaultAsync(x => x.student_id == studentId, cancellationToken);
        if (student is null)
        {
            return null;
        }

        var program = student.program_id.HasValue
            ? await _db.programs.AsNoTracking().FirstOrDefaultAsync(x => x.program_id == student.program_id.Value, cancellationToken)
            : null;
        var cohort = student.cohort_id.HasValue
            ? await _db.cohorts.AsNoTracking().FirstOrDefaultAsync(x => x.cohort_id == student.cohort_id.Value, cancellationToken)
            : null;

        var requirements = student.cohort_id.HasValue
            ? await _db.curriculum_requirements.AsNoTracking()
                .Where(x => x.cohort_id == student.cohort_id.Value)
                .ToListAsync(cancellationToken)
            : [];

        var categories = await _db.curriculum_categories.AsNoTracking()
            .Where(x => student.program_id.HasValue && x.program_id == student.program_id.Value)
            .ToDictionaryAsync(x => x.category_id, cancellationToken);

        var latestAttempts = await _db.v_latest_attempts.AsNoTracking()
            .Where(x => x.student_id == studentId)
            .ToListAsync(cancellationToken);

        var completedCourseCodes = latestAttempts
            .Where(IsAttemptPassed)
            .Select(x => x.course_code)
            .OfType<string>()
            .Select(x => x.ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var courseCodes = latestAttempts.Select(x => x.course_code).OfType<string>().Distinct().ToList();
        var courseMap = await _db.courses.AsNoTracking()
            .Where(x => courseCodes.Contains(x.course_code))
            .ToDictionaryAsync(x => x.course_code, cancellationToken);

        var equivalentRows = student.cohort_id.HasValue
            ? await _db.equivalencies.AsNoTracking()
                .Where(x => x.cohort_id == student.cohort_id.Value)
                .ToListAsync(cancellationToken)
            : [];
        ExpandByEquivalency(completedCourseCodes, equivalentRows);

        decimal totalCompletedCredits = 0;
        decimal credits100to200 = 0;
        decimal credits300to400 = 0;
        foreach (var attempt in latestAttempts.Where(IsAttemptPassed))
        {
            var credits = attempt.credits ?? 0;
            totalCompletedCredits += credits;
            if (attempt.course_code is not null && courseMap.TryGetValue(attempt.course_code, out var course))
            {
                if (course.course_level is >= 100 and <= 299)
                {
                    credits100to200 += credits;
                }

                if (course.course_level is >= 300 and <= 499)
                {
                    credits300to400 += credits;
                }
            }
        }

        var concentration = await _db.student_concentrations.AsNoTracking()
            .Include(x => x.concentration)
            .Where(x => x.student_id == studentId && x.status == "active")
            .OrderByDescending(x => x.created_at)
            .FirstOrDefaultAsync(cancellationToken);

        var concentrationInfo = concentration is null
            ? new ConcentrationInfoDto(null, null, false)
            : new ConcentrationInfoDto(concentration.concentration.concentration_code, concentration.concentration.concentration_name, true);

        var requiredCodes = requirements
            .Where(x => x.kind == "course" && x.is_required && !string.IsNullOrWhiteSpace(x.course_code))
            .Select(x => x.course_code!.ToUpperInvariant())
            .Distinct()
            .ToList();

        return new StudentSnapshot(
            student,
            program,
            cohort,
            requirements,
            categories,
            latestAttempts,
            completedCourseCodes,
            requiredCodes,
            totalCompletedCredits,
            credits100to200,
            credits300to400,
            concentrationInfo
        );
    }

    private static void ExpandByEquivalency(HashSet<string> completed, List<equivalency> equivalencies)
    {
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var eq in equivalencies)
            {
                var from = eq.course_code.ToUpperInvariant();
                var to = eq.equivalent_course_code.ToUpperInvariant();
                if (completed.Contains(from) && completed.Add(to))
                {
                    changed = true;
                }

                if (completed.Contains(to) && completed.Add(from))
                {
                    changed = true;
                }
            }
        }
    }

    private static bool IsAttemptPassed(v_latest_attempt attempt)
    {
        if (attempt.is_completed == true || (attempt.credits ?? 0) > 0)
        {
            return true;
        }

        if (attempt.grade_letter is null)
        {
            return false;
        }

        return PassingGrades.Contains(attempt.grade_letter);
    }

    private static List<string> ComputeMissingRequiredCourses(StudentSnapshot snapshot)
    {
        return snapshot.RequiredCourseCodes
            .Where(code => !snapshot.CompletedCourseCodes.Contains(code))
            .OrderBy(x => x)
            .ToList();
    }

    private static decimal ComputeElectiveCompletedCredits(StudentSnapshot snapshot)
    {
        var bucketRequirements = snapshot.Requirements.Where(x => x.kind == "credit_bucket").ToList();
        decimal total = 0;
        foreach (var req in bucketRequirements)
        {
            var allowed = req.allowed_courses ?? [];
            foreach (var courseCode in allowed.Select(x => x.ToUpperInvariant()))
            {
                if (snapshot.CompletedCourseCodes.Contains(courseCode))
                {
                    total += snapshot.LatestAttempts.FirstOrDefault(a => string.Equals(a.course_code, courseCode, StringComparison.OrdinalIgnoreCase))?.credits ?? 0;
                }
            }
        }

        return total;
    }

    private static decimal ComputeElectiveTargetCredits(StudentSnapshot snapshot)
    {
        return snapshot.Requirements.Where(x => x.kind == "credit_bucket").Sum(x => x.min_credits ?? 0);
    }

    private static Eligibility300To400Dto ComputeEligibility(StudentSnapshot snapshot)
    {
        var missing = new List<string>();
        if (snapshot.CompletedCredits100to200 < 100)
        {
            missing.Add("Chua du 100 tin chi level 100-200");
        }

        if (!snapshot.ConcentrationInfo.IsSelected)
        {
            missing.Add("Chua chon concentration");
        }

        if ((snapshot.Student.ielts_score ?? 0) < 6m && (snapshot.Student.english_level ?? 0) < 6)
        {
            missing.Add("Chua dat IELTS 6.0 hoac tuong duong");
        }

        return new Eligibility300To400Dto(missing.Count == 0, missing);
    }

    private static List<string> BuildWarnings(StudentSnapshot snapshot, Eligibility300To400Dto eligibility)
    {
        var warnings = new List<string>();
        if (!eligibility.IsEligible)
        {
            warnings.AddRange(eligibility.MissingReasons);
        }

        if (snapshot.Student.ielts_score is >= 6m && snapshot.CompletedCredits100to200 < 100)
        {
            warnings.Add("Co IELTS 6.0 nhung chua hoan tat stage 100-200");
        }

        return warnings.Distinct().ToList();
    }

    private static decimal ComputeCategoryEarnedCredits(List<curriculum_requirement> requirements, StudentSnapshot snapshot)
    {
        decimal total = 0;
        foreach (var req in requirements)
        {
            if (req.kind == "course" && !string.IsNullOrWhiteSpace(req.course_code))
            {
                var code = req.course_code.ToUpperInvariant();
                if (snapshot.CompletedCourseCodes.Contains(code))
                {
                    total += snapshot.LatestAttempts.FirstOrDefault(a => string.Equals(a.course_code, code, StringComparison.OrdinalIgnoreCase))?.credits ?? 0;
                }
            }

            if (req.kind == "credit_bucket")
            {
                var allowed = req.allowed_courses ?? [];
                foreach (var code in allowed.Select(x => x.ToUpperInvariant()))
                {
                    if (snapshot.CompletedCourseCodes.Contains(code))
                    {
                        total += snapshot.LatestAttempts.FirstOrDefault(a => string.Equals(a.course_code, code, StringComparison.OrdinalIgnoreCase))?.credits ?? 0;
                    }
                }
            }
        }

        return total;
    }

    private sealed record StudentSnapshot(
        student Student,
        program? Program,
        cohort? Cohort,
        IReadOnlyList<curriculum_requirement> Requirements,
        IReadOnlyDictionary<long, curriculum_category> Categories,
        IReadOnlyList<v_latest_attempt> LatestAttempts,
        HashSet<string> CompletedCourseCodes,
        List<string> RequiredCourseCodes,
        decimal TotalCompletedCredits,
        decimal CompletedCredits100to200,
        decimal CompletedCredits300to400,
        ConcentrationInfoDto ConcentrationInfo
    );
}
