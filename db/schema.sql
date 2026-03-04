-- ============================================================
-- ORDBMS Schema for Curriculum + Student Audit (PostgreSQL)
-- - JSONB polymorphic prerequisite rules
-- - Elective pool stored as course_code[]
-- - Strong constraints & indexes for audit queries
-- ============================================================

BEGIN;

-- ----------------------------
-- 0) Namespace
-- ----------------------------
CREATE SCHEMA IF NOT EXISTS acad;

-- ----------------------------
-- 1) Domains & Enums
-- ----------------------------

-- Term code like 20251, 20252, 20253...
CREATE DOMAIN acad.term_code AS int
CHECK (VALUE BETWEEN 20000 AND 29999 AND (VALUE % 10) IN (1,2,3));

-- Student id (MSSV) keep as text to avoid scientific notation issues from Excel
CREATE DOMAIN acad.student_id AS varchar(20)
CHECK (VALUE ~ '^[0-9]{6,20}$');

-- Course code: allow "POLS 132" / "POLS132" / "IELP 3"
CREATE DOMAIN acad.course_code AS varchar(20)
CHECK (
  VALUE ~ '^[A-Z]{2,8}[ ]?[0-9]{1,4}$'
);

-- Grade letter
CREATE TYPE acad.grade_letter AS ENUM (
  'A','A-','B+','B','B-','C+','C','C-','D+','D','D-','F','P','W','I'
);

-- Requirement kind
CREATE TYPE acad.requirement_kind AS ENUM (
  'course',        -- must take a specific course
  'credit_bucket'  -- must reach >= N credits in a category or pool
);

-- Planning/audit status (optional but useful)
CREATE TYPE acad.item_status AS ENUM (
  'planned','in_progress','completed','waived','failed'
);

-- ----------------------------
-- 1) Master: Faculty
-- ----------------------------


-- ----------------------------
-- 2) Master: Program / Cohort / Term
-- ----------------------------

CREATE TABLE acad.programs (
  program_id            bigserial PRIMARY KEY,
  program_code          text NOT NULL UNIQUE,   -- e.g., PM, MMT, CDT, BBS
  program_name          text NOT NULL,
  degree_level          text,
  default_target_credits numeric(6,1) CHECK (default_target_credits IS NULL OR default_target_credits >= 0),
  meta                 jsonb NOT NULL DEFAULT '{}'::jsonb
);

CREATE TABLE acad.cohorts (
  cohort_id    bigserial PRIMARY KEY,
  program_id   bigint NOT NULL REFERENCES acad.programs(program_id) ON DELETE RESTRICT,
  cohort_code  text NOT NULL,     -- e.g., K8-12, K13
  start_year   int CHECK (start_year IS NULL OR start_year BETWEEN 1990 AND 2100),
  note         text,
  UNIQUE(program_id, cohort_code)
);

CREATE TABLE acad.terms (
  term_code  acad.term_code PRIMARY KEY,
  year       int GENERATED ALWAYS AS (term_code / 10) STORED,
  term_no    int GENERATED ALWAYS AS (term_code % 10) STORED,
  start_date date,
  end_date   date,
  CHECK (start_date IS NULL OR end_date IS NULL OR start_date <= end_date)
);

-- Helpful index for range queries by year/term
CREATE INDEX IF NOT EXISTS idx_terms_year_term ON acad.terms (year, term_no);

-- ----------------------------
-- 3) Course Catalog
-- ----------------------------

CREATE TABLE acad.courses (
  course_code     acad.course_code PRIMARY KEY,
  course_name     text,
  credits         numeric(4,1) CHECK (credits IS NULL OR credits >= 0),
  subject_prefix  text,      -- optional derived: POLS/IELP/CSE...
  course_level    int CHECK (course_level IS NULL OR course_level BETWEEN 0 AND 999),
  is_language_prep boolean NOT NULL DEFAULT false,
  meta            jsonb NOT NULL DEFAULT '{}'::jsonb
);

