using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicPlanner.Api.Dtos;

public record ProgramDto(
    [property: JsonPropertyName("programId")] long ProgramId,
    [property: JsonPropertyName("programCode")] string ProgramCode,
    [property: JsonPropertyName("programName")] string ProgramName,
    [property: JsonPropertyName("degreeLevel")] string? DegreeLevel,
    [property: JsonPropertyName("defaultTargetCredits")] decimal? DefaultTargetCredits
);

public record CohortDto(
    [property: JsonPropertyName("cohortId")] long CohortId,
    [property: JsonPropertyName("cohortCode")] string CohortCode,
    [property: JsonPropertyName("startYear")] int? StartYear
);

public record CourseDto(
    [property: JsonPropertyName("courseCode")] string CourseCode,
    [property: JsonPropertyName("courseName")] string? CourseName,
    [property: JsonPropertyName("credits")] decimal? Credits,
    [property: JsonPropertyName("prefix")] string? Prefix,
    [property: JsonPropertyName("level")] int? Level
);

public record CoursePrerequisiteDto(
    [property: JsonPropertyName("courseCode")] string CourseCode,
    [property: JsonPropertyName("prerequisiteRules")] IReadOnlyList<string> PrerequisiteRules
);

public record CourseEquivalencyDto(
    [property: JsonPropertyName("courseCode")] string CourseCode,
    [property: JsonPropertyName("equivalentCourseCode")] string EquivalentCourseCode,
    [property: JsonPropertyName("cohortId")] long CohortId
);

public record ConcentrationDto(
    [property: JsonPropertyName("concentrationId")] long ConcentrationId,
    [property: JsonPropertyName("concentrationCode")] string ConcentrationCode,
    [property: JsonPropertyName("concentrationName")] string ConcentrationName,
    [property: JsonPropertyName("minCredits")] decimal? MinCredits
);

public record StudentConcentrationDto(
    [property: JsonPropertyName("studentId")] string StudentId,
    [property: JsonPropertyName("concentrationId")] long ConcentrationId,
    [property: JsonPropertyName("concentrationCode")] string ConcentrationCode,
    [property: JsonPropertyName("concentrationName")] string ConcentrationName,
    [property: JsonPropertyName("approvedTermCode")] int? ApprovedTermCode,
    [property: JsonPropertyName("status")] string Status
);

public record AssignStudentConcentrationRequestDto(
    [property: JsonPropertyName("concentrationId"), Required] long? ConcentrationId,
    [property: JsonPropertyName("approvedTermCode")] int? ApprovedTermCode
);

public record CourseQueryDto(
    [property: FromQuery(Name = "prefix")] string? Prefix,
    [property: FromQuery(Name = "level")] int? Level,
    [property: FromQuery(Name = "programCode")] string? ProgramCode,
    [property: FromQuery(Name = "keyword")] string? Keyword
);
