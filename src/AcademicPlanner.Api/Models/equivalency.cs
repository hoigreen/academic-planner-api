using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class equivalency
{
    public long equiv_set_id { get; set; }

    public string course_code { get; set; } = null!;

    public string equivalent_course_code { get; set; } = null!;

    public long cohort_id { get; set; }

    public string? note { get; set; }

    public virtual cohort cohort { get; set; } = null!;

    public virtual course course_codeNavigation { get; set; } = null!;

    public virtual equivalency_set equiv_set { get; set; } = null!;

    public virtual course equivalent_course_codeNavigation { get; set; } = null!;
}
