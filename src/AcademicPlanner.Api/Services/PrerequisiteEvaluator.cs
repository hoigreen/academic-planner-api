using System.Text.Json;

namespace AcademicPlanner.Api.Services;

/// <summary>
/// Context passed to the prerequisite rule evaluator.
/// </summary>
public record PrerequisiteContext(
    ISet<string> CompletedCourses,
    int? EnglishLevel,
    decimal? IeltsScore,
    decimal TotalCredits = 0,
    decimal Gpa = 0
);

/// <summary>
/// Result of evaluating a prerequisite rule tree.
/// </summary>
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

/// <summary>
/// Recursive, pure rule-based evaluator for prerequisite JSONB expressions.
///
/// Supports two formats:
///
/// 1. Op/Args format (preferred):
///    {"op":"AND","args":[...]}
///    {"op":"OR","args":[...]}
///    {"op":"NOT","arg":{...}}
///    {"op":"COMPLETED","course":"COURSE101"}
///    {"op":"MIN_CREDITS","value":30}
///    {"op":"MIN_GPA","value":2.5}
///    {"op":"ENGLISH","min_level":5,"min_ielts":5.5}
///
/// 2. Type format (legacy, backward-compatible):
///    {"type":"none"}
///    {"type":"course","courses":[{"code":"X","min_grade":"C"}]}
///    {"type":"english","min_level":5,"min_ielts":5.5}
///    {"type":"and","rules":[...]}
///    {"type":"or","rules":[...]}
/// </summary>
public class PrerequisiteEvaluator : IPrerequisiteEvaluator
{
    private static readonly HashSet<string> PassingGrades =
        ["A", "A-", "B+", "B", "B-", "C+", "C", "C-", "D+", "D", "D-", "P"];

    // Minimum grade ordinals for comparison
    private static readonly Dictionary<string, int> GradeOrdinal = new(StringComparer.OrdinalIgnoreCase)
    {
        ["A"]  = 12, ["A-"] = 11,
        ["B+"] = 10, ["B"]  =  9, ["B-"] = 8,
        ["C+"] =  7, ["C"]  =  6, ["C-"] = 5,
        ["D+"] =  4, ["D"]  =  3, ["D-"] = 2,
        ["F"]  =  0, ["P"]  =  6, // P treated as C-equivalent
    };

