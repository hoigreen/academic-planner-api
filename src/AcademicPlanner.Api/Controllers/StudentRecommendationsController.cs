using AcademicPlanner.Api.Dtos;
using AcademicPlanner.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcademicPlanner.Api.Controllers;

[ApiController]
[Route("api/v1/students/{studentId}/recommendations")]
public class StudentRecommendationsController : ControllerBase
{
    private readonly IRoadmapRecommendationService _recommendationService;
    private readonly IPlanValidationService _planValidationService;

    public StudentRecommendationsController(
        IRoadmapRecommendationService recommendationService,
        IPlanValidationService planValidationService)
    {
        _recommendationService = recommendationService;
        _planValidationService = planValidationService;
    }

    [HttpGet("next-term")]
    public async Task<ActionResult<ApiEnvelope<RecommendationResponseDto>>> GetNextTermRecommendations(
        string studentId,
        [FromQuery] NextTermRecommendationQueryDto query,
        CancellationToken cancellationToken)
    {
        if (query.TargetTermCode is null || !TermCodeValidator.IsValid(query.TargetTermCode.Value))
        {
            return BadRequest();
        }

        var response = await _recommendationService.GetNextTermAsync(
            studentId,
            query.TargetTermCode.Value,
            query.MinCredits,
            query.MaxCredits,
            query.Strategy,
            cancellationToken);
        return response is null ? NotFound() : Ok(ApiEnvelope.Ok(response));
    }

    [HttpPost("preview-plan")]
    public async Task<ActionResult<ApiEnvelope<object>>> PreviewPlan(
        string studentId,
        [FromBody] RecommendationPreviewPlanRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.TargetTermCode is null || !TermCodeValidator.IsValid(request.TargetTermCode.Value))
        {
            return BadRequest();
        }

        var recommendation = await _recommendationService.GetNextTermAsync(
            studentId,
            request.TargetTermCode.Value,
            request.MinCredits,
            request.MaxCredits,
            "balanced",
            cancellationToken);
        if (recommendation is null)
        {
            return NotFound();
        }

        var selectedCourses = request.CourseCodes?.Count > 0
            ? request.CourseCodes
            : recommendation.RecommendedCourses.Select(x => x.CourseCode).ToList();
        var validation = await _planValidationService.ValidateAsync(
            studentId,
            request.TargetTermCode.Value,
            selectedCourses.ToList(),
            request.MinCredits,
            request.MaxCredits,
            cancellationToken);

        return Ok(ApiEnvelope.Ok<object>(new { recommendation, validation }));
    }
}
