using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class course
{
    public string course_code { get; set; } = null!;

    public string? course_name { get; set; }

    public decimal? credits { get; set; }

    public string? subject_prefix { get; set; }

    public int? course_level { get; set; }

    public bool is_language_prep { get; set; }

    public string meta { get; set; } = null!;

    public virtual ICollection<concentration_course> concentration_courses { get; set; } = new List<concentration_course>();

    public virtual ICollection<course_advisory> course_advisories { get; set; } = new List<course_advisory>();

    public virtual ICollection<course_attempt> course_attempts { get; set; } = new List<course_attempt>();

    public virtual ICollection<course_offering> course_offerings { get; set; } = new List<course_offering>();

    public virtual ICollection<curriculum_requirement> curriculum_requirements { get; set; } = new List<curriculum_requirement>();

    public virtual ICollection<equivalency> equivalencycourse_codeNavigations { get; set; } = new List<equivalency>();

    public virtual ICollection<equivalency> equivalencyequivalent_course_codeNavigations { get; set; } = new List<equivalency>();

    public virtual ICollection<student_plan> student_plans { get; set; } = new List<student_plan>();
}
