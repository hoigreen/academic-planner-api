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

COMMIT;