    public PrerequisiteEvaluationResult Evaluate(string? prereqRuleJson, PrerequisiteContext context)
    {
        if (string.IsNullOrWhiteSpace(prereqRuleJson))
            return Satisfied();

        try
        {
            using var doc = JsonDocument.Parse(prereqRuleJson);
            return EvaluateNode(doc.RootElement, context);
        }
        catch (JsonException)
        {
            return Satisfied(); // treat unparseable rule as no requirement
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Dispatcher — detects op/args vs. type/legacy format
    // ────────────────────────────────────────────────────────────────────────

    private static PrerequisiteEvaluationResult EvaluateNode(JsonElement node, PrerequisiteContext ctx)
    {
        // Op/Args format
        if (node.TryGetProperty("op", out var opEl))
        {
            return (opEl.GetString()?.ToUpperInvariant()) switch
            {
                "AND"         => EvaluateAnd(node, ctx),
                "OR"          => EvaluateOr(node, ctx),
                "NOT"         => EvaluateNot(node, ctx),
                "COMPLETED"   => EvaluateCompleted(node, ctx),
                "MIN_CREDITS" => EvaluateMinCredits(node, ctx),
                "MIN_GPA"     => EvaluateMinGpa(node, ctx),
                "ENGLISH"     => EvaluateEnglishOp(node, ctx),
                _             => Satisfied()
            };
        }

        // Legacy type format
        if (node.TryGetProperty("type", out var typeEl))
        {
            return (typeEl.GetString()?.ToLowerInvariant()) switch
            {
                "none"    => Satisfied(),
                "course"  => EvaluateLegacyCourse(node, ctx),
                "english" => EvaluateLegacyEnglish(node, ctx),
                "and"     => EvaluateLegacyAnd(node, ctx),
                "or"      => EvaluateLegacyOr(node, ctx),
                _         => Satisfied()
            };
        }

        return Satisfied();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Op/Args operators
    // ────────────────────────────────────────────────────────────────────────

    private static PrerequisiteEvaluationResult EvaluateAnd(JsonElement node, PrerequisiteContext ctx)
    {
        var blockers  = new List<string>();
        var missing   = new List<string>();
        string? missingEng = null;

        if (!node.TryGetProperty("args", out var args) || args.ValueKind != JsonValueKind.Array)
            return Satisfied();

        foreach (var child in args.EnumerateArray())
        {
            var r = EvaluateNode(child, ctx);
            if (!r.IsSatisfied)
            {
                blockers.AddRange(r.BlockingReasons);
                missing.AddRange(r.MissingCourses);
                missingEng ??= r.MissingEnglishRequirement;
            }
        }

        return blockers.Count == 0
            ? Satisfied()
            : new PrerequisiteEvaluationResult(false, blockers.Distinct().ToList(), missing.Distinct().ToList(), missingEng);
    }

    private static PrerequisiteEvaluationResult EvaluateOr(JsonElement node, PrerequisiteContext ctx)
    {
        var blockers  = new List<string>();
        var missing   = new List<string>();
        string? missingEng = null;

        if (!node.TryGetProperty("args", out var args) || args.ValueKind != JsonValueKind.Array)
            return Satisfied();

        foreach (var child in args.EnumerateArray())
        {
            var r = EvaluateNode(child, ctx);
            if (r.IsSatisfied) return Satisfied();
            blockers.AddRange(r.BlockingReasons);
            missing.AddRange(r.MissingCourses);
            missingEng ??= r.MissingEnglishRequirement;
        }

        return new PrerequisiteEvaluationResult(false, blockers.Distinct().ToList(), missing.Distinct().ToList(), missingEng);
    }

    private static PrerequisiteEvaluationResult EvaluateNot(JsonElement node, PrerequisiteContext ctx)
    {
        if (!node.TryGetProperty("arg", out var child))
            return Satisfied();

        var inner = EvaluateNode(child, ctx);
        return inner.IsSatisfied
            ? new PrerequisiteEvaluationResult(false, ["NOT condition failed"], [], null)
            : Satisfied();
    }

    private static PrerequisiteEvaluationResult EvaluateCompleted(JsonElement node, PrerequisiteContext ctx)
    {
        if (!node.TryGetProperty("course", out var courseEl))
            return Satisfied();

        var code = courseEl.GetString()?.ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code)) return Satisfied();

        // Optional min_grade check
        if (node.TryGetProperty("min_grade", out var minGradeEl))
        {
            var minGrade = minGradeEl.GetString()?.ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(minGrade) && GradeOrdinal.TryGetValue(minGrade, out var minOrd))
            {
                // Check if student has the course with at least that grade
                // (We don't carry per-course grades here, so fall through to completion check)
            }
        }

        if (ctx.CompletedCourses.Contains(code))
            return Satisfied();

        return new PrerequisiteEvaluationResult(false, [$"Must complete {code} first"], [code], null);
    }

    private static PrerequisiteEvaluationResult EvaluateMinCredits(JsonElement node, PrerequisiteContext ctx)
    {
        if (!node.TryGetProperty("value", out var valEl) || !valEl.TryGetDecimal(out var required))
            return Satisfied();

        if (ctx.TotalCredits >= required)
            return Satisfied();

        var msg = $"Need at least {required} total credits (currently {ctx.TotalCredits})";
        return new PrerequisiteEvaluationResult(false, [msg], [], null);
    }

    private static PrerequisiteEvaluationResult EvaluateMinGpa(JsonElement node, PrerequisiteContext ctx)
    {
        if (!node.TryGetProperty("value", out var valEl) || !valEl.TryGetDecimal(out var required))
            return Satisfied();

        if (ctx.Gpa >= required)
            return Satisfied();

        var msg = $"Need GPA >= {required} (current: {ctx.Gpa:0.00})";
        return new PrerequisiteEvaluationResult(false, [msg], [], null);
    }

