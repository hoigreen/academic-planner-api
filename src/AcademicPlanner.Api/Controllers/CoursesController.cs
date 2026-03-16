using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Api.Controllers;

[ApiController]
[Route("api/v1/courses")]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CoursesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<CourseDto>>>> GetCourses(
        [FromQuery] CourseQueryDto queryDto,
        CancellationToken cancellationToken)
    {
        var query = _db.courses.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(queryDto.Prefix))
        {
            query = query.Where(x => x.subject_prefix == queryDto.Prefix.ToUpperInvariant());
        }

        if (queryDto.Level.HasValue)
        {
            query = query.Where(x => x.course_level == queryDto.Level.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Keyword))
        {
            var keyword = queryDto.Keyword.Trim();
            query = query.Where(x => x.course_code.Contains(keyword) || (x.course_name != null && x.course_name.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.ProgramCode))
        {
            var program = await _db.programs.AsNoTracking().FirstOrDefaultAsync(x => x.program_code == queryDto.ProgramCode.ToUpperInvariant(), cancellationToken);
            if (program is null)
            {
                return Ok(ApiEnvelope.Ok<IReadOnlyList<CourseDto>>([]));
            }

            var cohortIds = await _db.cohorts.AsNoTracking().Where(x => x.program_id == program.program_id).Select(x => x.cohort_id).ToListAsync(cancellationToken);
            var requiredCodes = await _db.curriculum_requirements.AsNoTracking()
                .Where(x => cohortIds.Contains(x.cohort_id) && x.course_code != null)
                .Select(x => x.course_code!)
                .Distinct()
                .ToListAsync(cancellationToken);
            query = query.Where(x => requiredCodes.Contains(x.course_code));
        }

        var courses = await query
            .OrderBy(x => x.course_code)
            .Select(x => new CourseDto(x.course_code, x.course_name, x.credits, x.subject_prefix, x.course_level))
            .ToListAsync(cancellationToken);

        return Ok(ApiEnvelope.Ok<IReadOnlyList<CourseDto>>(courses));
    }

    [HttpGet("{courseCode}")]
    public async Task<ActionResult<ApiEnvelope<CourseDto>>> GetCourse(string courseCode, CancellationToken cancellationToken)
    {
        var normalized = courseCode.ToUpperInvariant();
        var item = await _db.courses.AsNoTracking()
            .Where(x => x.course_code == normalized)
            .Select(x => new CourseDto(x.course_code, x.course_name, x.credits, x.subject_prefix, x.course_level))
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? NotFound() : Ok(ApiEnvelope.Ok(item));
    }

    [HttpGet("{courseCode}/prerequisites")]
    public async Task<ActionResult<ApiEnvelope<CoursePrerequisiteDto>>> GetCoursePrerequisites(string courseCode, CancellationToken cancellationToken)
    {
        var normalized = courseCode.ToUpperInvariant();
        var rules = await _db.curriculum_requirements.AsNoTracking()
            .Where(x => x.course_code == normalized && x.prereq_rule != null)
            .Select(x => x.prereq_rule!)
            .Distinct()
            .ToListAsync(cancellationToken);

        return Ok(ApiEnvelope.Ok(new CoursePrerequisiteDto(normalized, rules)));
    }

    [HttpGet("{courseCode}/equivalencies")]
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<CourseEquivalencyDto>>>> GetCourseEquivalencies(string courseCode, CancellationToken cancellationToken)
    {
        var normalized = courseCode.ToUpperInvariant();
        var rows = await _db.equivalencies.AsNoTracking()
            .Where(x => x.course_code == normalized || x.equivalent_course_code == normalized)
            .Select(x => new CourseEquivalencyDto(x.course_code, x.equivalent_course_code, x.cohort_id))
            .ToListAsync(cancellationToken);

        return Ok(ApiEnvelope.Ok<IReadOnlyList<CourseEquivalencyDto>>(rows));
    }
}
