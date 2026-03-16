using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Api.Services;

public interface IPlanValidationService
{
    Task<PlanValidationResultDto> ValidateAsync(
        string studentId,
        int termCode,
        IReadOnlyList<string> courseCodes,
        int minCredits,
        int maxCredits,
        CancellationToken cancellationToken);
}

public class PlanValidationService : IPlanValidationService
{
    private readonly AppDbContext _db;
    private readonly IStudentAuditService _auditService;

    public PlanValidationService(AppDbContext db, IStudentAuditService auditService)
    {
        _db = db;
        _auditService = auditService;
    }

    public async Task<PlanValidationResultDto> ValidateAsync(
        string studentId,
        int termCode,
        IReadOnlyList<string> courseCodes,
        int minCredits,
        int maxCredits,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        var normalized = courseCodes.Select(x => x.Trim().ToUpperInvariant()).ToList();
        if (normalized.Count != normalized.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            errors.Add("duplicate_plan_item");
        }

        var courses = await _db.courses.AsNoTracking().Where(x => normalized.Contains(x.course_code)).ToListAsync(cancellationToken);
        var totalCredits = courses.Sum(x => x.credits ?? 0);
        if (totalCredits > maxCredits)
        {
            errors.Add("plan_credit_overload");
        }

        if (totalCredits < minCredits)
        {
            warnings.Add("Plan duoi nguong minCredits");
        }

        var audit = await _auditService.GetAuditSummaryAsync(studentId, cancellationToken);
        if (audit is not null && !audit.Eligibility300To400.IsEligible && courses.Any(x => x.course_level is >= 300))
        {
            errors.Add("stage_requirement_not_satisfied");
        }

        if (courses.Any(x => x.course_code.Equals("BUS 496", StringComparison.OrdinalIgnoreCase)) && normalized.Count > 3)
        {
            warnings.Add("Ban dang dang ky BUS 496, nen giam tai cac mon con lai");
        }

        return new PlanValidationResultDto(errors.Count == 0, totalCredits, errors, warnings);
    }
}
