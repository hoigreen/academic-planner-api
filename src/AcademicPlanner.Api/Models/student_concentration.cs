using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class student_concentration
{
    public long student_concentration_id { get; set; }

    public string student_id { get; set; } = null!;

    public long concentration_id { get; set; }

    public int? approved_term_code { get; set; }

    public string status { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual term? approved_term_codeNavigation { get; set; }

    public virtual concentration concentration { get; set; } = null!;

    public virtual student student { get; set; } = null!;
}
