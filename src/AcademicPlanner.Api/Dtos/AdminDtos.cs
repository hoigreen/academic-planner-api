using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AcademicPlanner.Api.Dtos;

public record CreateCourseRequestDto(
    [property: JsonPropertyName("courseCode"), Required] string CourseCode,
    [property: JsonPropertyName("courseName")] string? CourseName,
    [property: JsonPropertyName("credits")] decimal? Credits,
    [property: JsonPropertyName("subjectPrefix")] string? SubjectPrefix,
    [property: JsonPropertyName("courseLevel")] int? CourseLevel
);

public record CreateCurriculumRequirementRequestDto(
    [property: JsonPropertyName("cohortId"), Required] long? CohortId,
    [property: JsonPropertyName("categoryId"), Required] long? CategoryId,
    [property: JsonPropertyName("kind"), Required] string Kind,
    [property: JsonPropertyName("courseCode")] string? CourseCode,
    [property: JsonPropertyName("minCredits")] decimal? MinCredits,
    [property: JsonPropertyName("isRequired")] bool IsRequired,
    [property: JsonPropertyName("allowedCourses")] IReadOnlyList<string>? AllowedCourses,
    [property: JsonPropertyName("prereqRule")] string? PrereqRule
);

public record CreateCourseOfferingRequestDto(
    [property: JsonPropertyName("termCode"), Required] int? TermCode,
    [property: JsonPropertyName("courseCode"), Required] string CourseCode,
    [property: JsonPropertyName("isOpen")] bool IsOpen,
    [property: JsonPropertyName("registrationChannel")] string? RegistrationChannel
);

public record CreateConcentrationRequestDto(
    [property: JsonPropertyName("programCode"), Required] string ProgramCode,
    [property: JsonPropertyName("concentrationCode"), Required] string ConcentrationCode,
    [property: JsonPropertyName("concentrationName"), Required] string ConcentrationName,
    [property: JsonPropertyName("minCredits")] decimal? MinCredits
);

public record CreateConcentrationCourseRequestDto(
    [property: JsonPropertyName("concentrationId"), Required] long? ConcentrationId,
    [property: JsonPropertyName("courseCode"), Required] string CourseCode,
    [property: JsonPropertyName("isRequired")] bool IsRequired,
    [property: JsonPropertyName("isEntryCourse")] bool IsEntryCourse
);

public record ImportCourseAttemptRequestDto(
    [property: JsonPropertyName("studentId"), Required] string StudentId,
    [property: JsonPropertyName("courseCode"), Required] string CourseCode,
    [property: JsonPropertyName("termCode"), Required] int? TermCode,
    [property: JsonPropertyName("attemptNo")] int AttemptNo,
    [property: JsonPropertyName("credits")] decimal? Credits,
    [property: JsonPropertyName("gradeLetter")] string? GradeLetter,
    [property: JsonPropertyName("isCompleted")] bool IsCompleted
);