-- Search / filter indexes
CREATE INDEX IF NOT EXISTS idx_courses_prefix ON acad.courses (subject_prefix);
CREATE INDEX IF NOT EXISTS idx_courses_level  ON acad.courses (course_level);
CREATE INDEX IF NOT EXISTS gin_courses_meta   ON acad.courses USING GIN (meta);

-- ----------------------------
-- 4) Curriculum: Category + Requirements
-- ----------------------------

CREATE TABLE acad.curriculum_categories (
  category_id   bigserial PRIMARY KEY,
  program_id    bigint NOT NULL REFERENCES acad.programs(program_id) ON DELETE RESTRICT,
  category_name text NOT NULL,     -- e.g., "Political theory and laws"
  min_credits   numeric(5,1) CHECK (min_credits IS NULL OR min_credits >= 0),
  sort_order    int,
  UNIQUE(program_id, category_name)
);

CREATE INDEX IF NOT EXISTS idx_categories_program_order
ON acad.curriculum_categories (program_id, sort_order NULLS LAST);

-- Curriculum requirements (per cohort, per category)
-- prereq_rule is polymorphic JSON object:
--   {"type":"course","courses":[{"code":"WRT 122","min_grade":"C"}], "raw":"WRT 122"}
--   {"type":"english","min_level":5,"min_ielts":5.5,"raw":"Level 5/IELTS 5.5+"}
-- elective pool is stored as course_code[] (optional)
CREATE TABLE acad.curriculum_requirements (
  requirement_id bigserial PRIMARY KEY,

  cohort_id      bigint NOT NULL REFERENCES acad.cohorts(cohort_id) ON DELETE RESTRICT,
  category_id    bigint NOT NULL REFERENCES acad.curriculum_categories(category_id) ON DELETE RESTRICT,

  kind           acad.requirement_kind NOT NULL,

  -- for kind='course'
  course_code    acad.course_code NULL REFERENCES acad.courses(course_code) ON DELETE RESTRICT,

  -- for kind='credit_bucket'
  min_credits    numeric(5,1) NULL CHECK (min_credits IS NULL OR min_credits >= 0),

  is_required    boolean NOT NULL DEFAULT true,

  -- ORDBMS "object" for rules
  prereq_rule    jsonb NULL,

  -- elective pool / allowed list (mainly for credit buckets; also allowed for course if needed)
  allowed_courses acad.course_code[] NULL,

  -- Optional effective term window (for versioning)
  effective_term_from acad.term_code NULL REFERENCES acad.terms(term_code) ON DELETE RESTRICT,
  effective_term_to   acad.term_code NULL REFERENCES acad.terms(term_code) ON DELETE RESTRICT,

  note          text,

  -- Constraints enforcing kind semantics
  CHECK (
    (kind = 'course' AND course_code IS NOT NULL AND min_credits IS NULL)
    OR
    (kind = 'credit_bucket' AND min_credits IS NOT NULL AND course_code IS NULL)
  ),
  CHECK (
    effective_term_from IS NULL OR effective_term_to IS NULL OR effective_term_from <= effective_term_to
  )
);

-- Common audit query patterns: by cohort/category; by course_code; rule filtering
CREATE INDEX IF NOT EXISTS idx_req_cohort_category
ON acad.curriculum_requirements (cohort_id, category_id);

CREATE INDEX IF NOT EXISTS idx_req_course
ON acad.curriculum_requirements (course_code)
WHERE kind = 'course';

-- GIN index for JSONB prerequisite rule (filter on rule.type, rule fields)
CREATE INDEX IF NOT EXISTS gin_req_prereq_rule
ON acad.curriculum_requirements USING GIN (prereq_rule);

-- GIN index for elective pool membership queries (course_code = ANY(allowed_courses))
CREATE INDEX IF NOT EXISTS gin_req_allowed_courses
ON acad.curriculum_requirements USING GIN (allowed_courses);

