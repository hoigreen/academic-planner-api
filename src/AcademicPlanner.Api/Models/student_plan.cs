using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class student_plan
{
    public long plan_id { get; set; }

    public string student_id { get; set; } = null!;

    public int term_code { get; set; }

    public string course_code { get; set; } = null!;

    public string status { get; set; } = null!;

    public string? note { get; set; }

    public DateTime created_at { get; set; }

    public virtual course course_codeNavigation { get; set; } = null!;

    public virtual student student { get; set; } = null!;

    public virtual term term_codeNavigation { get; set; } = null!;
}
