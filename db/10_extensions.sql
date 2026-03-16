BEGIN;

CREATE TABLE IF NOT EXISTS acad.concentrations (
  concentration_id bigserial PRIMARY KEY,
  program_id bigint NOT NULL REFERENCES acad.programs(program_id) ON DELETE RESTRICT,
  concentration_code text NOT NULL,
  concentration_name text NOT NULL,
  min_credits numeric(5,1) NULL,
  meta jsonb NOT NULL DEFAULT '{}'::jsonb,
  UNIQUE(program_id, concentration_code)
);

CREATE TABLE IF NOT EXISTS acad.concentration_courses (
  concentration_course_id bigserial PRIMARY KEY,
  concentration_id bigint NOT NULL REFERENCES acad.concentrations(concentration_id) ON DELETE CASCADE,
  course_code acad.course_code NOT NULL REFERENCES acad.courses(course_code) ON DELETE RESTRICT,
  is_required boolean NOT NULL DEFAULT true,
  is_entry_course boolean NOT NULL DEFAULT false,
  sort_order int NULL,
  meta jsonb NOT NULL DEFAULT '{}'::jsonb,
  UNIQUE(concentration_id, course_code)
);

CREATE TABLE IF NOT EXISTS acad.student_concentrations (
  student_concentration_id bigserial PRIMARY KEY,
  student_id acad.student_id NOT NULL REFERENCES acad.students(student_id) ON DELETE CASCADE,
  concentration_id bigint NOT NULL REFERENCES acad.concentrations(concentration_id) ON DELETE RESTRICT,
  approved_term_code acad.term_code NULL REFERENCES acad.terms(term_code) ON DELETE RESTRICT,
  status text NOT NULL DEFAULT 'active',
  created_at timestamptz NOT NULL DEFAULT now(),
  UNIQUE(student_id, concentration_id)
);

CREATE TABLE IF NOT EXISTS acad.course_offerings (
  offering_id bigserial PRIMARY KEY,
  term_code acad.term_code NOT NULL REFERENCES acad.terms(term_code) ON DELETE RESTRICT,
  course_code acad.course_code NOT NULL REFERENCES acad.courses(course_code) ON DELETE RESTRICT,
  is_open boolean NOT NULL DEFAULT true,
  registration_channel text NULL,
  meta jsonb NOT NULL DEFAULT '{}'::jsonb,
  UNIQUE(term_code, course_code)
);

CREATE TABLE IF NOT EXISTS acad.course_advisories (
  advisory_id bigserial PRIMARY KEY,
  course_code acad.course_code NOT NULL REFERENCES acad.courses(course_code) ON DELETE RESTRICT,
  advisory_type text NOT NULL,
  rule_json jsonb NOT NULL DEFAULT '{}'::jsonb,
  note text NULL
);

COMMIT;
