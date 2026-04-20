BEGIN;

-- ============================================================
-- 1) Programs
-- ============================================================

INSERT INTO acad.programs (program_code, program_name, degree_level, default_target_credits)
VALUES
('PM',  'Political Management', 'Bachelor', 120),
('MMT', 'Multimedia Technology', 'Bachelor', 130);

-- ============================================================
-- 2) Cohorts
-- ============================================================

INSERT INTO acad.cohorts (program_id, cohort_code, start_year, note)
SELECT program_id, 'K13', 2021, 'Cohort 13'
FROM acad.programs
WHERE program_code = 'PM';

INSERT INTO acad.cohorts (program_id, cohort_code, start_year)
SELECT program_id, 'K14', 2022
FROM acad.programs
WHERE program_code = 'PM';

-- ============================================================
-- 3) Terms
-- ============================================================

INSERT INTO acad.terms (term_code, start_date, end_date) VALUES
(20211, '2021-01-10', '2021-05-15'),
(20212, '2021-08-01', '2021-12-10'),
(20221, '2022-01-10', '2022-05-15'),
(20222, '2022-08-01', '2022-12-10');

-- ============================================================
-- 4) Courses
-- ============================================================

INSERT INTO acad.courses
(course_code, course_name, credits, subject_prefix, course_level, is_language_prep)
VALUES
('POLS101', 'Introduction to Political Science', 3, 'POLS', 100, false),
('POLS201', 'Political Theory', 3, 'POLS', 200, false),
('LAW101',  'Introduction to Law', 3, 'LAW', 100, false),
('ENG001',  'English Level 1', 2, 'ENG', 0, true),
('ENG005',  'English Level 5', 2, 'ENG', 0, true),
('ELEC001', 'Elective Course A', 3, 'ELEC', 100, false),
('ELEC002', 'Elective Course B', 3, 'ELEC', 100, false);

-- ============================================================
-- 5) Curriculum Categories
-- ============================================================

INSERT INTO acad.curriculum_categories
(program_id, category_name, min_credits, sort_order)
SELECT program_id, 'General Education', 30, 1
FROM acad.programs WHERE program_code = 'PM';

INSERT INTO acad.curriculum_categories
(program_id, category_name, min_credits, sort_order)
SELECT program_id, 'Major Core', 60, 2
FROM acad.programs WHERE program_code = 'PM';

INSERT INTO acad.curriculum_categories
(program_id, category_name, min_credits, sort_order)
SELECT program_id, 'Electives', 15, 3
FROM acad.programs WHERE program_code = 'PM';

-- ============================================================
-- 6) Curriculum Requirements
-- ============================================================

-- === Required course: POLS101 (no prerequisite)
INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule)
SELECT
  c.cohort_id,
  cat.category_id,
  'course',
  'POLS101',
  NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat
  ON cat.program_id = c.program_id
WHERE c.cohort_code = 'K13'
  AND cat.category_name = 'General Education';

-- === Required course with op/args prerequisite
INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule)
SELECT
  c.cohort_id,
  cat.category_id,
  'course',
  'POLS201',
  '{"op":"AND","args":[{"op":"COMPLETED","course":"POLS101"}]}'
FROM acad.cohorts c
JOIN acad.curriculum_categories cat
  ON cat.program_id = c.program_id
WHERE c.cohort_code = 'K13'
  AND cat.category_name = 'Major Core';

-- === Credit bucket (Electives)
INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, min_credits, allowed_courses, note)
SELECT
  c.cohort_id,
  cat.category_id,
  'credit_bucket',
  15,
  ARRAY['ELEC001','ELEC002']::acad.course_code[],
  'Choose any electives'
FROM acad.cohorts c
JOIN acad.curriculum_categories cat
  ON cat.program_id = c.program_id
WHERE c.cohort_code = 'K13'
  AND cat.category_name = 'Electives';

-- ============================================================
-- 7) Equivalency
-- ============================================================

INSERT INTO acad.equivalency_sets (program_id, title)
SELECT program_id, 'Elective Equivalency'
FROM acad.programs WHERE program_code = 'PM';

INSERT INTO acad.equivalencies
(equiv_set_id, course_code, equivalent_course_code)
SELECT
  es.equiv_set_id,
  'ELEC001',
  'ELEC002'
FROM acad.equivalency_sets es;

-- ============================================================
-- 8) Students
-- ============================================================

INSERT INTO acad.students
(student_id, last_name, first_name, program_id, cohort_id, english_level, ielts_score, status)
SELECT
  '20210001',
  'Nguyen',
  'An',
  p.program_id,
  c.cohort_id,
  5,
  6.0,
  'active'
FROM acad.programs p
JOIN acad.cohorts c ON c.program_id = p.program_id
WHERE p.program_code = 'PM'
  AND c.cohort_code = 'K13';

-- ============================================================
-- 9) Course Attempts
-- ============================================================

-- POLS101 completed
INSERT INTO acad.course_attempts
(student_id, course_code, term_code, attempt_no, credits, grade_letter, is_completed)
VALUES
('20210001', 'POLS101', 20211, 1, 3, 'B', true);

-- POLS201 in progress
INSERT INTO acad.course_attempts
(student_id, course_code, term_code, attempt_no, credits, grade_letter, is_completed)
VALUES
('20210001', 'POLS201', 20221, 1, 3, NULL, false);

-- ============================================================
-- 10) Student Plans
-- ============================================================

INSERT INTO acad.student_plans
(student_id, term_code, course_code, status)
VALUES
('20210001', 20222, 'ELEC001', 'planned'),
('20210001', 20222, 'ELEC002', 'planned');

COMMIT;