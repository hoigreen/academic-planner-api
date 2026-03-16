using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class course_advisory
{
    public long advisory_id { get; set; }

    public string course_code { get; set; } = null!;

    public string advisory_type { get; set; } = null!;

    public string rule_json { get; set; } = null!;

    public string? note { get; set; }

    public virtual course course_codeNavigation { get; set; } = null!;
}
