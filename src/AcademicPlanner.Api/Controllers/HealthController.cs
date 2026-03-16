using AcademicPlanner.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AcademicPlanner.Api.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet("/health/live")]
    public ActionResult<ApiEnvelope<HealthDto>> Live()
    {
        return Ok(ApiEnvelope.Ok(new HealthDto("live", DateTimeOffset.UtcNow)));
    }

    [HttpGet("/health/ready")]
    public ActionResult<ApiEnvelope<HealthDto>> Ready()
    {
        return Ok(ApiEnvelope.Ok(new HealthDto("ready", DateTimeOffset.UtcNow)));
    }
}
