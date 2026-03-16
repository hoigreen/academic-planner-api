using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class concentration
{
    public long concentration_id { get; set; }

    public long program_id { get; set; }

    public string concentration_code { get; set; } = null!;

    public string concentration_name { get; set; } = null!;

    public decimal? min_credits { get; set; }

    public string meta { get; set; } = null!;

    public virtual ICollection<concentration_course> concentration_courses { get; set; } = new List<concentration_course>();

    public virtual program program { get; set; } = null!;

    public virtual ICollection<student_concentration> student_concentrations { get; set; } = new List<student_concentration>();
}
