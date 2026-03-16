using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Dtos;
using AcademicPlanner.Api.Models;
using AcademicPlanner.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Api.Controllers;

[ApiController]
[Route("api/v1/students/{studentId}/plans")]
public class StudentPlansController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPlanValidationService _planValidationService;

    public StudentPlansController(AppDbContext db, IPlanValidationService planValidationService)
    {
        _db = db;
        _planValidationService = planValidationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<PlanByTermDto>>>> GetPlans(string studentId, CancellationToken cancellationToken)
    {
        var rows = await _db.student_plans.AsNoTracking()
            .Include(x => x.course_codeNavigation)
            .Where(x => x.student_id == studentId)
            .OrderBy(x => x.term_code).ThenBy(x => x.plan_id)
            .ToListAsync(cancellationToken);
        if (rows.Count == 0)
        {
            var exists = await _db.students.AsNoTracking().AnyAsync(x => x.student_id == studentId, cancellationToken);
            if (!exists)
            {
                return NotFound();
            }
        }

        return Ok(ApiEnvelope.Ok<IReadOnlyList<PlanByTermDto>>(GroupByTerm(studentId, rows)));
    }

    [HttpGet("{termCode:int}")]
    public async Task<ActionResult<ApiEnvelope<PlanByTermDto>>> GetPlanByTerm(string studentId, int termCode, CancellationToken cancellationToken)
    {
        var rows = await _db.student_plans.AsNoTracking()
            .Include(x => x.course_codeNavigation)
            .Where(x => x.student_id == studentId && x.term_code == termCode)
            .OrderBy(x => x.plan_id)
            .ToListAsync(cancellationToken);

        return rows.Count == 0 ? NotFound() : Ok(ApiEnvelope.Ok(ToPlanByTerm(studentId, termCode, rows)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiEnvelope<PlanByTermDto>>> CreatePlan(
        string studentId,
        [FromBody] CreatePlanRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.TermCode is null || !TermCodeValidator.IsValid(request.TermCode.Value))
        {
            return BadRequest();
        }

        foreach (var code in request.CourseCodes.Select(x => x.Trim().ToUpperInvariant()).Distinct())
        {
            _db.student_plans.Add(new student_plan
            {
                student_id = studentId,
                term_code = request.TermCode.Value,
                course_code = code,
                status = "planned",
                note = null
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        var rows = await _db.student_plans.AsNoTracking()
            .Include(x => x.course_codeNavigation)
            .Where(x => x.student_id == studentId && x.term_code == request.TermCode.Value)
            .OrderBy(x => x.plan_id)
            .ToListAsync(cancellationToken);

        return Ok(ApiEnvelope.Ok(ToPlanByTerm(studentId, request.TermCode.Value, rows)));
    }

    [HttpPost("{termCode:int}/items")]
    public async Task<ActionResult<ApiEnvelope<PlanItemDto>>> AddPlanItem(
        string studentId,
        int termCode,
        [FromBody] AddPlanItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var row = new student_plan
        {
            student_id = studentId,
            term_code = termCode,
            course_code = request.CourseCode.Trim().ToUpperInvariant(),
            status = request.Status?.Trim().ToLowerInvariant() ?? "planned",
            note = request.Note
        };
        _db.student_plans.Add(row);
        await _db.SaveChangesAsync(cancellationToken);

        var full = await _db.student_plans.AsNoTracking().Include(x => x.course_codeNavigation)
            .FirstAsync(x => x.plan_id == row.plan_id, cancellationToken);
        return Ok(ApiEnvelope.Ok(ToPlanItem(full)));
    }

    [HttpPatch("{termCode:int}/items/{planId:long}")]
    public async Task<ActionResult<ApiEnvelope<PlanItemDto>>> UpdatePlanItem(
        string studentId,
        int termCode,
        long planId,
        [FromBody] UpdatePlanItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var row = await _db.student_plans
            .Include(x => x.course_codeNavigation)
            .FirstOrDefaultAsync(x => x.plan_id == planId && x.student_id == studentId && x.term_code == termCode, cancellationToken);
        if (row is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            row.status = request.Status.Trim().ToLowerInvariant();
        }

        row.note = request.Note ?? row.note;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(ApiEnvelope.Ok(ToPlanItem(row)));
    }

    [HttpDelete("{termCode:int}/items/{planId:long}")]
    public async Task<IActionResult> DeletePlanItem(string studentId, int termCode, long planId, CancellationToken cancellationToken)
    {
        var row = await _db.student_plans.FirstOrDefaultAsync(x => x.plan_id == planId && x.student_id == studentId && x.term_code == termCode, cancellationToken);
        if (row is null)
        {
            return NotFound();
        }

        _db.student_plans.Remove(row);
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(ApiEnvelope.Ok<object>(new { deleted = true }));
    }

    [HttpPost("{termCode:int}/validate")]
    public async Task<ActionResult<ApiEnvelope<PlanValidationResultDto>>> ValidatePlan(
        string studentId,
        int termCode,
        CancellationToken cancellationToken)
    {
        var courses = await _db.student_plans.AsNoTracking()
            .Where(x => x.student_id == studentId && x.term_code == termCode)
            .Select(x => x.course_code)
            .ToListAsync(cancellationToken);
        var result = await _planValidationService.ValidateAsync(studentId, termCode, courses, 16, 20, cancellationToken);
        return Ok(ApiEnvelope.Ok(result));
    }

    private static List<PlanByTermDto> GroupByTerm(string studentId, List<student_plan> rows)
    {
        return rows.GroupBy(x => x.term_code)
            .Select(group => ToPlanByTerm(studentId, group.Key, group.ToList()))
            .OrderBy(x => x.TermCode)
            .ToList();
    }

    private static PlanByTermDto ToPlanByTerm(string studentId, int termCode, List<student_plan> rows)
    {
        return new PlanByTermDto(studentId, termCode, rows.Select(ToPlanItem).ToList());
    }

    private static PlanItemDto ToPlanItem(student_plan row)
    {
        return new PlanItemDto(row.plan_id, row.course_code, row.term_code, row.status, row.note, row.course_codeNavigation?.credits);
    }
}
