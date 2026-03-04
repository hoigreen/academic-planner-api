using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class equivalency_set
{
    public long equiv_set_id { get; set; }

    public long? program_id { get; set; }

    public string title { get; set; } = null!;

    public string? note { get; set; }

    public virtual ICollection<equivalency> equivalencies { get; set; } = new List<equivalency>();

    public virtual program? program { get; set; }
}
