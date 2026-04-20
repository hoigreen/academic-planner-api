namespace AcademicPlanner.Api.Models;

public partial class curriculum
{
    public long curriculum_id { get; set; }
    public long program_id { get; set; }
    public long cohort_id { get; set; }
    public string major_name { get; set; } = null!;
    public string cohort_code { get; set; } = null!;

    /// <summary>ORDBMS: acad.knowledge_block[] — curriculum block structure</summary>
    public KnowledgeBlock[] structure { get; set; } = [];

    /// <summary>ORDBMS: JSONB — maps block_name → [course_code, ...]</summary>
    public string course_mapping { get; set; } = "{}";

    public decimal? total_credits { get; set; }
    public string meta { get; set; } = "{}";
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public virtual program program { get; set; } = null!;
    public virtual cohort cohort { get; set; } = null!;
}
