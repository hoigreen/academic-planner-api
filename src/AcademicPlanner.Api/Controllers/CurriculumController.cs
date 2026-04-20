using System.Text.Json;
using AcademicPlanner.Api.Data;
using AcademicPlanner.Api.Dtos;
using AcademicPlanner.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Api.Controllers;

[ApiController]
[Route("api/v1/curriculum")]
public class CurriculumController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPrerequisiteEvaluator _prerequisiteEvaluator;

    public CurriculumController(AppDbContext db, IPrerequisiteEvaluator prerequisiteEvaluator)
    {
        _db = db;
        _prerequisiteEvaluator = prerequisiteEvaluator;
    }

    [HttpGet("{programCode}/{cohortCode}")]
    public async Task<ActionResult<ApiEnvelope<CurriculumStructureDto>>> GetStructure(
        string programCode, string cohortCode, CancellationToken cancellationToken)
    {
        var program = await _db.programs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.program_code == programCode.ToUpperInvariant(), cancellationToken);
        if (program is null)
            return NotFound(ApiEnvelope.Fail<CurriculumStructureDto>("Program not found"));

        var cohort = await _db.cohorts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.program_id == program.program_id && x.cohort_code == cohortCode, cancellationToken);
        if (cohort is null)
            return NotFound(ApiEnvelope.Fail<CurriculumStructureDto>("Cohort not found"));

        var curricula = await _db.curricula.AsNoTracking()
            .FirstOrDefaultAsync(x => x.program_id == program.program_id && x.cohort_id == cohort.cohort_id, cancellationToken);

        var knowledgeBlocks = new List<KnowledgeBlockDto>();
        var courseMapping = new Dictionary<string, List<string>>();

        if (curricula is not null)
        {
            courseMapping = ParseCourseMapping(curricula.course_mapping);
        }

        var categories = await _db.curriculum_categories.AsNoTracking()
            .Where(x => x.program_id == program.program_id)
            .OrderBy(x => x.sort_order)
            .ToListAsync(cancellationToken);

        var requirements = await _db.curriculum_requirements.AsNoTracking()
            .Where(x => x.cohort_id == cohort.cohort_id)
            .ToListAsync(cancellationToken);

        var courseCodes = requirements
            .SelectMany(r =>
            {
                var codes = new List<string>();
                if (!string.IsNullOrWhiteSpace(r.course_code)) codes.Add(r.course_code);
                if (r.allowed_courses is not null) codes.AddRange(r.allowed_courses);
                return codes;
            })
            .Distinct()
            .ToList();

        var courseMap = await _db.courses.AsNoTracking()
            .Where(x => courseCodes.Contains(x.course_code))
            .ToDictionaryAsync(x => x.course_code, cancellationToken);

        var categoryDetails = new List<CurriculumCategoryDetailDto>();
        foreach (var cat in categories)
        {
            var catReqs = requirements.Where(x => x.category_id == cat.category_id).ToList();
            var courses = new List<CurriculumCourseDetailDto>();

            foreach (var req in catReqs)
            {
                if (!string.IsNullOrWhiteSpace(req.course_code) && courseMap.TryGetValue(req.course_code, out var course))
                {
                    courses.Add(new CurriculumCourseDetailDto(
                        req.course_code,
                        course.course_name,
                        course.credits,
                        req.is_required,
                        req.prereq_rule
                    ));
                }

                if (req.allowed_courses is not null)
                {
                    foreach (var code in req.allowed_courses)
                    {
                        if (courseMap.TryGetValue(code, out var elective) && courses.All(c => c.CourseCode != code))
                        {
                            courses.Add(new CurriculumCourseDetailDto(
                                code,
                                elective.course_name,
                                elective.credits,
                                false,
                                req.prereq_rule
                            ));
                        }
                    }
                }
            }

            knowledgeBlocks.Add(new KnowledgeBlockDto(
                cat.category_name,
                (int)(cat.min_credits ?? 0),
                catReqs.Any(r => r.is_required),
                null
            ));

            if (!courseMapping.ContainsKey(cat.category_name))
            {
                courseMapping[cat.category_name] = courses.Select(c => c.CourseCode).ToList();
            }

            categoryDetails.Add(new CurriculumCategoryDetailDto(
                cat.category_id,
                cat.category_name,
                cat.min_credits,
                cat.sort_order,
                courses
            ));
        }

        var result = new CurriculumStructureDto(
            curricula?.curriculum_id ?? 0,
            program.program_code,
            program.program_name,
            cohort.cohort_code,
            curricula?.total_credits ?? categories.Sum(c => c.min_credits ?? 0),
            knowledgeBlocks,
            courseMapping,
            categoryDetails
        );

        return Ok(ApiEnvelope.Ok(result));
    }

    [HttpGet("students/{studentId}/eligible-courses")]
    public async Task<ActionResult<ApiEnvelope<EligibleCoursesResponseDto>>> GetEligibleCourses(
        string studentId, CancellationToken cancellationToken)
    {
        var student = await _db.students.AsNoTracking()
            .Include(x => x.program)
            .Include(x => x.cohort)
            .FirstOrDefaultAsync(x => x.student_id == studentId, cancellationToken);

        if (student is null)
            return NotFound(ApiEnvelope.Fail<EligibleCoursesResponseDto>("Student not found"));

        if (!student.cohort_id.HasValue)
            return Ok(ApiEnvelope.Ok(new EligibleCoursesResponseDto(studentId, [], 0)));

        var latestAttempts = await _db.v_latest_attempts.AsNoTracking()
            .Where(x => x.student_id == studentId)
            .ToListAsync(cancellationToken);

        var completedCodes = latestAttempts
            .Where(a => a.is_completed == true || (a.credits ?? 0) > 0)
            .Select(a => a.course_code!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var prereqContext = new PrerequisiteContext(completedCodes, student.english_level, student.ielts_score);

        var requirements = await _db.curriculum_requirements.AsNoTracking()
            .Where(x => x.cohort_id == student.cohort_id.Value)
            .ToListAsync(cancellationToken);

        var categories = await _db.curriculum_categories.AsNoTracking()
            .Where(x => student.program_id.HasValue && x.program_id == student.program_id.Value)
            .ToDictionaryAsync(x => x.category_id, cancellationToken);

        var allCourseCodes = requirements
            .SelectMany(r =>
            {
                var codes = new List<string>();
                if (!string.IsNullOrWhiteSpace(r.course_code)) codes.Add(r.course_code);
                if (r.allowed_courses is not null) codes.AddRange(r.allowed_courses);
                return codes;
            })
            .Distinct()
            .ToList();

        var courseMap = await _db.courses.AsNoTracking()
            .Where(x => allCourseCodes.Contains(x.course_code))
            .ToDictionaryAsync(x => x.course_code, cancellationToken);

        var eligible = new List<EligibleCourseDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var req in requirements)
        {
            var codesToCheck = new List<(string code, bool isRequired)>();
            if (!string.IsNullOrWhiteSpace(req.course_code))
                codesToCheck.Add((req.course_code, req.is_required));
            if (req.allowed_courses is not null)
                codesToCheck.AddRange(req.allowed_courses.Select(c => (c, false)));

            foreach (var (code, isRequired) in codesToCheck)
            {
                if (completedCodes.Contains(code) || !seen.Add(code)) continue;

                var evalResult = _prerequisiteEvaluator.Evaluate(req.prereq_rule, prereqContext);
                var categoryName = categories.TryGetValue(req.category_id, out var cat) ? cat.category_name : null;

                courseMap.TryGetValue(code, out var course);
                eligible.Add(new EligibleCourseDto(
                    code,
                    course?.course_name,
                    course?.credits,
                    categoryName,
                    isRequired,
                    evalResult.IsSatisfied,
                    evalResult.BlockingReasons
                ));
            }
        }

        var result = new EligibleCoursesResponseDto(
            studentId,
            eligible.OrderByDescending(x => x.PrerequisitesMet).ThenByDescending(x => x.IsRequired).ThenBy(x => x.CourseCode).ToList(),
            eligible.Count(x => x.PrerequisitesMet)
        );

        return Ok(ApiEnvelope.Ok(result));
    }

    [HttpGet("students/search")]
    public async Task<ActionResult<ApiEnvelope<StudentSearchResultDto>>> SearchStudents(
        [FromQuery] StudentSearchQueryDto query, CancellationToken cancellationToken)
    {
        var q = _db.students.AsNoTracking()
            .Include(x => x.program)
            .Include(x => x.cohort)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.StudentId))
            q = q.Where(x => x.student_id.Contains(query.StudentId));

        if (!string.IsNullOrWhiteSpace(query.ProgramCode))
            q = q.Where(x => x.program != null && x.program.program_code == query.ProgramCode.ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(query.CohortCode))
            q = q.Where(x => x.cohort != null && x.cohort.cohort_code == query.CohortCode);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.ToLower();
            q = q.Where(x =>
                (x.first_name != null && x.first_name.ToLower().Contains(kw)) ||
                (x.last_name != null && x.last_name.ToLower().Contains(kw)) ||
                x.student_id.Contains(kw));
        }

        var total = await q.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var students = await q
            .OrderBy(x => x.student_id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StudentDto(
                x.student_id,
                string.Join(" ", new[] { x.last_name, x.first_name }.Where(s => s != null)),
                x.program_id,
                x.program != null ? x.program.program_code : null,
                x.cohort_id,
                x.cohort != null ? x.cohort.cohort_code : null,
                x.status,
                x.english_level,
                x.ielts_score
            ))
            .ToListAsync(cancellationToken);

        return Ok(ApiEnvelope.Ok(new StudentSearchResultDto(students, total, page, pageSize)));
    }

    private static Dictionary<string, List<string>> ParseCourseMapping(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? new();
        }
        catch (JsonException)
        {
            return new();
        }
    }
}
