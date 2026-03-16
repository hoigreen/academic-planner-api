using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Dtos;
using AcademicPlanner.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class ConcentrationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ConcentrationsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("programs/{programCode}/concentrations")]
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<ConcentrationDto>>>> GetProgramConcentrations(string programCode, CancellationToken cancellationToken)
    {
        var program = await _db.programs.AsNoTracking().FirstOrDefaultAsync(x => x.program_code == programCode.ToUpperInvariant(), cancellationToken);
        if (program is null)
        {
            return NotFound();
        }

        var rows = await _db.concentrations.AsNoTracking()
            .Where(x => x.program_id == program.program_id)
            .OrderBy(x => x.concentration_code)
            .Select(x => new ConcentrationDto(x.concentration_id, x.concentration_code, x.concentration_name, x.min_credits))
            .ToListAsync(cancellationToken);
        return Ok(ApiEnvelope.Ok<IReadOnlyList<ConcentrationDto>>(rows));
    }

    [HttpGet("students/{studentId}/concentration")]
    public async Task<ActionResult<ApiEnvelope<StudentConcentrationDto>>> GetStudentConcentration(string studentId, CancellationToken cancellationToken)
    {
        var row = await _db.student_concentrations.AsNoTracking()
            .Include(x => x.concentration)
            .Where(x => x.student_id == studentId && x.status == "active")
            .OrderByDescending(x => x.created_at)
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return NotFound();
        }

        return Ok(ApiEnvelope.Ok(ToDto(row)));
    }

    [HttpPost("students/{studentId}/concentration")]
    public async Task<ActionResult<ApiEnvelope<StudentConcentrationDto>>> AssignStudentConcentration(
        string studentId,
        [FromBody] AssignStudentConcentrationRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.ConcentrationId is null)
        {
            return BadRequest();
        }

        var entity = new student_concentration
        {
            student_id = studentId,
            concentration_id = request.ConcentrationId.Value,
            approved_term_code = request.ApprovedTermCode,
            status = "active"
        };
        _db.student_concentrations.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var created = await _db.student_concentrations.AsNoTracking()
            .Include(x => x.concentration)
            .FirstAsync(x => x.student_concentration_id == entity.student_concentration_id, cancellationToken);
        return Ok(ApiEnvelope.Ok(ToDto(created)));
    }

    [HttpPatch("students/{studentId}/concentration")]
    public async Task<ActionResult<ApiEnvelope<StudentConcentrationDto>>> UpdateStudentConcentration(
        string studentId,
        [FromBody] AssignStudentConcentrationRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.ConcentrationId is null)
        {
            return BadRequest();
        }

        var activeRows = await _db.student_concentrations
            .Where(x => x.student_id == studentId && x.status == "active")
            .ToListAsync(cancellationToken);
        foreach (var row in activeRows)
        {
            row.status = "inactive";
        }

        _db.student_concentrations.Add(new student_concentration
        {
            student_id = studentId,
            concentration_id = request.ConcentrationId.Value,
            approved_term_code = request.ApprovedTermCode,
            status = "active"
        });
        await _db.SaveChangesAsync(cancellationToken);

        var latest = await _db.student_concentrations.AsNoTracking()
            .Include(x => x.concentration)
            .Where(x => x.student_id == studentId && x.status == "active")
            .OrderByDescending(x => x.created_at)
            .FirstAsync(cancellationToken);
        return Ok(ApiEnvelope.Ok(ToDto(latest)));
    }

    private static StudentConcentrationDto ToDto(student_concentration row)
    {
        return new StudentConcentrationDto(
            row.student_id,
            row.concentration_id,
            row.concentration.concentration_code,
            row.concentration.concentration_name,
            row.approved_term_code,
            row.status
        );
    }
}
