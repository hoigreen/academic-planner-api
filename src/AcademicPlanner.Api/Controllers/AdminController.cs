using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Dtos;
using AcademicPlanner.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequestDto request, CancellationToken cancellationToken)
    {
        var entity = new course
        {
            course_code = request.CourseCode.Trim().ToUpperInvariant(),
            course_name = request.CourseName,
            credits = request.Credits,
            subject_prefix = request.SubjectPrefix,
            course_level = request.CourseLevel,
            meta = "{}"
        };
        _db.courses.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(ApiEnvelope.Ok<object>(new { entity.course_code }));
    }

    [HttpPost("curriculum-requirements")]
    public async Task<IActionResult> CreateRequirement([FromBody] CreateCurriculumRequirementRequestDto request, CancellationToken cancellationToken)
    {
        if (request.CohortId is null || request.CategoryId is null)
        {
            return BadRequest();
        }

        var entity = new curriculum_requirement
        {
            cohort_id = request.CohortId.Value,
            category_id = request.CategoryId.Value,
            kind = request.Kind.Trim().ToLowerInvariant(),
            course_code = request.CourseCode?.Trim().ToUpperInvariant(),
            min_credits = request.MinCredits,
            is_required = request.IsRequired,
            allowed_courses = request.AllowedCourses?.Select(x => x.Trim().ToUpperInvariant()).ToArray(),
            prereq_rule = request.PrereqRule
        };
        _db.curriculum_requirements.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(ApiEnvelope.Ok<object>(new { entity.requirement_id }));
    }

    [HttpPost("course-offerings")]
    public async Task<IActionResult> CreateOffering([FromBody] CreateCourseOfferingRequestDto request, CancellationToken cancellationToken)
    {
        if (request.TermCode is null)
        {
            return BadRequest();
        }

        var entity = new course_offering
        {
            term_code = request.TermCode.Value,
            course_code = request.CourseCode.Trim().ToUpperInvariant(),
            is_open = request.IsOpen,
            registration_channel = request.RegistrationChannel,
            meta = "{}"
        };
        _db.course_offerings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(ApiEnvelope.Ok<object>(new { entity.offering_id }));
    }

    [HttpPost("concentrations")]
    public async Task<IActionResult> CreateConcentration([FromBody] CreateConcentrationRequestDto request, CancellationToken cancellationToken)
    {
        var program = await _db.programs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.program_code == request.ProgramCode.ToUpperInvariant(), cancellationToken);
        if (program is null)
        {
            return NotFound();
        }

        var entity = new concentration
        {
            program_id = program.program_id,
            concentration_code = request.ConcentrationCode.Trim().ToUpperInvariant(),
            concentration_name = request.ConcentrationName.Trim(),
            min_credits = request.MinCredits,
            meta = "{}"
        };
        _db.concentrations.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(ApiEnvelope.Ok<object>(new { entity.concentration_id }));
    }

    [HttpPost("concentration-courses")]
    public async Task<IActionResult> CreateConcentrationCourse([FromBody] CreateConcentrationCourseRequestDto request, CancellationToken cancellationToken)
    {
        if (request.ConcentrationId is null)
        {
            return BadRequest();
        }

        var entity = new concentration_course
        {
            concentration_id = request.ConcentrationId.Value,
            course_code = request.CourseCode.Trim().ToUpperInvariant(),
            is_required = request.IsRequired,
            is_entry_course = request.IsEntryCourse,
            meta = "{}"
        };
        _db.concentration_courses.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(ApiEnvelope.Ok<object>(new { entity.concentration_course_id }));
    }

    [HttpPost("students/import-attempts")]
    public async Task<IActionResult> ImportAttempts([FromBody] ImportCourseAttemptRequestDto request, CancellationToken cancellationToken)
    {
        if (request.TermCode is null)
        {
            return BadRequest();
        }

        var row = new course_attempt
        {
            student_id = request.StudentId,
            course_code = request.CourseCode.Trim().ToUpperInvariant(),
            term_code = request.TermCode.Value,
            attempt_no = request.AttemptNo,
            credits = request.Credits,
            grade_letter = request.GradeLetter,
            is_completed = request.IsCompleted,
            raw_record = "{}"
        };
        _db.course_attempts.Add(row);
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(ApiEnvelope.Ok<object>(new { row.attempt_id }));
    }
}
