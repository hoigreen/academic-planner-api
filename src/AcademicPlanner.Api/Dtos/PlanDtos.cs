using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AcademicPlanner.Api.Dtos;

public record PlanItemDto(
    [property: JsonPropertyName("planId")] long PlanId,
    [property: JsonPropertyName("courseCode")] string CourseCode,
    [property: JsonPropertyName("termCode")] int TermCode,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("note")] string? Note,
    [property: JsonPropertyName("credits")] decimal? Credits
);

public record PlanByTermDto(
    [property: JsonPropertyName("studentId")] string StudentId,
    [property: JsonPropertyName("termCode")] int TermCode,
    [property: JsonPropertyName("items")] IReadOnlyList<PlanItemDto> Items
);

public record CreatePlanRequestDto(
    [property: JsonPropertyName("termCode"), Required] int? TermCode,
    [property: JsonPropertyName("courseCodes"), Required, MinLength(1)] IReadOnlyList<string> CourseCodes
);

public record AddPlanItemRequestDto(
    [property: JsonPropertyName("courseCode"), Required] string CourseCode,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("note")] string? Note
);

public record UpdatePlanItemRequestDto(
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("note")] string? Note
);

public record PlanValidationResultDto(
    [property: JsonPropertyName("isValid")] bool IsValid,
    [property: JsonPropertyName("totalCredits")] decimal TotalCredits,
    [property: JsonPropertyName("errors")] IReadOnlyList<string> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<string> Warnings
);
