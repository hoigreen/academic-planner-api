using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Api.Controllers;

[ApiController]
[Route("api/v1/programs")]
public class ProgramsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProgramsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<ProgramDto>>>> GetPrograms(CancellationToken cancellationToken)
    {
        var data = await _db.programs.AsNoTracking()
            .OrderBy(x => x.program_code)
            .Select(x => new ProgramDto(x.program_id, x.program_code, x.program_name, x.degree_level, x.default_target_credits))
            .ToListAsync(cancellationToken);

        return Ok(ApiEnvelope.Ok<IReadOnlyList<ProgramDto>>(data));
    }

    [HttpGet("{programCode}")]
    public async Task<ActionResult<ApiEnvelope<ProgramDto>>> GetProgram(string programCode, CancellationToken cancellationToken)
    {
        var item = await _db.programs.AsNoTracking()
            .Where(x => x.program_code == programCode.ToUpperInvariant())
            .Select(x => new ProgramDto(x.program_id, x.program_code, x.program_name, x.degree_level, x.default_target_credits))
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? NotFound() : Ok(ApiEnvelope.Ok(item));
    }

    [HttpGet("{programCode}/cohorts")]
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<CohortDto>>>> GetCohorts(string programCode, CancellationToken cancellationToken)
    {
        var program = await _db.programs.AsNoTracking().FirstOrDefaultAsync(x => x.program_code == programCode.ToUpperInvariant(), cancellationToken);
        if (program is null)
        {
            return NotFound();
        }

        var cohorts = await _db.cohorts.AsNoTracking()
            .Where(x => x.program_id == program.program_id)
            .OrderBy(x => x.cohort_code)
            .Select(x => new CohortDto(x.cohort_id, x.cohort_code, x.start_year))
            .ToListAsync(cancellationToken);

        return Ok(ApiEnvelope.Ok<IReadOnlyList<CohortDto>>(cohorts));
    }

    [HttpGet("{programCode}/curriculum")]
    public async Task<ActionResult<ApiEnvelope<object>>> GetCurriculum(string programCode, CancellationToken cancellationToken)
    {
        var program = await _db.programs.AsNoTracking().FirstOrDefaultAsync(x => x.program_code == programCode.ToUpperInvariant(), cancellationToken);
        if (program is null)
        {
            return NotFound();
        }

        var categories = await _db.curriculum_categories.AsNoTracking()
            .Where(x => x.program_id == program.program_id)
            .OrderBy(x => x.sort_order)
            .Select(x => new { x.category_id, x.category_name, x.min_credits, x.sort_order })
            .ToListAsync(cancellationToken);

        return Ok(ApiEnvelope.Ok<object>(new
        {
            programCode = program.program_code,
            categories
        }));
    }

    [HttpGet("{programCode}/courses")]
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<CourseDto>>>> GetProgramCourses(string programCode, CancellationToken cancellationToken)
    {
        var program = await _db.programs.AsNoTracking().FirstOrDefaultAsync(x => x.program_code == programCode.ToUpperInvariant(), cancellationToken);
        if (program is null)
        {
            return NotFound();
        }

        var cohortIds = await _db.cohorts.AsNoTracking()
            .Where(x => x.program_id == program.program_id)
            .Select(x => x.cohort_id)
            .ToListAsync(cancellationToken);

        var requirements = await _db.curriculum_requirements.AsNoTracking()
            .Where(x => cohortIds.Contains(x.cohort_id))
            .ToListAsync(cancellationToken);

        var requiredCodes = requirements
            .SelectMany(x =>
            {
                var codes = new List<string>();
                if (!string.IsNullOrWhiteSpace(x.course_code))
                {
                    codes.Add(x.course_code);
                }

                if (x.allowed_courses is not null && x.allowed_courses.Length > 0)
                {
                    codes.AddRange(x.allowed_courses.Where(code => !string.IsNullOrWhiteSpace(code)));
                }

                return codes;
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var courses = await _db.courses.AsNoTracking()
            .Where(x => requiredCodes.Contains(x.course_code))
            .OrderBy(x => x.course_code)
            .Select(x => new CourseDto(x.course_code, x.course_name, x.credits, x.subject_prefix, x.course_level))
            .ToListAsync(cancellationToken);

        return Ok(ApiEnvelope.Ok<IReadOnlyList<CourseDto>>(courses));
    }
}
