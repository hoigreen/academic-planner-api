using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class course_offering
{
    public long offering_id { get; set; }

    public int term_code { get; set; }

    public string course_code { get; set; } = null!;

    public bool is_open { get; set; }

    public string? registration_channel { get; set; }

    public string meta { get; set; } = null!;

    public virtual course course_codeNavigation { get; set; } = null!;

    public virtual term term_codeNavigation { get; set; } = null!;
}
