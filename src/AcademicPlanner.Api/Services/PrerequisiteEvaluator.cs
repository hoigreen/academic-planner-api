using System.Text.Json;

namespace AcademicPlanner.Api.Services;

public record PrerequisiteContext(
    ISet<string> CompletedCourses,
    int? EnglishLevel,
    decimal? IeltsScore
);

public record PrerequisiteEvaluationResult(
    bool IsSatisfied,
    IReadOnlyList<string> BlockingReasons,
    IReadOnlyList<string> MissingCourses,
    string? MissingEnglishRequirement
);

public interface IPrerequisiteEvaluator
{
    PrerequisiteEvaluationResult Evaluate(string? prereqRuleJson, PrerequisiteContext context);
}

public class PrerequisiteEvaluator : IPrerequisiteEvaluator
{
    public PrerequisiteEvaluationResult Evaluate(string? prereqRuleJson, PrerequisiteContext context)
    {
        if (string.IsNullOrWhiteSpace(prereqRuleJson))
        {
            return new PrerequisiteEvaluationResult(true, [], [], null);
        }

        try
        {
            using var doc = JsonDocument.Parse(prereqRuleJson);
            return EvaluateNode(doc.RootElement, context);
        }
        catch (JsonException)
        {
            return new PrerequisiteEvaluationResult(true, [], [], null);
        }
    }

    private static PrerequisiteEvaluationResult EvaluateNode(JsonElement node, PrerequisiteContext context)
    {
        if (!node.TryGetProperty("type", out var typeElement))
        {
            return new PrerequisiteEvaluationResult(true, [], [], null);
        }

        var type = typeElement.GetString()?.ToLowerInvariant();
        return type switch
        {
            "none" => new PrerequisiteEvaluationResult(true, [], [], null),
            "course" => EvaluateCourse(node, context),
            "english" => EvaluateEnglish(node, context),
            "and" => EvaluateAnd(node, context),
            "or" => EvaluateOr(node, context),
            _ => new PrerequisiteEvaluationResult(true, [], [], null)
        };
    }

    private static PrerequisiteEvaluationResult EvaluateCourse(JsonElement node, PrerequisiteContext context)
    {
        var missing = new List<string>();
        if (node.TryGetProperty("courses", out var courses) && courses.ValueKind == JsonValueKind.Array)
        {
            foreach (var course in courses.EnumerateArray())
            {
                if (course.TryGetProperty("code", out var codeValue))
                {
                    var code = codeValue.GetString()?.ToUpperInvariant();
                    if (!string.IsNullOrWhiteSpace(code) && !context.CompletedCourses.Contains(code))
                    {
                        missing.Add(code);
                    }
                }
            }
        }

        var blocking = missing.Select(x => $"Missing prerequisite course: {x}").ToList();
        return new PrerequisiteEvaluationResult(missing.Count == 0, blocking, missing, null);
    }

    private static PrerequisiteEvaluationResult EvaluateEnglish(JsonElement node, PrerequisiteContext context)
    {
        int? minLevel = null;
        decimal? minIelts = null;
        if (node.TryGetProperty("min_level", out var levelValue) && levelValue.TryGetInt32(out var parsedLevel))
        {
            minLevel = parsedLevel;
        }

        if (node.TryGetProperty("min_ielts", out var ieltsValue) && ieltsValue.TryGetDecimal(out var parsedIelts))
        {
            minIelts = parsedIelts;
        }

        var okLevel = !minLevel.HasValue || (context.EnglishLevel.HasValue && context.EnglishLevel.Value >= minLevel.Value);
        var okIelts = !minIelts.HasValue || (context.IeltsScore.HasValue && context.IeltsScore.Value >= minIelts.Value);
        if (okLevel && okIelts)
        {
            return new PrerequisiteEvaluationResult(true, [], [], null);
        }

        var requirementText = $"Need English level >= {minLevel?.ToString() ?? "N/A"} or IELTS >= {minIelts?.ToString("0.0") ?? "N/A"}";
        return new PrerequisiteEvaluationResult(false, [requirementText], [], requirementText);
    }

    private static PrerequisiteEvaluationResult EvaluateAnd(JsonElement node, PrerequisiteContext context)
    {
        var blockers = new List<string>();
        var missingCourses = new List<string>();
        string? missingEnglish = null;

        if (node.TryGetProperty("rules", out var rules) && rules.ValueKind == JsonValueKind.Array)
        {
            foreach (var rule in rules.EnumerateArray())
            {
                var result = EvaluateNode(rule, context);
                if (!result.IsSatisfied)
                {
                    blockers.AddRange(result.BlockingReasons);
                    missingCourses.AddRange(result.MissingCourses);
                    missingEnglish ??= result.MissingEnglishRequirement;
                }
            }
        }

        return new PrerequisiteEvaluationResult(blockers.Count == 0, blockers.Distinct().ToList(), missingCourses.Distinct().ToList(), missingEnglish);
    }

    private static PrerequisiteEvaluationResult EvaluateOr(JsonElement node, PrerequisiteContext context)
    {
        var collectedBlockers = new List<string>();
        var collectedMissingCourses = new List<string>();
        string? missingEnglish = null;

        if (node.TryGetProperty("rules", out var rules) && rules.ValueKind == JsonValueKind.Array)
        {
            foreach (var rule in rules.EnumerateArray())
            {
                var result = EvaluateNode(rule, context);
                if (result.IsSatisfied)
                {
                    return new PrerequisiteEvaluationResult(true, [], [], null);
                }

                collectedBlockers.AddRange(result.BlockingReasons);
                collectedMissingCourses.AddRange(result.MissingCourses);
                missingEnglish ??= result.MissingEnglishRequirement;
            }
        }

        return new PrerequisiteEvaluationResult(false, collectedBlockers.Distinct().ToList(), collectedMissingCourses.Distinct().ToList(), missingEnglish);
    }
}
