using AcademicPlanner.Api.Dtos;
using AcademicPlanner.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcademicPlanner.Api.Controllers;

[ApiController]
[Route("api/v1/students/{studentId}/audit")]
public class StudentAuditController : ControllerBase
{
    private readonly IStudentAuditService _auditService;

    public StudentAuditController(IStudentAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiEnvelope<StudentAuditDto>>> GetAudit(string studentId, CancellationToken cancellationToken)
    {
        var data = await _auditService.GetAuditAsync(studentId, cancellationToken);
        return data is null ? NotFound() : Ok(ApiEnvelope.Ok(data));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiEnvelope<StudentAuditSummaryDto>>> GetAuditSummary(string studentId, CancellationToken cancellationToken)
    {
        var data = await _auditService.GetAuditSummaryAsync(studentId, cancellationToken);
        return data is null ? NotFound() : Ok(ApiEnvelope.Ok(data));
    }

    [HttpGet("missing-courses")]
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<string>>>> GetMissingCourses(string studentId, CancellationToken cancellationToken)
    {
        var data = await _auditService.GetMissingCoursesAsync(studentId, cancellationToken);
        return data is null ? NotFound() : Ok(ApiEnvelope.Ok(data));
    }

    [HttpGet("eligibility-300-400")]
    public async Task<ActionResult<ApiEnvelope<Eligibility300To400Dto>>> GetEligibility300To400(string studentId, CancellationToken cancellationToken)
    {
        var data = await _auditService.GetEligibility300To400Async(studentId, cancellationToken);
        return data is null ? NotFound() : Ok(ApiEnvelope.Ok(data));
    }

    [HttpGet("progress-by-category")]
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<CategoryProgressDto>>>> GetProgressByCategory(string studentId, CancellationToken cancellationToken)
    {
        var data = await _auditService.GetProgressByCategoryAsync(studentId, cancellationToken);
        return data is null ? NotFound() : Ok(ApiEnvelope.Ok(data));
    }
}
