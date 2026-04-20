using System.Text.Json.Serialization;

namespace AcademicPlanner.Api.Dtos;

public record KnowledgeBlockDto(
    [property: JsonPropertyName("blockName")] string BlockName,
    [property: JsonPropertyName("minCreditsRequired")] int MinCreditsRequired,
    [property: JsonPropertyName("isMandatory")] bool IsMandatory,
    [property: JsonPropertyName("description")] string? Description
);

public record CurriculumStructureDto(
    [property: JsonPropertyName("curriculumId")] long CurriculumId,
    [property: JsonPropertyName("programCode")] string ProgramCode,
    [property: JsonPropertyName("programName")] string ProgramName,
    [property: JsonPropertyName("cohortCode")] string CohortCode,
    [property: JsonPropertyName("totalCredits")] decimal? TotalCredits,
    [property: JsonPropertyName("knowledgeBlocks")] IReadOnlyList<KnowledgeBlockDto> KnowledgeBlocks,
    [property: JsonPropertyName("courseMapping")] Dictionary<string, List<string>> CourseMapping,
    [property: JsonPropertyName("categories")] IReadOnlyList<CurriculumCategoryDetailDto> Categories
);

public record CurriculumCategoryDetailDto(
    [property: JsonPropertyName("categoryId")] long CategoryId,
    [property: JsonPropertyName("categoryName")] string CategoryName,
    [property: JsonPropertyName("minCredits")] decimal? MinCredits,
    [property: JsonPropertyName("sortOrder")] int? SortOrder,
    [property: JsonPropertyName("courses")] IReadOnlyList<CurriculumCourseDetailDto> Courses
);

public record CurriculumCourseDetailDto(
    [property: JsonPropertyName("courseCode")] string CourseCode,
    [property: JsonPropertyName("courseName")] string? CourseName,
    [property: JsonPropertyName("credits")] decimal? Credits,
    [property: JsonPropertyName("isRequired")] bool IsRequired,
    [property: JsonPropertyName("prereqRule")] string? PrereqRule
);

public record StudentSearchQueryDto
{
    [property: JsonPropertyName("studentId")]
    public string? StudentId { get; init; }
    [property: JsonPropertyName("programCode")]
    public string? ProgramCode { get; init; }
    [property: JsonPropertyName("cohortCode")]
    public string? CohortCode { get; init; }
    [property: JsonPropertyName("keyword")]
    public string? Keyword { get; init; }
    [property: JsonPropertyName("page")]
    public int Page { get; init; } = 1;
    [property: JsonPropertyName("pageSize")]
    public int PageSize { get; init; } = 20;
}

public record StudentSearchResultDto(
    [property: JsonPropertyName("students")] IReadOnlyList<StudentDto> Students,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize
);

public record EligibleCourseDto(
    [property: JsonPropertyName("courseCode")] string CourseCode,
    [property: JsonPropertyName("courseName")] string? CourseName,
    [property: JsonPropertyName("credits")] decimal? Credits,
    [property: JsonPropertyName("categoryName")] string? CategoryName,
    [property: JsonPropertyName("isRequired")] bool IsRequired,
    [property: JsonPropertyName("prerequisitesMet")] bool PrerequisitesMet,
    [property: JsonPropertyName("blockingReasons")] IReadOnlyList<string> BlockingReasons
);

public record EligibleCoursesResponseDto(
    [property: JsonPropertyName("studentId")] string StudentId,
    [property: JsonPropertyName("eligibleCourses")] IReadOnlyList<EligibleCourseDto> EligibleCourses,
    [property: JsonPropertyName("totalEligible")] int TotalEligible
);
