using System;
using System.Collections.Generic;
using AcademicPlanner.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicPlanner.Api.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<cohort> cohorts { get; set; }

    public virtual DbSet<concentration> concentrations { get; set; }

    public virtual DbSet<concentration_course> concentration_courses { get; set; }

    public virtual DbSet<course> courses { get; set; }

    public virtual DbSet<course_advisory> course_advisories { get; set; }

    public virtual DbSet<course_attempt> course_attempts { get; set; }

    public virtual DbSet<course_offering> course_offerings { get; set; }

    public virtual DbSet<curriculum> curricula { get; set; }

    public virtual DbSet<curriculum_category> curriculum_categories { get; set; }

    public virtual DbSet<curriculum_requirement> curriculum_requirements { get; set; }

    public virtual DbSet<equivalency> equivalencies { get; set; }

    public virtual DbSet<equivalency_set> equivalency_sets { get; set; }

    public virtual DbSet<program> programs { get; set; }

    public virtual DbSet<student> students { get; set; }

    public virtual DbSet<student_concentration> student_concentrations { get; set; }

    public virtual DbSet<student_plan> student_plans { get; set; }

    public virtual DbSet<term> terms { get; set; }

    public virtual DbSet<v_latest_attempt> v_latest_attempts { get; set; }

    public virtual DbSet<v_student_audit> v_student_audits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("acad", "grade_letter", new[] { "A", "A-", "B+", "B", "B-", "C+", "C", "C-", "D+", "D", "D-", "F", "P", "W", "I" })
            .HasPostgresEnum("acad", "item_status", new[] { "planned", "in_progress", "completed", "waived", "failed" })
            .HasPostgresEnum("acad", "requirement_kind", new[] { "course", "credit_bucket" });
        // acad.knowledge_block composite type is registered via NpgsqlDataSourceBuilder.MapComposite<KnowledgeBlock>()
        // in Program.cs — no ModelBuilder call needed.

        modelBuilder.Entity<cohort>(entity =>
        {
            entity.HasKey(e => e.cohort_id).HasName("cohorts_pkey");

            entity.ToTable("cohorts", "acad");

            entity.HasIndex(e => new { e.program_id, e.cohort_code }, "cohorts_program_id_cohort_code_key").IsUnique();

            entity.HasOne(d => d.program).WithMany(p => p.cohorts)
                .HasForeignKey(d => d.program_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("cohorts_program_id_fkey");
        });

        modelBuilder.Entity<concentration>(entity =>
        {
            entity.HasKey(e => e.concentration_id).HasName("concentrations_pkey");

            entity.ToTable("concentrations", "acad");

            entity.HasIndex(e => new { e.program_id, e.concentration_code }, "concentrations_program_id_concentration_code_key").IsUnique();

            entity.Property(e => e.meta)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.min_credits).HasPrecision(5, 1);

            entity.HasOne(d => d.program).WithMany(p => p.concentrations)
                .HasForeignKey(d => d.program_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("concentrations_program_id_fkey");
        });

        modelBuilder.Entity<concentration_course>(entity =>
        {
            entity.HasKey(e => e.concentration_course_id).HasName("concentration_courses_pkey");

            entity.ToTable("concentration_courses", "acad");

            entity.HasIndex(e => new { e.concentration_id, e.course_code }, "concentration_courses_concentration_id_course_code_key").IsUnique();

            entity.Property(e => e.course_code).HasMaxLength(20);
            entity.Property(e => e.is_entry_course).HasDefaultValue(false);
            entity.Property(e => e.is_required).HasDefaultValue(true);
            entity.Property(e => e.meta)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");

            entity.HasOne(d => d.concentration).WithMany(p => p.concentration_courses)
                .HasForeignKey(d => d.concentration_id)
                .HasConstraintName("concentration_courses_concentration_id_fkey");

            entity.HasOne(d => d.course_codeNavigation).WithMany(p => p.concentration_courses)
                .HasForeignKey(d => d.course_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("concentration_courses_course_code_fkey");
        });

        modelBuilder.Entity<course>(entity =>
        {
            entity.HasKey(e => e.course_code).HasName("courses_pkey");

            entity.ToTable("courses", "acad");

            entity.HasIndex(e => e.meta, "gin_courses_meta").HasMethod("gin");

            entity.HasIndex(e => e.course_level, "idx_courses_level");

            entity.HasIndex(e => e.subject_prefix, "idx_courses_prefix");

            entity.Property(e => e.course_code).HasMaxLength(20);
            entity.Property(e => e.credits).HasPrecision(4, 1);
            entity.Property(e => e.is_language_prep).HasDefaultValue(false);
            entity.Property(e => e.meta)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
        });

        modelBuilder.Entity<course_advisory>(entity =>
        {
            entity.HasKey(e => e.advisory_id).HasName("course_advisories_pkey");

            entity.ToTable("course_advisories", "acad");

            entity.Property(e => e.course_code).HasMaxLength(20);
            entity.Property(e => e.rule_json)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");

            entity.HasOne(d => d.course_codeNavigation).WithMany(p => p.course_advisories)
                .HasForeignKey(d => d.course_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("course_advisories_course_code_fkey");
        });

        modelBuilder.Entity<course_attempt>(entity =>
        {
            entity.HasKey(e => e.attempt_id).HasName("course_attempts_pkey");

            entity.ToTable("course_attempts", "acad");

            entity.HasIndex(e => new { e.student_id, e.course_code, e.term_code, e.attempt_no }, "course_attempts_student_id_course_code_term_code_attempt_no_key").IsUnique();

            entity.HasIndex(e => e.raw_record, "gin_attempts_raw_record").HasMethod("gin");

            entity.HasIndex(e => new { e.course_code, e.term_code }, "idx_attempts_course_term");

            entity.HasIndex(e => new { e.student_id, e.course_code, e.term_code, e.attempt_no }, "idx_attempts_latest").IsDescending(false, false, true, true);

            entity.HasIndex(e => new { e.student_id, e.course_code }, "idx_attempts_student_course");

            entity.HasIndex(e => new { e.student_id, e.term_code }, "idx_attempts_student_term");

            entity.Property(e => e.attempt_no).HasDefaultValue(1);
            entity.Property(e => e.course_code).HasMaxLength(20);
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.credits).HasPrecision(4, 1);
            entity.Property(e => e.grade_letter).HasColumnType("acad.grade_letter");
            entity.Property(e => e.is_completed).HasDefaultValue(false);
            entity.Property(e => e.raw_record)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.snapshot_cum_credits).HasPrecision(6, 1);
            entity.Property(e => e.snapshot_cum_gpa).HasPrecision(3, 2);
            entity.Property(e => e.snapshot_target_credits).HasPrecision(6, 1);
            entity.Property(e => e.student_id).HasMaxLength(20);

            entity.HasOne(d => d.course_codeNavigation).WithMany(p => p.course_attempts)
                .HasForeignKey(d => d.course_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("course_attempts_course_code_fkey");

            entity.HasOne(d => d.student).WithMany(p => p.course_attempts)
                .HasForeignKey(d => d.student_id)
                .HasConstraintName("course_attempts_student_id_fkey");

            entity.HasOne(d => d.term_codeNavigation).WithMany(p => p.course_attempts)
                .HasForeignKey(d => d.term_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("course_attempts_term_code_fkey");
        });

        modelBuilder.Entity<course_offering>(entity =>
        {
            entity.HasKey(e => e.offering_id).HasName("course_offerings_pkey");

            entity.ToTable("course_offerings", "acad");

            entity.HasIndex(e => new { e.term_code, e.course_code }, "course_offerings_term_code_course_code_key").IsUnique();

            entity.Property(e => e.course_code).HasMaxLength(20);
            entity.Property(e => e.is_open).HasDefaultValue(true);
            entity.Property(e => e.meta)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");

            entity.HasOne(d => d.course_codeNavigation).WithMany(p => p.course_offerings)
                .HasForeignKey(d => d.course_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("course_offerings_course_code_fkey");

            entity.HasOne(d => d.term_codeNavigation).WithMany(p => p.course_offerings)
                .HasForeignKey(d => d.term_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("course_offerings_term_code_fkey");
        });

        modelBuilder.Entity<curriculum>(entity =>
        {
            entity.HasKey(e => e.curriculum_id).HasName("curricula_pkey");

            entity.ToTable("curricula", "acad");

            entity.HasIndex(e => new { e.program_id, e.cohort_id }, "curricula_program_id_cohort_id_key").IsUnique();

            // ORDBMS: composite type array — acad.knowledge_block[]
            entity.Property(e => e.structure)
                .HasColumnType("acad.knowledge_block[]")
                .HasDefaultValueSql("'{}'");

            entity.Property(e => e.course_mapping)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.meta)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
            entity.Property(e => e.total_credits).HasPrecision(6, 1);
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.updated_at).HasDefaultValueSql("now()");

            entity.HasOne(d => d.program).WithMany()
                .HasForeignKey(d => d.program_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("curricula_program_id_fkey");

            entity.HasOne(d => d.cohort).WithMany()
                .HasForeignKey(d => d.cohort_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("curricula_cohort_id_fkey");
        });

        modelBuilder.Entity<curriculum_category>(entity =>
        {
            entity.HasKey(e => e.category_id).HasName("curriculum_categories_pkey");

            entity.ToTable("curriculum_categories", "acad");

            entity.HasIndex(e => new { e.program_id, e.category_name }, "curriculum_categories_program_id_category_name_key").IsUnique();

            entity.HasIndex(e => new { e.program_id, e.sort_order }, "idx_categories_program_order");

            entity.Property(e => e.min_credits).HasPrecision(5, 1);

            entity.HasOne(d => d.program).WithMany(p => p.curriculum_categories)
                .HasForeignKey(d => d.program_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("curriculum_categories_program_id_fkey");
        });

        modelBuilder.Entity<curriculum_requirement>(entity =>
        {
            entity.HasKey(e => e.requirement_id).HasName("curriculum_requirements_pkey");

            entity.ToTable("curriculum_requirements", "acad");

            entity.HasIndex(e => e.prereq_rule, "gin_req_prereq_rule").HasMethod("gin");

            entity.HasIndex(e => new { e.cohort_id, e.category_id }, "idx_req_cohort_category");

            entity.HasIndex(e => e.course_code, "idx_req_course").HasFilter("(kind = 'course'::acad.requirement_kind)");

            entity.HasIndex(e => new { e.cohort_id, e.course_code }, "uq_req_course_once_per_cohort")
                .IsUnique()
                .HasFilter("((kind = 'course'::acad.requirement_kind) AND (effective_term_from IS NULL) AND (effective_term_to IS NULL))");

            entity.Property(e => e.course_code).HasMaxLength(20);
            entity.Property(e => e.is_required).HasDefaultValue(true);
            entity.Property(e => e.kind).HasColumnType("acad.requirement_kind");
            entity.Property(e => e.min_credits).HasPrecision(5, 1);
            entity.Property(e => e.prereq_rule).HasColumnType("jsonb");
            entity.Property(e => e.allowed_courses).HasColumnType("acad.course_code[]");

            entity.HasOne(d => d.category).WithMany(p => p.curriculum_requirements)
                .HasForeignKey(d => d.category_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("curriculum_requirements_category_id_fkey");

            entity.HasOne(d => d.cohort).WithMany(p => p.curriculum_requirements)
                .HasForeignKey(d => d.cohort_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("curriculum_requirements_cohort_id_fkey");

            entity.HasOne(d => d.course_codeNavigation).WithMany(p => p.curriculum_requirements)
                .HasForeignKey(d => d.course_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("curriculum_requirements_course_code_fkey");

            entity.HasOne(d => d.effective_term_fromNavigation).WithMany(p => p.curriculum_requirementeffective_term_fromNavigations)
                .HasForeignKey(d => d.effective_term_from)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("curriculum_requirements_effective_term_from_fkey");

            entity.HasOne(d => d.effective_term_toNavigation).WithMany(p => p.curriculum_requirementeffective_term_toNavigations)
                .HasForeignKey(d => d.effective_term_to)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("curriculum_requirements_effective_term_to_fkey");
        });

        modelBuilder.Entity<equivalency>(entity =>
        {
            entity.HasKey(e => new { e.equiv_set_id, e.course_code, e.equivalent_course_code, e.cohort_id }).HasName("equivalencies_pkey");

            entity.ToTable("equivalencies", "acad");

            entity.HasIndex(e => new { e.course_code, e.cohort_id }, "idx_equiv_lookup");

            entity.HasIndex(e => new { e.equivalent_course_code, e.cohort_id }, "idx_equiv_reverse_lookup");

            entity.Property(e => e.course_code).HasMaxLength(20);
            entity.Property(e => e.equivalent_course_code).HasMaxLength(20);

            entity.HasOne(d => d.cohort).WithMany(p => p.equivalencies)
                .HasForeignKey(d => d.cohort_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("equivalencies_cohort_id_fkey");

            entity.HasOne(d => d.course_codeNavigation).WithMany(p => p.equivalencycourse_codeNavigations)
                .HasForeignKey(d => d.course_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("equivalencies_course_code_fkey");

            entity.HasOne(d => d.equiv_set).WithMany(p => p.equivalencies)
                .HasForeignKey(d => d.equiv_set_id)
                .HasConstraintName("equivalencies_equiv_set_id_fkey");

            entity.HasOne(d => d.equivalent_course_codeNavigation).WithMany(p => p.equivalencyequivalent_course_codeNavigations)
                .HasForeignKey(d => d.equivalent_course_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("equivalencies_equivalent_course_code_fkey");
        });

        modelBuilder.Entity<equivalency_set>(entity =>
        {
            entity.HasKey(e => e.equiv_set_id).HasName("equivalency_sets_pkey");

            entity.ToTable("equivalency_sets", "acad");

            entity.HasOne(d => d.program).WithMany(p => p.equivalency_sets)
                .HasForeignKey(d => d.program_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("equivalency_sets_program_id_fkey");
        });

        modelBuilder.Entity<program>(entity =>
        {
            entity.HasKey(e => e.program_id).HasName("programs_pkey");

            entity.ToTable("programs", "acad");

            entity.HasIndex(e => e.program_code, "programs_program_code_key").IsUnique();

            entity.Property(e => e.default_target_credits).HasPrecision(6, 1);
            entity.Property(e => e.meta)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");
        });

        modelBuilder.Entity<student>(entity =>
        {
            entity.HasKey(e => e.student_id).HasName("students_pkey");

            entity.ToTable("students", "acad");

            entity.HasIndex(e => e.meta, "gin_students_meta").HasMethod("gin");

            entity.HasIndex(e => new { e.program_id, e.cohort_id }, "idx_students_program_cohort");

            entity.Property(e => e.student_id).HasMaxLength(20);
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.ielts_score).HasPrecision(3, 1);
            entity.Property(e => e.meta)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb");

            entity.HasOne(d => d.cohort).WithMany(p => p.students)
                .HasForeignKey(d => d.cohort_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("students_cohort_id_fkey");

            entity.HasOne(d => d.program).WithMany(p => p.students)
                .HasForeignKey(d => d.program_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("students_program_id_fkey");
        });

        modelBuilder.Entity<student_concentration>(entity =>
        {
            entity.HasKey(e => e.student_concentration_id).HasName("student_concentrations_pkey");

            entity.ToTable("student_concentrations", "acad");

            entity.HasIndex(e => new { e.student_id, e.concentration_id }, "student_concentrations_student_id_concentration_id_key").IsUnique();

            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.status).HasDefaultValueSql("'active'::text");
            entity.Property(e => e.student_id).HasMaxLength(20);

            entity.HasOne(d => d.approved_term_codeNavigation).WithMany(p => p.student_concentrations)
                .HasForeignKey(d => d.approved_term_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("student_concentrations_approved_term_code_fkey");

            entity.HasOne(d => d.concentration).WithMany(p => p.student_concentrations)
                .HasForeignKey(d => d.concentration_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("student_concentrations_concentration_id_fkey");

            entity.HasOne(d => d.student).WithMany(p => p.student_concentrations)
                .HasForeignKey(d => d.student_id)
                .HasConstraintName("student_concentrations_student_id_fkey");
        });

        modelBuilder.Entity<student_plan>(entity =>
        {
            entity.HasKey(e => e.plan_id).HasName("student_plans_pkey");

            entity.ToTable("student_plans", "acad");

            entity.HasIndex(e => new { e.student_id, e.term_code }, "idx_plans_student_term");

            entity.HasIndex(e => new { e.student_id, e.term_code, e.course_code }, "student_plans_student_id_term_code_course_code_key").IsUnique();

            entity.Property(e => e.course_code).HasMaxLength(20);
            entity.Property(e => e.created_at).HasDefaultValueSql("now()");
            entity.Property(e => e.status)
                .HasColumnType("acad.item_status")
                .HasDefaultValueSql("'planned'::acad.item_status");
            entity.Property(e => e.student_id).HasMaxLength(20);

            entity.HasOne(d => d.course_codeNavigation).WithMany(p => p.student_plans)
                .HasForeignKey(d => d.course_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("student_plans_course_code_fkey");

            entity.HasOne(d => d.student).WithMany(p => p.student_plans)
                .HasForeignKey(d => d.student_id)
                .HasConstraintName("student_plans_student_id_fkey");

            entity.HasOne(d => d.term_codeNavigation).WithMany(p => p.student_plans)
                .HasForeignKey(d => d.term_code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("student_plans_term_code_fkey");
        });

        modelBuilder.Entity<term>(entity =>
        {
            entity.HasKey(e => e.term_code).HasName("terms_pkey");

            entity.ToTable("terms", "acad");

            entity.HasIndex(e => new { e.year, e.term_no }, "idx_terms_year_term");

            entity.Property(e => e.term_code).ValueGeneratedNever();
            entity.Property(e => e.term_no).HasComputedColumnSql("((term_code)::integer % 10)", true);
            entity.Property(e => e.year).HasComputedColumnSql("((term_code)::integer / 10)", true);
        });

        modelBuilder.Entity<v_latest_attempt>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_latest_attempt", "acad");

            entity.Property(e => e.course_code).HasMaxLength(20);
            entity.Property(e => e.credits).HasPrecision(4, 1);
            entity.Property(e => e.grade_letter).HasColumnType("acad.grade_letter");
            entity.Property(e => e.student_id).HasMaxLength(20);
        });

        modelBuilder.Entity<v_student_audit>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_student_audit", "acad");

            entity.Property(e => e.course_code).HasMaxLength(20);
            entity.Property(e => e.min_credits).HasPrecision(5, 1);
            entity.Property(e => e.prereq_rule).HasColumnType("jsonb");
            entity.Property(e => e.student_id).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
