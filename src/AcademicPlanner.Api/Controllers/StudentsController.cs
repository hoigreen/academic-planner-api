using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Api.Controllers;

[ApiController]
[Route("api/v1/students")]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StudentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{studentId}")]
    public async Task<ActionResult<ApiEnvelope<StudentDto>>> GetStudent(string studentId, CancellationToken cancellationToken)
    {
        var student = await _db.students.AsNoTracking()
            .Include(x => x.program)
            .Include(x => x.cohort)
            .FirstOrDefaultAsync(x => x.student_id == studentId, cancellationToken);

        if (student is null)
        {
            return NotFound();
        }

        var dto = new StudentDto(
            student.student_id,
            BuildFullName(student.last_name, student.first_name, student.student_id),
            student.program_id,
            student.program?.program_code,
            student.cohort_id,
            student.cohort?.cohort_code,
            student.status,
            student.english_level,
            student.ielts_score
        );
        return Ok(ApiEnvelope.Ok(dto));
    }

    [HttpGet("{studentId}/transcript")]
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<TranscriptItemDto>>>> GetTranscript(string studentId, CancellationToken cancellationToken)
    {
        var exists = await _db.students.AsNoTracking().AnyAsync(x => x.student_id == studentId, cancellationToken);
        if (!exists)
        {
            return NotFound();
        }

        var rows = await _db.course_attempts.AsNoTracking()
            .Include(x => x.course_codeNavigation)
            .Where(x => x.student_id == studentId)
            .OrderBy(x => x.term_code).ThenBy(x => x.attempt_no)
            .Select(x => new TranscriptItemDto(
                x.attempt_id,
                x.course_code,
                x.course_codeNavigation.course_name,
                x.term_code,
                x.attempt_no,
                x.credits,
                x.grade_letter,
                x.is_completed
            ))
            .ToListAsync(cancellationToken);
        return Ok(ApiEnvelope.Ok<IReadOnlyList<TranscriptItemDto>>(rows));
    }

    [HttpGet("{studentId}/latest-attempts")]
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<LatestAttemptDto>>>> GetLatestAttempts(string studentId, CancellationToken cancellationToken)
    {
        var exists = await _db.students.AsNoTracking().AnyAsync(x => x.student_id == studentId, cancellationToken);
        if (!exists)
        {
            return NotFound();
        }

        var rows = await _db.v_latest_attempts.AsNoTracking()
            .Where(x => x.student_id == studentId)
            .OrderBy(x => x.course_code)
            .Select(x => new LatestAttemptDto(
                x.course_code!,
                x.term_code,
                x.attempt_no,
                x.credits,
                x.grade_letter,
                x.is_completed
            ))
            .ToListAsync(cancellationToken);
        return Ok(ApiEnvelope.Ok<IReadOnlyList<LatestAttemptDto>>(rows));
    }

    [HttpGet("{studentId}/profile-summary")]
    public async Task<ActionResult<ApiEnvelope<StudentProfileSummaryDto>>> GetProfileSummary(string studentId, CancellationToken cancellationToken)
    {
        var student = await _db.students.AsNoTracking()
            .Include(x => x.program)
            .Include(x => x.cohort)
            .FirstOrDefaultAsync(x => x.student_id == studentId, cancellationToken);
        if (student is null)
        {
            return NotFound();
        }

        var latest = await _db.v_latest_attempts.AsNoTracking()
            .Where(x => x.student_id == studentId)
            .ToListAsync(cancellationToken);

        var credits = latest.Where(x => x.is_completed == true || (x.credits ?? 0) > 0).Sum(x => x.credits ?? 0);
        var gpa = await _db.course_attempts.AsNoTracking()
            .Where(x => x.student_id == studentId && x.snapshot_cum_gpa.HasValue)
            .OrderByDescending(x => x.term_code)
            .ThenByDescending(x => x.attempt_no)
            .Select(x => x.snapshot_cum_gpa)
            .FirstOrDefaultAsync(cancellationToken);
        var lastTerm = latest.Where(x => x.term_code.HasValue).Select(x => x.term_code!.Value).DefaultIfEmpty().Max();

        var dto = new StudentProfileSummaryDto(
            studentId,
            student.program?.program_code,
            student.cohort?.cohort_code,
            credits,
            gpa,
            student.english_level,
            student.ielts_score,
            lastTerm == 0 ? null : lastTerm
        );
        return Ok(ApiEnvelope.Ok(dto));
    }

    private static string BuildFullName(string? lastName, string? firstName, string fallback)
    {
        var fullName = string.Join(" ", new[] { lastName, firstName }.Where(x => !string.IsNullOrWhiteSpace(x)));
        return string.IsNullOrWhiteSpace(fullName) ? fallback : fullName;
    }
}
