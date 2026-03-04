using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class v_latest_attempt
{
    public string? student_id { get; set; }

    public string? course_code { get; set; }

    public int? term_code { get; set; }

    public int? attempt_no { get; set; }

    public bool? is_completed { get; set; }

    public decimal? credits { get; set; }
}