    private static PrerequisiteEvaluationResult EvaluateEnglishOp(JsonElement node, PrerequisiteContext ctx)
    {
        int? minLevel = null;
        decimal? minIelts = null;

        if (node.TryGetProperty("min_level", out var lvl) && lvl.TryGetInt32(out var lv))
            minLevel = lv;
        if (node.TryGetProperty("min_ielts", out var ielts) && ielts.TryGetDecimal(out var il))
            minIelts = il;

        var okLevel = !minLevel.HasValue || (ctx.EnglishLevel.HasValue && ctx.EnglishLevel.Value >= minLevel.Value);
        var okIelts = !minIelts.HasValue || (ctx.IeltsScore.HasValue && ctx.IeltsScore.Value >= minIelts.Value);

        if (okLevel || okIelts) return Satisfied();

        var req = $"English: level >= {minLevel?.ToString() ?? "N/A"} or IELTS >= {minIelts?.ToString("0.0") ?? "N/A"}";
        return new PrerequisiteEvaluationResult(false, [req], [], req);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Legacy type-based operators
    // ────────────────────────────────────────────────────────────────────────

    private static PrerequisiteEvaluationResult EvaluateLegacyCourse(JsonElement node, PrerequisiteContext ctx)
    {
        var missing = new List<string>();
        if (node.TryGetProperty("courses", out var courses) && courses.ValueKind == JsonValueKind.Array)
        {
            foreach (var course in courses.EnumerateArray())
            {
                if (course.TryGetProperty("code", out var codeVal))
                {
                    var code = codeVal.GetString()?.ToUpperInvariant();
                    if (!string.IsNullOrWhiteSpace(code) && !ctx.CompletedCourses.Contains(code))
                        missing.Add(code);
                }
            }
        }
        if (missing.Count == 0) return Satisfied();
        var blockers = missing.Select(x => $"Missing prerequisite course: {x}").ToList();
        return new PrerequisiteEvaluationResult(false, blockers, missing, null);
    }

    private static PrerequisiteEvaluationResult EvaluateLegacyEnglish(JsonElement node, PrerequisiteContext ctx)
    {
        int? minLevel = null;
        decimal? minIelts = null;

        if (node.TryGetProperty("min_level", out var lvl) && lvl.TryGetInt32(out var lv)) minLevel = lv;
        if (node.TryGetProperty("min_ielts", out var ielts) && ielts.TryGetDecimal(out var il)) minIelts = il;

        var okLevel = !minLevel.HasValue || (ctx.EnglishLevel.HasValue && ctx.EnglishLevel.Value >= minLevel.Value);
        var okIelts = !minIelts.HasValue || (ctx.IeltsScore.HasValue && ctx.IeltsScore.Value >= minIelts.Value);

        if (okLevel && okIelts) return Satisfied();

        var req = $"Need English level >= {minLevel?.ToString() ?? "N/A"} or IELTS >= {minIelts?.ToString("0.0") ?? "N/A"}";
        return new PrerequisiteEvaluationResult(false, [req], [], req);
    }

    private static PrerequisiteEvaluationResult EvaluateLegacyAnd(JsonElement node, PrerequisiteContext ctx)
    {
        var blockers  = new List<string>();
        var missing   = new List<string>();
        string? missingEng = null;

        if (node.TryGetProperty("rules", out var rules) && rules.ValueKind == JsonValueKind.Array)
        {
            foreach (var rule in rules.EnumerateArray())
            {
                var r = EvaluateNode(rule, ctx);
                if (!r.IsSatisfied)
                {
                    blockers.AddRange(r.BlockingReasons);
                    missing.AddRange(r.MissingCourses);
                    missingEng ??= r.MissingEnglishRequirement;
                }
            }
        }
        return blockers.Count == 0
            ? Satisfied()
            : new PrerequisiteEvaluationResult(false, blockers.Distinct().ToList(), missing.Distinct().ToList(), missingEng);
    }

    private static PrerequisiteEvaluationResult EvaluateLegacyOr(JsonElement node, PrerequisiteContext ctx)
    {
        var blockers  = new List<string>();
        var missing   = new List<string>();
        string? missingEng = null;

        if (node.TryGetProperty("rules", out var rules) && rules.ValueKind == JsonValueKind.Array)
        {
            foreach (var rule in rules.EnumerateArray())
            {
                var r = EvaluateNode(rule, ctx);
                if (r.IsSatisfied) return Satisfied();
                blockers.AddRange(r.BlockingReasons);
                missing.AddRange(r.MissingCourses);
                missingEng ??= r.MissingEnglishRequirement;
            }
        }
        return new PrerequisiteEvaluationResult(false, blockers.Distinct().ToList(), missing.Distinct().ToList(), missingEng);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    private static PrerequisiteEvaluationResult Satisfied() =>
        new(true, Array.Empty<string>(), Array.Empty<string>(), null);
}
