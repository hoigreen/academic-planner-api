using System.Text.Json.Serialization;

namespace AcademicPlanner.Api.Dtos;

public record ApiEnvelope<T>(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("data")] T? Data,
    [property: JsonPropertyName("errors")] IReadOnlyList<string> Errors
);

public static class ApiEnvelope
{
    public static ApiEnvelope<T> Ok<T>(T data, string message = "OK")
        => new(true, message, data, []);

    public static ApiEnvelope<T> Fail<T>(string message, IReadOnlyList<string>? errors = null)
        => new(false, message, default, errors ?? []);
}

public record HealthDto(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("time")] DateTimeOffset Time
);
