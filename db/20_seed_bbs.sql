BEGIN;

INSERT INTO acad.programs (program_code, program_name, degree_level, default_target_credits)
VALUES ('BBS', 'Business Administration', 'Bachelor', 195)
ON CONFLICT (program_code) DO NOTHING;

INSERT INTO acad.cohorts (program_id, cohort_code, start_year, note)
SELECT p.program_id, 'K2025', 2025, 'BBS cohort seed'
FROM acad.programs p
WHERE p.program_code = 'BBS'
ON CONFLICT (program_id, cohort_code) DO NOTHING;

INSERT INTO acad.terms (term_code, start_date, end_date) VALUES
  (20251, '2025-01-10', '2025-05-10'),
  (20252, '2025-08-10', '2025-12-10'),
  (20261, '2026-01-10', '2026-05-10'),
  (20262, '2026-08-10', '2026-12-10')
ON CONFLICT (term_code) DO NOTHING;

INSERT INTO acad.courses (course_code, course_name, credits, subject_prefix, course_level, is_language_prep)
VALUES
  ('BUS 101', 'Business Fundamentals', 3, 'BUS', 100, false),
  ('WRT 122', 'Academic Writing', 3, 'WRT', 100, false),
  ('MTH 171', 'Business Mathematics', 3, 'MTH', 100, false),
  ('ECO 204', 'Economics', 3, 'ECO', 200, false),
  ('ACTG 240', 'Accounting Principles', 3, 'ACTG', 200, false),
  ('BUS 303', 'Business Communication', 3, 'BUS', 300, false),
  ('BUS 314', 'Organizational Behavior', 3, 'BUS', 300, false),
  ('BUS 328', 'Financial Management', 3, 'BUS', 300, false),
  ('BUS 332', 'Marketing Fundamentals', 3, 'BUS', 300, false),
  ('BUS 357', 'Entrepreneurship', 3, 'BUS', 300, false),
  ('BUS 359', 'Supply Chain Management', 3, 'BUS', 300, false),
  ('BUS 450', 'Internship', 6, 'BUS', 400, false),
  ('BUS 496', 'Capstone', 6, 'BUS', 400, false)
ON CONFLICT (course_code) DO NOTHING;

INSERT INTO acad.concentrations (program_id, concentration_code, concentration_name, min_credits)
SELECT p.program_id, x.code, x.name, 20
FROM acad.programs p
CROSS JOIN (
  VALUES
    ('MKT', 'Marketing'),
    ('SCM', 'Supply Chain Management'),
    ('HRM', 'Human Resource Management'),
    ('HOS', 'Hospitality'),
    ('FIN', 'Finance'),
    ('ACC', 'Accounting'),
    ('ENT', 'Entrepreneurship')
) AS x(code, name)
WHERE p.program_code = 'BBS'
ON CONFLICT (program_id, concentration_code) DO NOTHING;

INSERT INTO acad.course_offerings (term_code, course_code, is_open, registration_channel)
VALUES
  (20262, 'BUS 303', true, 'portal'),
  (20262, 'BUS 314', true, 'portal'),
  (20262, 'BUS 328', true, 'portal'),
  (20262, 'BUS 332', true, 'portal'),
  (20262, 'BUS 357', true, 'portal'),
  (20262, 'BUS 359', true, 'portal'),
  (20262, 'BUS 450', true, 'manual_email'),
  (20262, 'BUS 496', true, 'portal')
ON CONFLICT (term_code, course_code) DO NOTHING;

INSERT INTO acad.course_advisories (course_code, advisory_type, rule_json, note)
VALUES
  ('BUS 450', 'manual_registration', '{"registrationChannel":"manual_email","phase":"start"}', 'BUS 450 start requires faculty email registration'),
  ('BUS 496', 'capstone_load_limit', '{"max_parallel_courses":2}', 'Take BUS 496 with at most 1-2 additional courses')
ON CONFLICT DO NOTHING;

-- ============================================================
-- BBS Curriculum Categories
-- ============================================================

INSERT INTO acad.curriculum_categories (program_id, category_name, min_credits, sort_order)
SELECT p.program_id, x.cat_name, x.min_cr, x.ord
FROM acad.programs p
CROSS JOIN (VALUES
  ('General Foundation',   45, 1),
  ('Business Core',        60, 2),
  ('Concentration',        20, 3),
  ('Electives',            15, 4),
  ('Internship & Capstone',12, 5)
) AS x(cat_name, min_cr, ord)
WHERE p.program_code = 'BBS'
ON CONFLICT (program_id, category_name) DO NOTHING;

