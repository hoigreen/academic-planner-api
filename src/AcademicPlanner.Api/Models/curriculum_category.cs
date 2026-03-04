using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class curriculum_category
{
    public long category_id { get; set; }

    public long program_id { get; set; }

    public string category_name { get; set; } = null!;

    public decimal? min_credits { get; set; }

    public int? sort_order { get; set; }

    public virtual ICollection<curriculum_requirement> curriculum_requirements { get; set; } = new List<curriculum_requirement>();

    public virtual program program { get; set; } = null!;
}
