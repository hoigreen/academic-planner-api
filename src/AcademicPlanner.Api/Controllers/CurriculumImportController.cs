using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AcademicPlanner.Api.Services;

namespace AcademicPlanner.Api.Controllers;

/// <summary>
/// Handles Excel-based curriculum import.
/// POST /api/v1/curriculum/import — Upload a .xlsx file to populate
/// acad.curricula (structure: knowledge_block[], course_mapping: JSONB)
/// and acad.curriculum_requirements (prereq_rule: JSONB).
/// </summary>
[ApiController]
[Route("api/v1/curriculum")]
public class CurriculumImportController : ControllerBase
{
    private readonly IExcelImportService _importService;
    private readonly ILogger<CurriculumImportController> _logger;

    public CurriculumImportController(IExcelImportService importService, ILogger<CurriculumImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    /// <summary>
    /// Import curriculum from an Excel workbook (.xlsx).
    ///
    /// Expected sheets:
    ///   - "Meta"    : rows key | value  (program_code, cohort_code, major_name)
    ///   - "Blocks"  : rows block_name | min_credits | is_mandatory | description
    ///   - "Courses" : rows block_name | course_code | is_required | prereq_rule_json
    ///
    /// On success the endpoint returns the curriculum_id, block count, and any warnings.
    /// </summary>
    [HttpPost("import")]
    [Authorize(Policy = "RequireAdmin")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Import(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file uploaded." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".xlsx")
            return BadRequest(new { success = false, message = "Only .xlsx files are supported." });

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _importService.ImportAsync(stream, cancellationToken);

            _logger.LogInformation(
                "Curriculum import succeeded: {MajorName}/{CohortCode}, blocks={Blocks}, courses={Courses}",
                result.MajorName, result.CohortCode, result.BlocksImported, result.CoursesImported);

            return Ok(new
            {
                success = true,
                message = $"Import complete for {result.MajorName} / {result.CohortCode}.",
                data = new
                {
                    result.CurriculumId,
                    result.MajorName,
                    result.CohortCode,
                    result.BlocksImported,
                    result.CoursesImported,
                    result.RequirementsUpserted,
                    result.Warnings,
                },
                errors = Array.Empty<string>(),
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message, errors = new[] { ex.Message } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Curriculum import failed");
            return StatusCode(500, new { success = false, message = "Import failed.", errors = new[] { ex.Message } });
        }
    }

    /// <summary>
    /// Download a blank import template as an in-memory .xlsx file.
    /// </summary>
    [HttpGet("import/template")]
    [AllowAnonymous]
    public IActionResult DownloadTemplate()
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();

        var meta = workbook.Worksheets.Add("Meta");
        meta.Cell(1, 1).Value = "key";
        meta.Cell(1, 2).Value = "value";
        meta.Cell(2, 1).Value = "program_code";
        meta.Cell(2, 2).Value = "BBS";
        meta.Cell(3, 1).Value = "cohort_code";
        meta.Cell(3, 2).Value = "K2025";
        meta.Cell(4, 1).Value = "major_name";
        meta.Cell(4, 2).Value = "Business Administration";

        var blocks = workbook.Worksheets.Add("Blocks");
        blocks.Cell(1, 1).Value = "block_name";
        blocks.Cell(1, 2).Value = "min_credits";
        blocks.Cell(1, 3).Value = "is_mandatory";
        blocks.Cell(1, 4).Value = "description";
        blocks.Cell(2, 1).Value = "General Foundation";
        blocks.Cell(2, 2).Value = 45;
        blocks.Cell(2, 3).Value = "true";
        blocks.Cell(2, 4).Value = "Foundation courses for all students";
        blocks.Cell(3, 1).Value = "Business Core";
        blocks.Cell(3, 2).Value = 60;
        blocks.Cell(3, 3).Value = "true";
        blocks.Cell(3, 4).Value = "Core business courses";

        var courses = workbook.Worksheets.Add("Courses");
        courses.Cell(1, 1).Value = "block_name";
        courses.Cell(1, 2).Value = "course_code";
        courses.Cell(1, 3).Value = "is_required";
        courses.Cell(1, 4).Value = "prereq_rule_json";
        courses.Cell(2, 1).Value = "General Foundation";
        courses.Cell(2, 2).Value = "WRT 122";
        courses.Cell(2, 3).Value = "true";
        courses.Cell(2, 4).Value = "";
        courses.Cell(3, 1).Value = "Business Core";
        courses.Cell(3, 2).Value = "BUS 101";
        courses.Cell(3, 3).Value = "true";
        courses.Cell(3, 4).Value = "";
        courses.Cell(4, 1).Value = "Business Core";
        courses.Cell(4, 2).Value = "BUS 303";
        courses.Cell(4, 3).Value = "true";
        courses.Cell(4, 4).Value = "{\"op\":\"AND\",\"args\":[{\"op\":\"COMPLETED\",\"course\":\"WRT 122\"},{\"op\":\"COMPLETED\",\"course\":\"BUS 101\"}]}";

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "curriculum_import_template.xlsx");
    }
}
