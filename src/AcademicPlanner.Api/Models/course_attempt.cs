using System;
using System.Collections.Generic;

namespace AcademicPlanner.Api.Models;

public partial class course_attempt
{
    public long attempt_id { get; set; }

    public string student_id { get; set; } = null!;

    public string course_code { get; set; } = null!;

    public int term_code { get; set; }

    public int? term_seq { get; set; }

    public int attempt_no { get; set; }

    public decimal? credits { get; set; }

    public string? grade_letter { get; set; }

    public bool is_completed { get; set; }

    public decimal? snapshot_cum_credits { get; set; }

    public decimal? snapshot_target_credits { get; set; }

    public decimal? snapshot_cum_gpa { get; set; }

    public string? source_file { get; set; }

    public string? source_rowkey { get; set; }

    public string raw_record { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual course course_codeNavigation { get; set; } = null!;

    public virtual student student { get; set; } = null!;

    public virtual term term_codeNavigation { get; set; } = null!;
}
