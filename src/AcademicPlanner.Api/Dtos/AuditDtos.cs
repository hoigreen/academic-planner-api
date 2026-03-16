using System.Text.Json.Serialization;

namespace AcademicPlanner.Api.Dtos;

public record ConcentrationInfoDto(
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("isSelected")] bool IsSelected
);

public record Eligibility300To400Dto(
    [property: JsonPropertyName("isEligible")] bool IsEligible,
    [property: JsonPropertyName("missingReasons")] IReadOnlyList<string> MissingReasons
);

public record CategoryProgressDto(
    [property: JsonPropertyName("categoryId")] long CategoryId,
    [property: JsonPropertyName("categoryName")] string CategoryName,
    [property: JsonPropertyName("requiredCredits")] decimal RequiredCredits,
    [property: JsonPropertyName("earnedCredits")] decimal EarnedCredits,
    [property: JsonPropertyName("missingCredits")] decimal MissingCredits,
    [property: JsonPropertyName("requiredCoursesCompleted")] int RequiredCoursesCompleted,
    [property: JsonPropertyName("requiredCoursesRemaining")] int RequiredCoursesRemaining
);

public record StudentAuditSummaryDto(
    [property: JsonPropertyName("studentId")] string StudentId,
    [property: JsonPropertyName("programCode")] string? ProgramCode,
    [property: JsonPropertyName("cohortCode")] string? CohortCode,
    [property: JsonPropertyName("totalCompletedCredits")] decimal TotalCompletedCredits,
    [property: JsonPropertyName("completedCredits100to200")] decimal CompletedCredits100to200,
    [property: JsonPropertyName("completedCredits300to400")] decimal CompletedCredits300to400,
    [property: JsonPropertyName("requiredCoursesCompleted")] int RequiredCoursesCompleted,
    [property: JsonPropertyName("requiredCoursesRemaining")] int RequiredCoursesRemaining,
    [property: JsonPropertyName("electiveCreditsCompleted")] decimal ElectiveCreditsCompleted,
    [property: JsonPropertyName("electiveCreditsRemaining")] decimal ElectiveCreditsRemaining,
    [property: JsonPropertyName("concentration")] ConcentrationInfoDto Concentration,
    [property: JsonPropertyName("eligibility300to400")] Eligibility300To400Dto Eligibility300To400,
    [property: JsonPropertyName("overallProgressPercent")] decimal OverallProgressPercent,
    [property: JsonPropertyName("warnings")] IReadOnlyList<string> Warnings
);

public record StudentAuditDto(
    [property: JsonPropertyName("summary")] StudentAuditSummaryDto Summary,
    [property: JsonPropertyName("progressByCategory")] IReadOnlyList<CategoryProgressDto> ProgressByCategory,
    [property: JsonPropertyName("missingCourses")] IReadOnlyList<string> MissingCourses
);
