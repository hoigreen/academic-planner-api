using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class concentration_course
{
    public long concentration_course_id { get; set; }

    public long concentration_id { get; set; }

    public string course_code { get; set; } = null!;

    public bool is_required { get; set; }

    public bool is_entry_course { get; set; }

    public int? sort_order { get; set; }

    public string meta { get; set; } = null!;

    public virtual concentration concentration { get; set; } = null!;

    public virtual course course_codeNavigation { get; set; } = null!;
}
