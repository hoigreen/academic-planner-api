namespace AcademicPlanner.Api.Models;

/// <summary>
/// Mirrors the PostgreSQL composite type acad.knowledge_block.
/// Represents one block/category in a curriculum structure.
/// </summary>
public sealed class KnowledgeBlock
{
    public string BlockName { get; set; } = string.Empty;
    public int MinCreditsRequired { get; set; }
    public bool IsMandatory { get; set; }
    public string? Description { get; set; }
}
