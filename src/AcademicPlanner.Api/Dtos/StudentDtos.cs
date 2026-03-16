using System.Text.Json.Serialization;

namespace AcademicPlanner.Api.Dtos;

public record StudentDto(
    [property: JsonPropertyName("studentId")] string StudentId,
    [property: JsonPropertyName("fullName")] string FullName,
    [property: JsonPropertyName("programId")] long? ProgramId,
    [property: JsonPropertyName("programCode")] string? ProgramCode,
    [property: JsonPropertyName("cohortId")] long? CohortId,
    [property: JsonPropertyName("cohortCode")] string? CohortCode,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("englishLevel")] int? EnglishLevel,
    [property: JsonPropertyName("ieltsScore")] decimal? IeltsScore
);

public record TranscriptItemDto(
    [property: JsonPropertyName("attemptId")] long AttemptId,
    [property: JsonPropertyName("courseCode")] string CourseCode,
    [property: JsonPropertyName("courseName")] string? CourseName,
    [property: JsonPropertyName("termCode")] int TermCode,
    [property: JsonPropertyName("attemptNo")] int AttemptNo,
    [property: JsonPropertyName("credits")] decimal? Credits,
    [property: JsonPropertyName("gradeLetter")] string? GradeLetter,
    [property: JsonPropertyName("isCompleted")] bool IsCompleted
);

public record LatestAttemptDto(
    [property: JsonPropertyName("courseCode")] string CourseCode,
    [property: JsonPropertyName("termCode")] int? TermCode,
    [property: JsonPropertyName("attemptNo")] int? AttemptNo,
    [property: JsonPropertyName("credits")] decimal? Credits,
    [property: JsonPropertyName("gradeLetter")] string? GradeLetter,
    [property: JsonPropertyName("isCompleted")] bool? IsCompleted
);

public record StudentProfileSummaryDto(
    [property: JsonPropertyName("studentId")] string StudentId,
    [property: JsonPropertyName("programCode")] string? ProgramCode,
    [property: JsonPropertyName("cohortCode")] string? CohortCode,
    [property: JsonPropertyName("totalCompletedCredits")] decimal TotalCompletedCredits,
    [property: JsonPropertyName("gpa")] decimal? Gpa,
    [property: JsonPropertyName("englishLevel")] int? EnglishLevel,
    [property: JsonPropertyName("ieltsScore")] decimal? IeltsScore,
    [property: JsonPropertyName("lastTermCode")] int? LastTermCode
);