-- Ensure a cohort doesn't duplicate the same course requirement twice in same effective window (soft guarantee)
-- Note: cannot fully enforce overlap without exclusion constraints; this is a practical uniqueness.
CREATE UNIQUE INDEX IF NOT EXISTS uq_req_course_once_per_cohort
ON acad.curriculum_requirements (cohort_id, course_code)
WHERE kind = 'course' AND effective_term_from IS NULL AND effective_term_to IS NULL;

-- ----------------------------
-- 5) Equivalency / Substitution (Course <-> Course)
-- ----------------------------

CREATE TABLE acad.equivalency_sets (
  equiv_set_id bigserial PRIMARY KEY,
  program_id   bigint NULL REFERENCES acad.programs(program_id) ON DELETE RESTRICT,
  title        text NOT NULL,
  note         text
);

-- Each row states: course_code can be substituted by equivalent_course_code
-- Scope can be narrowed by cohort_id if needed.
CREATE TABLE acad.equivalencies (
  equiv_set_id bigint NOT NULL REFERENCES acad.equivalency_sets(equiv_set_id) ON DELETE CASCADE,
  course_code  acad.course_code NOT NULL REFERENCES acad.courses(course_code) ON DELETE RESTRICT,
  equivalent_course_code acad.course_code NOT NULL REFERENCES acad.courses(course_code) ON DELETE RESTRICT,
  cohort_id    bigint NULL REFERENCES acad.cohorts(cohort_id) ON DELETE RESTRICT,
  note         text,
  PRIMARY KEY (equiv_set_id, course_code, equivalent_course_code, cohort_id),
  CHECK (course_code <> equivalent_course_code)
);

CREATE INDEX IF NOT EXISTS idx_equiv_lookup
ON acad.equivalencies (course_code, cohort_id);

CREATE INDEX IF NOT EXISTS idx_equiv_reverse_lookup
ON acad.equivalencies (equivalent_course_code, cohort_id);

-- ----------------------------
-- 6) Students
-- ----------------------------

CREATE TABLE acad.students (
  student_id    acad.student_id PRIMARY KEY,
  last_name     text,
  first_name    text,

  program_id    bigint NULL REFERENCES acad.programs(program_id) ON DELETE SET NULL,
  cohort_id     bigint NULL REFERENCES acad.cohorts(cohort_id) ON DELETE SET NULL,

  status        text,                  -- active/stop/graduated...
  english_level int CHECK (english_level IS NULL OR english_level BETWEEN 0 AND 20),
  ielts_score   numeric(3,1) CHECK (ielts_score IS NULL OR (ielts_score >= 0 AND ielts_score <= 9.0)),

  meta          jsonb NOT NULL DEFAULT '{}'::jsonb,

  -- Data integrity: if cohort_id set, it should belong to same program_id (if program_id set).
  -- This cannot be enforced by a simple FK; use a trigger in stricter setups.
  -- We'll keep it as a comment and enforce at ETL/application layer.
  created_at    timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_students_program_cohort
ON acad.students (program_id, cohort_id);

CREATE INDEX IF NOT EXISTS gin_students_meta
ON acad.students USING GIN (meta);

-- ----------------------------
-- 7) Fact: Course Attempts / Enrollments
-- ----------------------------

