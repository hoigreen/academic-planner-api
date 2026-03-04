using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AcademicPlanner.Api.Data;

namespace AcademicPlanner.Api.Controllers;

[ApiController]
[Route("api/programs")]
public class ProgramsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProgramsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var programs = await _db.Programs
            .AsNoTracking()
            .OrderBy(p => p.ProgramCode)
            .ToListAsync();

        return Ok(programs);
    }
}
