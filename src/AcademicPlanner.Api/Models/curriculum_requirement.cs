using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class curriculum_requirement
{
    public long requirement_id { get; set; }

    public long cohort_id { get; set; }

    public long category_id { get; set; }

    public string? course_code { get; set; }

    public decimal? min_credits { get; set; }

    public bool is_required { get; set; }

    public string? prereq_rule { get; set; }

    public int? effective_term_from { get; set; }

    public int? effective_term_to { get; set; }

    public string? note { get; set; }

    public virtual curriculum_category category { get; set; } = null!;

    public virtual cohort cohort { get; set; } = null!;

    public virtual course? course_codeNavigation { get; set; }

    public virtual term? effective_term_fromNavigation { get; set; }

    public virtual term? effective_term_toNavigation { get; set; }
}
