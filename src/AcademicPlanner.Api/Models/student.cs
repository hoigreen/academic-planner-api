using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class student
{
    public string student_id { get; set; } = null!;

    public string? last_name { get; set; }

    public string? first_name { get; set; }

    public long? program_id { get; set; }

    public long? cohort_id { get; set; }

    public string? status { get; set; }

    public int? english_level { get; set; }

    public decimal? ielts_score { get; set; }

    public string meta { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual cohort? cohort { get; set; }

    public virtual ICollection<course_attempt> course_attempts { get; set; } = new List<course_attempt>();

    public virtual program? program { get; set; }

    public virtual ICollection<student_concentration> student_concentrations { get; set; } = new List<student_concentration>();

    public virtual ICollection<student_plan> student_plans { get; set; } = new List<student_plan>();
}
