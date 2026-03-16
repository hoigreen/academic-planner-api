using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicPlanner.Api.Dtos;

public record CourseRecommendationDto(
    [property: JsonPropertyName("courseCode")] string CourseCode,
    [property: JsonPropertyName("courseName")] string? CourseName,
    [property: JsonPropertyName("credits")] decimal Credits,
    [property: JsonPropertyName("recommendationType")] string RecommendationType,
    [property: JsonPropertyName("priorityScore")] int PriorityScore,
    [property: JsonPropertyName("reasons")] IReadOnlyList<string> Reasons,
    [property: JsonPropertyName("warnings")] IReadOnlyList<string> Warnings,
    [property: JsonPropertyName("registrationChannel")] string RegistrationChannel,
    [property: JsonPropertyName("canRegister")] bool CanRegister
);

public record RecommendationResponseDto(
    [property: JsonPropertyName("studentId")] string StudentId,
    [property: JsonPropertyName("targetTermCode")] int TargetTermCode,
    [property: JsonPropertyName("strategy")] string Strategy,
    [property: JsonPropertyName("recommendedCredits")] decimal RecommendedCredits,
    [property: JsonPropertyName("recommendedCourses")] IReadOnlyList<CourseRecommendationDto> RecommendedCourses,
    [property: JsonPropertyName("notRecommendedButRelevant")] IReadOnlyList<CourseRecommendationDto> NotRecommendedButRelevant,
    [property: JsonPropertyName("blockers")] IReadOnlyList<string> Blockers,
    [property: JsonPropertyName("advisoryNotes")] IReadOnlyList<string> AdvisoryNotes
);

public record NextTermRecommendationQueryDto(
    [property: FromQuery(Name = "targetTermCode")] int? TargetTermCode,
    [property: FromQuery(Name = "minCredits")] int MinCredits = 16,
    [property: FromQuery(Name = "maxCredits")] int MaxCredits = 20,
    [property: FromQuery(Name = "strategy")] string Strategy = "balanced"
);

public record RecommendationPreviewPlanRequestDto(
    [property: JsonPropertyName("targetTermCode")] int? TargetTermCode,
    [property: JsonPropertyName("courseCodes")] IReadOnlyList<string>? CourseCodes,
    [property: JsonPropertyName("minCredits")] int MinCredits = 16,
    [property: JsonPropertyName("maxCredits")] int MaxCredits = 20
);