CREATE TABLE acad.course_attempts (
  attempt_id   bigserial PRIMARY KEY,

  student_id   acad.student_id NOT NULL REFERENCES acad.students(student_id) ON DELETE CASCADE,
  course_code  acad.course_code NOT NULL REFERENCES acad.courses(course_code) ON DELETE RESTRICT,
  term_code    acad.term_code NOT NULL REFERENCES acad.terms(term_code) ON DELETE RESTRICT,

  term_seq     int CHECK (term_seq IS NULL OR term_seq BETWEEN 0 AND 20),
  attempt_no   int NOT NULL DEFAULT 1 CHECK (attempt_no BETWEEN 1 AND 20),

  credits      numeric(4,1) CHECK (credits IS NULL OR credits >= 0),
  grade_letter acad.grade_letter NULL,
  is_completed boolean NOT NULL DEFAULT false,

  -- Snapshot fields (optional, from Excel export)
  snapshot_cum_credits    numeric(6,1) CHECK (snapshot_cum_credits IS NULL OR snapshot_cum_credits >= 0),
  snapshot_target_credits numeric(6,1) CHECK (snapshot_target_credits IS NULL OR snapshot_target_credits >= 0),
  snapshot_cum_gpa        numeric(3,2) CHECK (snapshot_cum_gpa IS NULL OR (snapshot_cum_gpa >= 0 AND snapshot_cum_gpa <= 4.00)),

  -- lineage
  source_file   text,
  source_rowkey text,
  raw_record    jsonb NOT NULL DEFAULT '{}'::jsonb,

  created_at    timestamptz NOT NULL DEFAULT now(),

  -- Enforce no duplicate attempt rows
  UNIQUE(student_id, course_code, term_code, attempt_no),

  -- If grade present and is_completed false -> allow (in-progress grade). If strict, uncomment:
  -- CHECK (NOT (grade_letter IS NOT NULL AND is_completed = false))
  CHECK (true)
);

-- High-impact indexes for audit/performance
CREATE INDEX IF NOT EXISTS idx_attempts_student_term
ON acad.course_attempts (student_id, term_code);

CREATE INDEX IF NOT EXISTS idx_attempts_student_course
ON acad.course_attempts (student_id, course_code);

CREATE INDEX IF NOT EXISTS idx_attempts_course_term
ON acad.course_attempts (course_code, term_code);

-- Useful for "latest attempt" queries
CREATE INDEX IF NOT EXISTS idx_attempts_latest
ON acad.course_attempts (student_id, course_code, term_code DESC, attempt_no DESC);

CREATE INDEX IF NOT EXISTS gin_attempts_raw_record
ON acad.course_attempts USING GIN (raw_record);

-- ----------------------------
-- 8) Optional: Student Plans (HK hiện tại / HK sau)
-- ----------------------------

CREATE TABLE acad.student_plans (
  plan_id     bigserial PRIMARY KEY,
  student_id  acad.student_id NOT NULL REFERENCES acad.students(student_id) ON DELETE CASCADE,
  term_code   acad.term_code NOT NULL REFERENCES acad.terms(term_code) ON DELETE RESTRICT,
  course_code acad.course_code NOT NULL REFERENCES acad.courses(course_code) ON DELETE RESTRICT,
  status      acad.item_status NOT NULL DEFAULT 'planned',
  note        text,
  created_at  timestamptz NOT NULL DEFAULT now(),
  UNIQUE(student_id, term_code, course_code)
);

CREATE INDEX IF NOT EXISTS idx_plans_student_term
ON acad.student_plans (student_id, term_code);

-- ----------------------------
-- 9) Views for Audit (report layer)
-- ----------------------------

-- Latest attempt per (student, course)
CREATE OR REPLACE VIEW acad.v_latest_attempt AS
SELECT DISTINCT ON (student_id, course_code)
  student_id,
  course_code,
  term_code,
  attempt_no,
  grade_letter,
  is_completed,
  credits
FROM acad.course_attempts
ORDER BY student_id, course_code, term_code DESC, attempt_no DESC;

-- Student audit: join requirements with latest attempt
CREATE OR REPLACE VIEW acad.v_student_audit AS
SELECT
  s.student_id,
  s.program_id,
  s.cohort_id,
  cr.requirement_id,
  cc.category_name,
  cr.kind,
  cr.course_code,
  cr.min_credits,
  cr.allowed_courses,
  cr.prereq_rule,
  la.term_code   AS last_term,
  la.grade_letter AS last_grade,
  la.is_completed AS completed
FROM acad.students s
JOIN acad.curriculum_requirements cr
  ON cr.cohort_id = s.cohort_id
JOIN acad.curriculum_categories cc
  ON cc.category_id = cr.category_id
LEFT JOIN acad.v_latest_attempt la
  ON la.student_id = s.student_id
 AND la.course_code = cr.course_code;

COMMIT;
