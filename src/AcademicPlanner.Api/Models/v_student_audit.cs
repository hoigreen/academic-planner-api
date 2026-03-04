using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class v_student_audit
{
    public string? student_id { get; set; }

    public long? program_id { get; set; }

    public long? cohort_id { get; set; }

    public long? requirement_id { get; set; }

    public string? category_name { get; set; }

    public string? course_code { get; set; }

    public decimal? min_credits { get; set; }

    public string? prereq_rule { get; set; }

    public int? last_term { get; set; }

    public bool? completed { get; set; }
}
