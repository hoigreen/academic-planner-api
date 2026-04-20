using System.Text.Json;
using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Dtos;
using AcademicPlanner.Api.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Api.Services;

/// <summary>
/// Result returned after a successful Excel curriculum import.
/// </summary>
public record ExcelImportResult(
    long CurriculumId,
    string MajorName,
    string CohortCode,
    int BlocksImported,
    int CoursesImported,
    int RequirementsUpserted,
    IReadOnlyList<string> Warnings
);

public interface IExcelImportService
{
    /// <summary>
    /// Parses an uploaded .xlsx file and upserts it into acad.curricula + acad.curriculum_requirements.
    ///
    /// Expected workbook layout:
    ///   Sheet 1 "Blocks"   — columns: block_name | min_credits | is_mandatory | description
    ///   Sheet 2 "Courses"  — columns: block_name | course_code | is_required | prereq_rule_json
    ///   Sheet 3 "Meta"     — columns: key | value  (major_name, cohort_code, program_code rows)
    /// </summary>
    Task<ExcelImportResult> ImportAsync(Stream xlsxStream, CancellationToken cancellationToken = default);
}

public class ExcelImportService : IExcelImportService
{
    private readonly AppDbContext _db;

    public ExcelImportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ExcelImportResult> ImportAsync(Stream xlsxStream, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook(xlsxStream);
        var warnings = new List<string>();

        // ── Sheet 3: Meta ──────────────────────────────────────────────────
        var meta = ReadMetaSheet(workbook, warnings);
        var programCode = meta.GetValueOrDefault("program_code", "");
        var majorName   = meta.GetValueOrDefault("major_name", programCode);
        var cohortCode  = meta.GetValueOrDefault("cohort_code", "");

        if (string.IsNullOrWhiteSpace(programCode) || string.IsNullOrWhiteSpace(cohortCode))
            throw new InvalidOperationException("Sheet 'Meta' must contain rows for 'program_code' and 'cohort_code'.");

        // ── Resolve program + cohort ────────────────────────────────────────
        var program = await _db.programs.AsNoTracking()
            .FirstOrDefaultAsync(p => p.program_code == programCode, cancellationToken)
            ?? throw new InvalidOperationException($"Program '{programCode}' not found in the database.");

        var cohort = await _db.cohorts.AsNoTracking()
            .FirstOrDefaultAsync(c => c.program_id == program.program_id && c.cohort_code == cohortCode, cancellationToken)
            ?? throw new InvalidOperationException($"Cohort '{cohortCode}' not found for program '{programCode}'.");

        // ── Sheet 1: Blocks → knowledge_block[] ───────────────────────────
        var blocks = ReadBlocksSheet(workbook, warnings);

        // ── Sheet 2: Courses → course_mapping JSONB + requirements ─────────
        var (courseMapping, requirementRows) = ReadCoursesSheet(workbook, blocks, warnings);

        // ── Build ORDBMS data ──────────────────────────────────────────────
        var courseMappingJson = JsonSerializer.Serialize(courseMapping);
        var totalCredits = blocks.Sum(b => (decimal)b.MinCreditsRequired);

        // Upsert into acad.curricula
        var existing = await _db.curricula
            .FirstOrDefaultAsync(c => c.program_id == program.program_id && c.cohort_id == cohort.cohort_id, cancellationToken);

        long curriculumId;
        if (existing is null)
        {
            var newCurriculum = new curriculum
            {
                program_id    = program.program_id,
                cohort_id     = cohort.cohort_id,
                major_name    = majorName,
                cohort_code   = cohortCode,
                structure     = blocks.ToArray(),
                course_mapping = courseMappingJson,
                total_credits = totalCredits,
                created_at    = DateTime.UtcNow,
                updated_at    = DateTime.UtcNow,
            };
            _db.curricula.Add(newCurriculum);
            await _db.SaveChangesAsync(cancellationToken);
            curriculumId = newCurriculum.curriculum_id;
        }
        else
        {
            existing.structure      = blocks.ToArray();
            existing.course_mapping = courseMappingJson;
            existing.total_credits  = totalCredits;
            existing.major_name     = majorName;
            existing.updated_at     = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            curriculumId = existing.curriculum_id;
        }

        // ── Upsert curriculum categories ───────────────────────────────────
        var categoriesMap = await EnsureCategoriesAsync(program.program_id, blocks, warnings, cancellationToken);

        // ── Upsert curriculum_requirements ────────────────────────────────
        int requirementsUpserted = 0;
        foreach (var row in requirementRows)
        {
            if (!categoriesMap.TryGetValue(row.BlockName, out var categoryId))
            {
                warnings.Add($"Block '{row.BlockName}' for course '{row.CourseCode}' not found — skipping requirement.");
                continue;
            }

            var req = await _db.curriculum_requirements
                .FirstOrDefaultAsync(r =>
                    r.cohort_id == cohort.cohort_id &&
                    r.category_id == categoryId &&
                    r.course_code == row.CourseCode &&
                    r.kind == "course", cancellationToken);

            if (req is null)
            {
                _db.curriculum_requirements.Add(new curriculum_requirement
                {
                    cohort_id    = cohort.cohort_id,
                    category_id  = categoryId,
                    kind         = "course",
                    course_code  = row.CourseCode,
                    is_required  = row.IsRequired,
                    prereq_rule  = row.PrereqRuleJson,
                });
            }
            else
            {
                req.is_required = row.IsRequired;
                req.prereq_rule = row.PrereqRuleJson;
            }

            requirementsUpserted++;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new ExcelImportResult(
            curriculumId,
            majorName,
            cohortCode,
            blocks.Count,
            requirementRows.Count,
            requirementsUpserted,
            warnings
        );
    }

    // ────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ────────────────────────────────────────────────────────────────────────

    private static Dictionary<string, string> ReadMetaSheet(XLWorkbook wb, List<string> warnings)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!wb.TryGetWorksheet("Meta", out var sheet)) return result;

        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            var key   = row.Cell(1).GetValue<string>()?.Trim();
            var value = row.Cell(2).GetValue<string>()?.Trim();
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                result[key] = value;
        }
        return result;
    }

    private static List<KnowledgeBlock> ReadBlocksSheet(XLWorkbook wb, List<string> warnings)
    {
        var result = new List<KnowledgeBlock>();
        if (!wb.TryGetWorksheet("Blocks", out var sheet))
        {
            warnings.Add("Sheet 'Blocks' not found — no knowledge blocks imported.");
            return result;
        }

        // Expected columns: block_name | min_credits | is_mandatory | description
        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            var name    = row.Cell(1).GetValue<string>()?.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;

            int.TryParse(row.Cell(2).GetValue<string>(), out var minCredits);
            var isMandatory = ParseBool(row.Cell(3).GetValue<string>());
            var description = row.Cell(4).GetValue<string>()?.Trim();

            result.Add(new KnowledgeBlock
            {
                BlockName           = name,
                MinCreditsRequired  = minCredits,
                IsMandatory         = isMandatory,
                Description         = string.IsNullOrWhiteSpace(description) ? null : description,
            });
        }
        return result;
    }

    private record CourseRow(string BlockName, string CourseCode, bool IsRequired, string? PrereqRuleJson);

    private static (Dictionary<string, List<string>> courseMapping, List<CourseRow> requirements)
        ReadCoursesSheet(XLWorkbook wb, List<KnowledgeBlock> blocks, List<string> warnings)
    {
        var blockNames = blocks.Select(b => b.BlockName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var courseMapping = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var requirements  = new List<CourseRow>();

        if (!wb.TryGetWorksheet("Courses", out var sheet))
        {
            warnings.Add("Sheet 'Courses' not found — no courses imported.");
            return (courseMapping, requirements);
        }

        // Expected columns: block_name | course_code | is_required | prereq_rule_json
        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            var blockName  = row.Cell(1).GetValue<string>()?.Trim();
            var courseCode = row.Cell(2).GetValue<string>()?.Trim()?.ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(courseCode)) continue;

            if (!blockNames.Contains(blockName))
            {
                warnings.Add($"Course '{courseCode}' references unknown block '{blockName}' — skipping.");
                continue;
            }

            var isRequired  = ParseBool(row.Cell(3).GetValue<string>(), defaultValue: true);
            var prereqRaw   = row.Cell(4).GetValue<string>()?.Trim();
            string? prereqJson = null;

            if (!string.IsNullOrWhiteSpace(prereqRaw))
            {
                // Validate JSONB
                try
                {
                    JsonDocument.Parse(prereqRaw);
                    prereqJson = prereqRaw;
                }
                catch
                {
                    warnings.Add($"Course '{courseCode}': invalid prereq_rule JSON — stored as null.");
                }
            }

            if (!courseMapping.ContainsKey(blockName))
                courseMapping[blockName] = new List<string>();
            courseMapping[blockName].Add(courseCode);

            requirements.Add(new CourseRow(blockName, courseCode, isRequired, prereqJson));
        }
        return (courseMapping, requirements);
    }

    private async Task<Dictionary<string, long>> EnsureCategoriesAsync(
        long programId,
        List<KnowledgeBlock> blocks,
        List<string> warnings,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        int order = 1;

        foreach (var block in blocks)
        {
            var cat = await _db.curriculum_categories
                .FirstOrDefaultAsync(c => c.program_id == programId && c.category_name == block.BlockName, cancellationToken);

            if (cat is null)
            {
                cat = new curriculum_category
                {
                    program_id    = programId,
                    category_name = block.BlockName,
                    min_credits   = block.MinCreditsRequired,
                    sort_order    = order,
                };
                _db.curriculum_categories.Add(cat);
                await _db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                cat.min_credits = block.MinCreditsRequired;
                cat.sort_order  = order;
                await _db.SaveChangesAsync(cancellationToken);
            }

            result[block.BlockName] = cat.category_id;
            order++;
        }
        return result;
    }

    private static bool ParseBool(string? value, bool defaultValue = false)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        return value.Trim().ToLowerInvariant() is "true" or "yes" or "1" or "x";
    }
}
