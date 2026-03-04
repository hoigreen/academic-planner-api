using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class program
{
    public long program_id { get; set; }

    public string program_code { get; set; } = null!;

    public string program_name { get; set; } = null!;

    public string? degree_level { get; set; }

    public decimal? default_target_credits { get; set; }

    public string meta { get; set; } = null!;

    public virtual ICollection<cohort> cohorts { get; set; } = new List<cohort>();

    public virtual ICollection<curriculum_category> curriculum_categories { get; set; } = new List<curriculum_category>();

    public virtual ICollection<equivalency_set> equivalency_sets { get; set; } = new List<equivalency_set>();

    public virtual ICollection<student> students { get; set; } = new List<student>();
}