-- ============================================================
-- BBS Curriculum Requirements (K2025 cohort) — op/args format
-- ============================================================

-- General Foundation: WRT 122, MTH 171 (no prereqs)
INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, course_code, prereq_rule, is_required)
SELECT co.cohort_id, cat.category_id, 'course', x.code, x.prereq::jsonb, true
FROM acad.cohorts co
JOIN acad.programs p ON p.program_id = co.program_id
JOIN acad.curriculum_categories cat ON cat.program_id = p.program_id
CROSS JOIN (VALUES
  ('WRT 122', NULL),
  ('MTH 171', NULL),
  ('ECO 204', NULL)
) AS x(code, prereq)
WHERE p.program_code = 'BBS' AND co.cohort_code = 'K2025'
  AND cat.category_name = 'General Foundation'
ON CONFLICT DO NOTHING;

-- Business Core: BUS 101 (no prereq), BUS 303 (prereq: WRT 122 AND BUS 101)
INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, course_code, prereq_rule, is_required)
SELECT co.cohort_id, cat.category_id, 'course', x.code, x.prereq::jsonb, true
FROM acad.cohorts co
JOIN acad.programs p ON p.program_id = co.program_id
JOIN acad.curriculum_categories cat ON cat.program_id = p.program_id
CROSS JOIN (VALUES
  ('BUS 101', NULL),
  ('ACTG 240', '{"op":"COMPLETED","course":"BUS 101"}'),
  ('BUS 303', '{"op":"AND","args":[{"op":"COMPLETED","course":"WRT 122"},{"op":"COMPLETED","course":"BUS 101"}]}'),
  ('BUS 314', '{"op":"COMPLETED","course":"BUS 101"}'),
  ('BUS 328', '{"op":"AND","args":[{"op":"COMPLETED","course":"ACTG 240"},{"op":"COMPLETED","course":"ECO 204"}]}'),
  ('BUS 332', '{"op":"COMPLETED","course":"BUS 101"}'),
  ('BUS 357', '{"op":"AND","args":[{"op":"COMPLETED","course":"BUS 303"},{"op":"COMPLETED","course":"BUS 314"}]}'),
  ('BUS 359', '{"op":"AND","args":[{"op":"COMPLETED","course":"BUS 303"},{"op":"COMPLETED","course":"BUS 332"}]}')
) AS x(code, prereq)
WHERE p.program_code = 'BBS' AND co.cohort_code = 'K2025'
  AND cat.category_name = 'Business Core'
ON CONFLICT DO NOTHING;

-- Internship & Capstone
INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, course_code, prereq_rule, is_required)
SELECT co.cohort_id, cat.category_id, 'course', x.code, x.prereq::jsonb, true
FROM acad.cohorts co
JOIN acad.programs p ON p.program_id = co.program_id
JOIN acad.curriculum_categories cat ON cat.program_id = p.program_id
CROSS JOIN (VALUES
  ('BUS 450', '{"op":"MIN_CREDITS","value":90}'),
  ('BUS 496', '{"op":"AND","args":[{"op":"COMPLETED","course":"BUS 450"},{"op":"MIN_CREDITS","value":150},{"op":"ENGLISH","min_level":6,"min_ielts":6.0}]}')
) AS x(code, prereq)
WHERE p.program_code = 'BBS' AND co.cohort_code = 'K2025'
  AND cat.category_name = 'Internship & Capstone'
ON CONFLICT DO NOTHING;

-- ============================================================
-- Sync curricula table (populate ORDBMS knowledge_block[] + JSONB)
-- ============================================================

DO $$
DECLARE
  v_program_id bigint;
  v_cohort_id  bigint;
BEGIN
  SELECT program_id INTO v_program_id FROM acad.programs WHERE program_code = 'BBS';
  SELECT cohort_id INTO v_cohort_id FROM acad.cohorts
    WHERE program_id = v_program_id AND cohort_code = 'K2025';
  IF v_program_id IS NOT NULL AND v_cohort_id IS NOT NULL THEN
    PERFORM acad.sync_curriculum_structure(v_program_id, v_cohort_id);
  END IF;

  SELECT program_id INTO v_program_id FROM acad.programs WHERE program_code = 'PM';
  SELECT cohort_id INTO v_cohort_id FROM acad.cohorts
    WHERE program_id = v_program_id AND cohort_code = 'K13';
  IF v_program_id IS NOT NULL AND v_cohort_id IS NOT NULL THEN
    PERFORM acad.sync_curriculum_structure(v_program_id, v_cohort_id);
  END IF;
END $$;

COMMIT;
