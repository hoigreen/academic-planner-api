using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class term
{
    public int term_code { get; set; }

    public int? year { get; set; }

    public int? term_no { get; set; }

    public DateOnly? start_date { get; set; }

    public DateOnly? end_date { get; set; }

    public virtual ICollection<course_offering> course_offerings { get; set; } = new List<course_offering>();

    public virtual ICollection<course_attempt> course_attempts { get; set; } = new List<course_attempt>();

    public virtual ICollection<curriculum_requirement> curriculum_requirementeffective_term_fromNavigations { get; set; } = new List<curriculum_requirement>();

    public virtual ICollection<curriculum_requirement> curriculum_requirementeffective_term_toNavigations { get; set; } = new List<curriculum_requirement>();

    public virtual ICollection<student_concentration> student_concentrations { get; set; } = new List<student_concentration>();

    public virtual ICollection<student_plan> student_plans { get; set; } = new List<student_plan>();
}
