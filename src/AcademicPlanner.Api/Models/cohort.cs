using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class cohort
{
    public long cohort_id { get; set; }

    public long program_id { get; set; }

    public string cohort_code { get; set; } = null!;

    public int? start_year { get; set; }

    public string? note { get; set; }

    public virtual ICollection<curriculum_requirement> curriculum_requirements { get; set; } = new List<curriculum_requirement>();

    public virtual ICollection<equivalency> equivalencies { get; set; } = new List<equivalency>();

    public virtual program program { get; set; } = null!;

    public virtual ICollection<student> students { get; set; } = new List<student>();
}
