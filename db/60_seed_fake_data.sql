BEGIN;

-- ============================================================
-- 1) Term calendar used by curated benchmark scenarios
-- ============================================================

INSERT INTO acad.terms (term_code, start_date, end_date)
VALUES
  (20211, '2021-01-11', '2021-05-08'),
  (20212, '2021-08-16', '2021-12-18'),
  (20221, '2022-01-10', '2022-05-14'),
  (20222, '2022-08-15', '2022-12-17'),
  (20231, '2023-01-09', '2023-05-13'),
  (20232, '2023-08-14', '2023-12-16'),
  (20241, '2024-01-08', '2024-05-11'),
  (20242, '2024-08-12', '2024-12-14'),
  (20251, '2025-01-13', '2025-05-10'),
  (20252, '2025-08-11', '2025-12-13'),
  (20261, '2026-01-12', '2026-05-09'),
  (20262, '2026-08-10', '2026-12-12'),
  (20271, '2027-01-11', '2027-05-08'),
  (20272, '2027-08-09', '2027-12-11'),
  (20281, '2028-01-10', '2028-05-13'),
  (20282, '2028-08-14', '2028-12-16')
ON CONFLICT (term_code) DO NOTHING;

-- ============================================================
-- 2) Programs and cohorts
-- "Faculty/school" is represented in programs.meta because the
-- current schema has no dedicated faculties table.
-- ============================================================

INSERT INTO acad.programs AS prg (program_code, program_name, degree_level, default_target_credits, meta)
VALUES
  ('BBS', 'Business Administration', 'Bachelor', 195, '{"school":"School of Business","campus":"Da Nang","delivery":"full_time"}'::jsonb),
  ('PM', 'Political Management', 'Bachelor', 84, '{"school":"School of Governance and Public Affairs","campus":"Da Nang","delivery":"full_time"}'::jsonb),
  ('MMT', 'Multimedia Technology', 'Bachelor', 92, '{"school":"School of Design and Digital Media","campus":"Da Nang","delivery":"studio_based"}'::jsonb)
ON CONFLICT (program_code) DO UPDATE SET
  program_name = EXCLUDED.program_name,
  degree_level = EXCLUDED.degree_level,
  default_target_credits = EXCLUDED.default_target_credits,
  meta = prg.meta || EXCLUDED.meta;

INSERT INTO acad.cohorts (program_id, cohort_code, start_year, note)
SELECT p.program_id, v.cohort_code, v.start_year, v.note
FROM acad.programs p
JOIN (
  VALUES
    ('BBS', 'K12', 2020, 'Legacy BBS cohort used by imported audit curriculum'),
    ('BBS', 'K13', 2023, 'Current BBS cohort used for benchmark recommendation scenarios'),
    ('BBS', 'K2025', 2025, 'Compact demo cohort for onboarding and API smoke tests'),
    ('PM',  'K13', 2021, 'Political Management intake 2021'),
    ('PM',  'K14', 2022, 'Political Management intake 2022'),
    ('PM',  'K15', 2023, 'Political Management intake 2023'),
    ('MMT', 'K14', 2022, 'Multimedia Technology intake 2022'),
    ('MMT', 'K15', 2023, 'Multimedia Technology intake 2023'),
    ('MMT', 'K16', 2024, 'Multimedia Technology intake 2024')
) AS v(program_code, cohort_code, start_year, note)
  ON p.program_code = v.program_code
ON CONFLICT (program_id, cohort_code) DO UPDATE SET
  start_year = EXCLUDED.start_year,
  note = EXCLUDED.note;

-- ============================================================
-- 3) Additional course catalog entries for PM and MMT
-- ============================================================

INSERT INTO acad.courses AS c (course_code, course_name, credits, subject_prefix, course_level, is_language_prep)
VALUES
  ('HIST 240', 'Modern Vietnamese Political History', 4, 'HIST', 240, false),
  ('LAW 320', 'Constitutional and Administrative Law', 4, 'LAW', 320, false),
  ('POLS 101', 'Introduction to Political Science', 4, 'POLS', 101, false),
  ('POLS 240', 'Political Theory and Ideologies', 4, 'POLS', 240, false),
  ('POLS 410', 'Comparative Governance', 4, 'POLS', 410, false),
  ('RES 301', 'Research Methods for Public Policy', 4, 'RES', 301, false),
  ('GOV 220', 'Principles of Public Administration', 4, 'GOV', 220, false),
  ('GOV 330', 'Public Budgeting and Procurement', 4, 'GOV', 330, false),
  ('GOV 340', 'Public Policy Analysis', 4, 'GOV', 340, false),
  ('GOV 350', 'Digital Government and Civic Innovation', 4, 'GOV', 350, false),
  ('GOV 430', 'Ethics, Integrity and Accountability', 4, 'GOV', 430, false),
  ('GOV 450', 'Field Internship in Local Government', 6, 'GOV', 450, false),
  ('GOV 498', 'Political Management Capstone', 6, 'GOV', 498, false),
  ('DSGN 101', 'Design Fundamentals', 4, 'DSGN', 101, false),
  ('MMT 110', 'Visual Storytelling', 4, 'MMT', 110, false),
  ('MMT 120', 'Digital Imaging and Compositing', 4, 'MMT', 120, false),
  ('MMT 130', 'Typography and Editorial Layout', 4, 'MMT', 130, false),
  ('MMT 210', 'Motion Graphics', 4, 'MMT', 210, false),
  ('MMT 215', 'User Experience Foundations', 4, 'MMT', 215, false),
  ('MMT 220', 'Web Design Studio', 4, 'MMT', 220, false),
  ('MMT 230', 'Audio Video Production', 4, 'MMT', 230, false),
  ('MMT 240', '3D Modelling Basics', 4, 'MMT', 240, false),
  ('MMT 310', 'Interactive Media Design', 4, 'MMT', 310, false),
  ('MMT 320', 'User Interface Systems', 4, 'MMT', 320, false),
  ('MMT 325', 'Front-End Development for Creatives', 4, 'MMT', 325, false),
  ('MMT 330', 'Animation Production', 4, 'MMT', 330, false),
  ('MMT 335', 'Game Art Pipeline', 4, 'MMT', 335, false),
  ('MMT 340', 'Service Design Lab', 4, 'MMT', 340, false),
  ('MMT 345', 'UX Research Studio', 4, 'MMT', 345, false),
  ('MMT 350', 'Design Systems for Digital Products', 4, 'MMT', 350, false),
  ('MMT 360', 'Character Animation', 4, 'MMT', 360, false),
  ('MMT 365', 'Real-Time Rendering', 4, 'MMT', 365, false),
  ('MMT 370', 'Game Interface Design', 4, 'MMT', 370, false),
  ('MMT 410', 'Portfolio Development', 4, 'MMT', 410, false),
  ('MMT 420', 'Industry Internship', 6, 'MMT', 420, false),
  ('MMT 498', 'Graduation Project', 6, 'MMT', 498, false)
ON CONFLICT (course_code) DO UPDATE SET
  course_name = EXCLUDED.course_name,
  credits = COALESCE(EXCLUDED.credits, c.credits),
  subject_prefix = EXCLUDED.subject_prefix,
  course_level = EXCLUDED.course_level,
  is_language_prep = EXCLUDED.is_language_prep;

-- ============================================================
-- 4) Curriculum category master
-- ============================================================

INSERT INTO acad.curriculum_categories (program_id, category_name, min_credits, sort_order)
SELECT p.program_id, v.category_name, v.min_credits, v.sort_order
FROM acad.programs p
JOIN (
  VALUES
    ('PM',  'Civic Foundations', 24, 1),
    ('PM',  'Governance Core', 24, 2),
    ('PM',  'Strategy and Leadership', 12, 3),
    ('PM',  'Public Affairs Electives', 12, 4),
    ('PM',  'Internship and Capstone', 12, 5),
    ('MMT', 'Visual Communication Foundation', 16, 1),
    ('MMT', 'Interactive Media Core', 20, 2),
    ('MMT', 'Production and Technology', 20, 3),
    ('MMT', 'Concentration Studio', 12, 4),
    ('MMT', 'Portfolio and Industry Practice', 16, 5),
    ('MMT', 'Open Electives', 8, 6)
) AS v(program_code, category_name, min_credits, sort_order)
  ON p.program_code = v.program_code
ON CONFLICT (program_id, category_name) DO UPDATE SET
  min_credits = EXCLUDED.min_credits,
  sort_order = EXCLUDED.sort_order;

-- ============================================================
-- 5) PM curriculum requirements
-- ============================================================

INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, course_code, prereq_rule, is_required, note)
SELECT co.cohort_id, cat.category_id, 'course', v.course_code, v.prereq_rule::jsonb, true, v.note
FROM acad.cohorts co
JOIN acad.programs p
  ON p.program_id = co.program_id
 AND p.program_code = 'PM'
JOIN acad.curriculum_categories cat
  ON cat.program_id = p.program_id
 AND cat.category_name = 'Civic Foundations'
JOIN (
  VALUES
    ('POLS 101', NULL, 'Entry course into political management'),
    ('COMS 101', NULL, 'Oral communication foundation'),
    ('HIST 240', NULL, 'Historical context for governance studies'),
    ('SOC 144', NULL, 'Social systems and behavioral context'),
    ('STA 228', NULL, 'Quantitative literacy for policy work'),
    ('ECO 205', NULL, 'Macroeconomic literacy for public sector planning')
) AS v(course_code, prereq_rule, note)
  ON TRUE
WHERE co.cohort_code IN ('K13', 'K14', 'K15')
ON CONFLICT DO NOTHING;

INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, course_code, prereq_rule, is_required, note)
SELECT co.cohort_id, cat.category_id, 'course', v.course_code, v.prereq_rule::jsonb, true, v.note
FROM acad.cohorts co
JOIN acad.programs p
  ON p.program_id = co.program_id
 AND p.program_code = 'PM'
JOIN acad.curriculum_categories cat
  ON cat.program_id = p.program_id
 AND cat.category_name = 'Governance Core'
JOIN (
  VALUES
    ('GOV 220', '{"op":"COMPLETED","course":"POLS 101"}', 'Core introduction to public administration'),
    ('LAW 320', '{"op":"COMPLETED","course":"POLS 101"}', 'Public law and administrative law grounding'),
    ('POLS 240', '{"op":"COMPLETED","course":"POLS 101"}', 'Political theory sequence'),
    ('RES 301', '{"op":"AND","args":[{"op":"COMPLETED","course":"COMS 101"},{"op":"COMPLETED","course":"STA 228"}]}', 'Applied research methods'),
    ('GOV 330', '{"op":"AND","args":[{"op":"COMPLETED","course":"GOV 220"},{"op":"COMPLETED","course":"ECO 205"}]}', 'Budgeting requires governance and economics basics'),
    ('GOV 340', '{"op":"AND","args":[{"op":"COMPLETED","course":"GOV 220"},{"op":"COMPLETED","course":"RES 301"}]}', 'Policy analysis depends on admin and research training')
) AS v(course_code, prereq_rule, note)
  ON TRUE
WHERE co.cohort_code IN ('K13', 'K14', 'K15')
ON CONFLICT DO NOTHING;

INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, course_code, prereq_rule, is_required, note)
SELECT co.cohort_id, cat.category_id, 'course', v.course_code, v.prereq_rule::jsonb, true, v.note
FROM acad.cohorts co
JOIN acad.programs p
  ON p.program_id = co.program_id
 AND p.program_code = 'PM'
JOIN acad.curriculum_categories cat
  ON cat.program_id = p.program_id
 AND cat.category_name = 'Strategy and Leadership'
JOIN (
  VALUES
    ('GOV 350', '{"op":"COMPLETED","course":"GOV 340"}', 'Digital governance is taken after policy analysis'),
    ('POLS 410', '{"op":"COMPLETED","course":"POLS 240"}', 'Comparative governance after political theory'),
    ('GOV 430', '{"op":"AND","args":[{"op":"COMPLETED","course":"LAW 320"},{"op":"COMPLETED","course":"GOV 220"}]}', 'Ethics and accountability require law and administration context')
) AS v(course_code, prereq_rule, note)
  ON TRUE
WHERE co.cohort_code IN ('K13', 'K14', 'K15')
ON CONFLICT DO NOTHING;

INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, course_code, prereq_rule, is_required, note)
SELECT co.cohort_id, cat.category_id, 'course', v.course_code, v.prereq_rule::jsonb, true, v.note
FROM acad.cohorts co
JOIN acad.programs p
  ON p.program_id = co.program_id
 AND p.program_code = 'PM'
JOIN acad.curriculum_categories cat
  ON cat.program_id = p.program_id
 AND cat.category_name = 'Internship and Capstone'
JOIN (
  VALUES
    ('GOV 450', '{"op":"MIN_CREDITS","value":60}', 'Field internship opens after core progression'),
    ('GOV 498', '{"op":"AND","args":[{"op":"COMPLETED","course":"GOV 450"},{"op":"MIN_CREDITS","value":72}]}', 'Capstone is reserved for near-completion students')
) AS v(course_code, prereq_rule, note)
  ON TRUE
WHERE co.cohort_code IN ('K13', 'K14', 'K15')
ON CONFLICT DO NOTHING;

INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, min_credits, allowed_courses, prereq_rule, is_required, note)
SELECT co.cohort_id, cat.category_id, 'credit_bucket', 12,
       ARRAY['INTL 271','MGMT 321','PSYC 154','FIN 239','LAWS 122','BUS 309']::acad.course_code[],
       '{"op":"MIN_CREDITS","value":24}'::jsonb,
       false,
       'Choose 12 credits of public affairs electives after completing the foundation block'
FROM acad.cohorts co
JOIN acad.programs p
  ON p.program_id = co.program_id
 AND p.program_code = 'PM'
JOIN acad.curriculum_categories cat
  ON cat.program_id = p.program_id
 AND cat.category_name = 'Public Affairs Electives'
WHERE co.cohort_code IN ('K13', 'K14', 'K15')
  AND NOT EXISTS (
    SELECT 1
    FROM acad.curriculum_requirements r
    WHERE r.cohort_id = co.cohort_id
      AND r.category_id = cat.category_id
      AND r.kind = 'credit_bucket'
      AND r.min_credits = 12
  );

-- ============================================================
-- 6) MMT curriculum requirements
-- ============================================================

INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, course_code, prereq_rule, is_required, note)
SELECT co.cohort_id, cat.category_id, 'course', v.course_code, v.prereq_rule::jsonb, true, v.note
FROM acad.cohorts co
JOIN acad.programs p
  ON p.program_id = co.program_id
 AND p.program_code = 'MMT'
JOIN acad.curriculum_categories cat
  ON cat.program_id = p.program_id
 AND cat.category_name = 'Visual Communication Foundation'
JOIN (
  VALUES
    ('DSGN 101', NULL, 'Shared design language and composition principles'),
    ('MMT 110', NULL, 'Narrative thinking for digital media'),
    ('MMT 120', NULL, 'Hands-on image editing and compositing'),
    ('MMT 130', NULL, 'Typography and layout systems')
) AS v(course_code, prereq_rule, note)
  ON TRUE
WHERE co.cohort_code IN ('K14', 'K15', 'K16')
ON CONFLICT DO NOTHING;

INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, course_code, prereq_rule, is_required, note)
SELECT co.cohort_id, cat.category_id, 'course', v.course_code, v.prereq_rule::jsonb, true, v.note
FROM acad.cohorts co
JOIN acad.programs p
  ON p.program_id = co.program_id
 AND p.program_code = 'MMT'
JOIN acad.curriculum_categories cat
  ON cat.program_id = p.program_id
 AND cat.category_name = 'Interactive Media Core'
JOIN (
  VALUES
    ('MMT 215', '{"op":"COMPLETED","course":"DSGN 101"}', 'UX foundations build on core design thinking'),
    ('MMT 220', '{"op":"AND","args":[{"op":"COMPLETED","course":"MMT 120"},{"op":"COMPLETED","course":"MMT 130"}]}', 'Web design needs image and layout fluency'),
    ('MMT 210', '{"op":"AND","args":[{"op":"COMPLETED","course":"MMT 110"},{"op":"COMPLETED","course":"MMT 120"}]}', 'Motion work requires storyboarding and imaging basics'),
    ('MMT 230', '{"op":"COMPLETED","course":"MMT 110"}', 'Audio-video production after storytelling'),
    ('MMT 240', '{"op":"COMPLETED","course":"MMT 120"}', '3D modelling after digital imaging')
) AS v(course_code, prereq_rule, note)
  ON TRUE
WHERE co.cohort_code IN ('K14', 'K15', 'K16')
ON CONFLICT DO NOTHING;

INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, course_code, prereq_rule, is_required, note)
SELECT co.cohort_id, cat.category_id, 'course', v.course_code, v.prereq_rule::jsonb, true, v.note
FROM acad.cohorts co
JOIN acad.programs p
  ON p.program_id = co.program_id
 AND p.program_code = 'MMT'
JOIN acad.curriculum_categories cat
  ON cat.program_id = p.program_id
 AND cat.category_name = 'Production and Technology'
JOIN (
  VALUES
    ('MMT 310', '{"op":"AND","args":[{"op":"COMPLETED","course":"MMT 210"},{"op":"COMPLETED","course":"MMT 220"}]}', 'Interactive media design combines motion and web studio'),
    ('MMT 320', '{"op":"AND","args":[{"op":"COMPLETED","course":"MMT 215"},{"op":"COMPLETED","course":"MMT 220"}]}', 'UI systems require UX and interface prototyping'),
    ('MMT 325', '{"op":"COMPLETED","course":"MMT 220"}', 'Front-end delivery after web design studio'),
    ('MMT 330', '{"op":"AND","args":[{"op":"COMPLETED","course":"MMT 210"},{"op":"COMPLETED","course":"MMT 230"}]}', 'Animation production after motion and video production'),
    ('MMT 340', '{"op":"AND","args":[{"op":"COMPLETED","course":"MMT 215"},{"op":"COMPLETED","course":"MMT 320"}]}', 'Service design lab after UX and UI systems')
) AS v(course_code, prereq_rule, note)
  ON TRUE
WHERE co.cohort_code IN ('K14', 'K15', 'K16')
ON CONFLICT DO NOTHING;

INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, course_code, prereq_rule, is_required, note)
SELECT co.cohort_id, cat.category_id, 'course', v.course_code, v.prereq_rule::jsonb, true, v.note
FROM acad.cohorts co
JOIN acad.programs p
  ON p.program_id = co.program_id
 AND p.program_code = 'MMT'
JOIN acad.curriculum_categories cat
  ON cat.program_id = p.program_id
 AND cat.category_name = 'Portfolio and Industry Practice'
JOIN (
  VALUES
    ('MMT 410', '{"op":"AND","args":[{"op":"COMPLETED","course":"MMT 310"},{"op":"COMPLETED","course":"MMT 320"}]}', 'Portfolio is built after the main production stack'),
    ('MMT 420', '{"op":"MIN_CREDITS","value":60}', 'Internship requires studio maturity'),
    ('MMT 498', '{"op":"AND","args":[{"op":"COMPLETED","course":"MMT 410"},{"op":"COMPLETED","course":"MMT 420"},{"op":"MIN_CREDITS","value":80}]}', 'Graduation project after portfolio and internship')
) AS v(course_code, prereq_rule, note)
  ON TRUE
WHERE co.cohort_code IN ('K14', 'K15', 'K16')
ON CONFLICT DO NOTHING;

INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, min_credits, allowed_courses, prereq_rule, is_required, note)
SELECT co.cohort_id, cat.category_id, 'credit_bucket', 12,
       ARRAY['MMT 335','MMT 345','MMT 350','MMT 360','MMT 365','MMT 370']::acad.course_code[],
       '{"op":"MIN_CREDITS","value":40}'::jsonb,
       false,
       'Choose an advanced studio concentration after clearing the core production sequence'
FROM acad.cohorts co
JOIN acad.programs p
  ON p.program_id = co.program_id
 AND p.program_code = 'MMT'
JOIN acad.curriculum_categories cat
  ON cat.program_id = p.program_id
 AND cat.category_name = 'Concentration Studio'
WHERE co.cohort_code IN ('K14', 'K15', 'K16')
  AND NOT EXISTS (
    SELECT 1
    FROM acad.curriculum_requirements r
    WHERE r.cohort_id = co.cohort_id
      AND r.category_id = cat.category_id
      AND r.kind = 'credit_bucket'
      AND r.min_credits = 12
  );

INSERT INTO acad.curriculum_requirements
  (cohort_id, category_id, kind, min_credits, allowed_courses, prereq_rule, is_required, note)
SELECT co.cohort_id, cat.category_id, 'credit_bucket', 8,
       ARRAY['MKTG 420','BUS 345','MIS 446','IEPS 385']::acad.course_code[],
       NULL,
       false,
       'Open electives used to benchmark broader catalog queries without inflating student counts'
FROM acad.cohorts co
JOIN acad.programs p
  ON p.program_id = co.program_id
 AND p.program_code = 'MMT'
JOIN acad.curriculum_categories cat
  ON cat.program_id = p.program_id
 AND cat.category_name = 'Open Electives'
WHERE co.cohort_code IN ('K14', 'K15', 'K16')
  AND NOT EXISTS (
    SELECT 1
    FROM acad.curriculum_requirements r
    WHERE r.cohort_id = co.cohort_id
      AND r.category_id = cat.category_id
      AND r.kind = 'credit_bucket'
      AND r.min_credits = 8
  );

-- ============================================================
-- 7) Concentration master and concentration course maps
-- ============================================================

INSERT INTO acad.concentrations AS conc (program_id, concentration_code, concentration_name, min_credits, meta)
SELECT p.program_id, v.concentration_code, v.concentration_name, v.min_credits, v.meta::jsonb
FROM acad.programs p
JOIN (
  VALUES
    ('BBS', 'BAN', 'Business Analytics', 20, '{"school":"School of Business"}'),
    ('MMT', 'UX',  'User Experience Design', 12, '{"school":"School of Design and Digital Media"}'),
    ('MMT', 'ANI', 'Animation and Motion Design', 12, '{"school":"School of Design and Digital Media"}'),
    ('MMT', 'GDM', 'Game and Digital Media', 12, '{"school":"School of Design and Digital Media"}')
) AS v(program_code, concentration_code, concentration_name, min_credits, meta)
  ON p.program_code = v.program_code
ON CONFLICT (program_id, concentration_code) DO UPDATE SET
  concentration_name = EXCLUDED.concentration_name,
  min_credits = EXCLUDED.min_credits,
  meta = conc.meta || EXCLUDED.meta;

INSERT INTO acad.concentration_courses AS cc
  (concentration_id, course_code, is_required, is_entry_course, sort_order, meta)
SELECT c.concentration_id, v.course_code, true, v.is_entry_course, v.sort_order, v.meta::jsonb
FROM acad.programs p
JOIN acad.concentrations c
  ON c.program_id = p.program_id
JOIN (
  VALUES
    ('BBS', 'MKT', 'MKTG 336', true, 1, '{"track":"marketing"}'),
    ('BBS', 'MKT', 'MKTG 363', false, 2, '{"track":"marketing"}'),
    ('BBS', 'MKT', 'MKTG 460', false, 3, '{"track":"marketing"}'),
    ('BBS', 'MKT', 'MKTG 461', false, 4, '{"track":"marketing"}'),
    ('BBS', 'MKT', 'MKTG 462', false, 5, '{"track":"marketing"}'),
    ('BBS', 'MKT', 'MKTG 466', false, 6, '{"track":"marketing"}'),
    ('BBS', 'MKT', 'MKTG 467', false, 7, '{"track":"marketing"}'),
    ('BBS', 'SCM', 'SCLM 429', true, 1, '{"track":"supply_chain"}'),
    ('BBS', 'SCM', 'SCLM 439', false, 2, '{"track":"supply_chain"}'),
    ('BBS', 'SCM', 'SCLM 449', false, 3, '{"track":"supply_chain"}'),
    ('BBS', 'SCM', 'SCLM 459', false, 4, '{"track":"supply_chain"}'),
    ('BBS', 'SCM', 'SCLM 479', false, 5, '{"track":"supply_chain"}'),
    ('BBS', 'HRM', 'HRM 351', true, 1, '{"track":"human_resources"}'),
    ('BBS', 'HRM', 'HRM 461', false, 2, '{"track":"human_resources"}'),
    ('BBS', 'HRM', 'HRM 471', false, 3, '{"track":"human_resources"}'),
    ('BBS', 'HRM', 'HRM 491', false, 4, '{"track":"human_resources"}'),
    ('BBS', 'HRM', 'HRM 492', false, 5, '{"track":"human_resources"}'),
    ('BBS', 'HRM', 'HRM 493', false, 6, '{"track":"human_resources"}'),
    ('BBS', 'HOS', 'HMGT 213', true, 1, '{"track":"hospitality"}'),
    ('BBS', 'HOS', 'HMGT 358', false, 2, '{"track":"hospitality"}'),
    ('BBS', 'HOS', 'HMGT 361', false, 3, '{"track":"hospitality"}'),
    ('BBS', 'HOS', 'HMGT 401', false, 4, '{"track":"hospitality"}'),
    ('BBS', 'HOS', 'HMGT 410', false, 5, '{"track":"hospitality"}'),
    ('BBS', 'HOS', 'HMGT 434', false, 6, '{"track":"hospitality"}'),
    ('BBS', 'HOS', 'HMGT 482', false, 7, '{"track":"hospitality"}'),
    ('BBS', 'FIN', 'FIN 319', true, 1, '{"track":"finance"}'),
    ('BBS', 'FIN', 'FIN 352', false, 2, '{"track":"finance"}'),
    ('BBS', 'FIN', 'ACTG 381', false, 3, '{"track":"finance"}'),
    ('BBS', 'FIN', 'FIN 443', false, 4, '{"track":"finance"}'),
    ('BBS', 'FIN', 'FIN 449', false, 5, '{"track":"finance"}'),
    ('BBS', 'FIN', 'FIN 456', false, 6, '{"track":"finance"}'),
    ('BBS', 'FIN', 'FIN 465', false, 7, '{"track":"finance"}'),
    ('BBS', 'ACC', 'ACTG 335', true, 1, '{"track":"accounting"}'),
    ('BBS', 'ACC', 'ACTG 360', false, 2, '{"track":"accounting"}'),
    ('BBS', 'ACC', 'ACTG 381', false, 3, '{"track":"accounting"}'),
    ('BBS', 'ACC', 'ACTG 382', false, 4, '{"track":"accounting"}'),
    ('BBS', 'ACC', 'ACTG 383', false, 5, '{"track":"accounting"}'),
    ('BBS', 'ACC', 'ACTG 498', false, 6, '{"track":"accounting"}'),
    ('BBS', 'ENT', 'IEPS 360', true, 1, '{"track":"entrepreneurship"}'),
    ('BBS', 'ENT', 'IEPS 361', false, 2, '{"track":"entrepreneurship"}'),
    ('BBS', 'ENT', 'IEPS 371', false, 3, '{"track":"entrepreneurship"}'),
    ('BBS', 'ENT', 'IEPS 385', false, 4, '{"track":"entrepreneurship"}'),
    ('BBS', 'ENT', 'IEPS 450', false, 5, '{"track":"entrepreneurship"}'),
    ('BBS', 'ENT', 'IEPS 488', false, 6, '{"track":"entrepreneurship"}'),
    ('BBS', 'BAN', 'MIS 301', true, 1, '{"track":"business_analytics"}'),
    ('BBS', 'BAN', 'MIS 311', true, 2, '{"track":"business_analytics"}'),
    ('BBS', 'BAN', 'MIS 315', false, 3, '{"track":"business_analytics"}'),
    ('BBS', 'BAN', 'MIS 443', false, 4, '{"track":"business_analytics"}'),
    ('BBS', 'BAN', 'MIS 446', false, 5, '{"track":"business_analytics"}'),
    ('BBS', 'BAN', 'MIS 451', false, 6, '{"track":"business_analytics"}'),
    ('BBS', 'BAN', 'MIS 495', false, 7, '{"track":"business_analytics"}'),
    ('MMT', 'UX',  'MMT 345', true, 1, '{"track":"ux"}'),
    ('MMT', 'UX',  'MMT 350', false, 2, '{"track":"ux"}'),
    ('MMT', 'UX',  'MMT 340', false, 3, '{"track":"ux"}'),
    ('MMT', 'ANI', 'MMT 335', true, 1, '{"track":"animation"}'),
    ('MMT', 'ANI', 'MMT 360', false, 2, '{"track":"animation"}'),
    ('MMT', 'ANI', 'MMT 330', false, 3, '{"track":"animation"}'),
    ('MMT', 'GDM', 'MMT 365', true, 1, '{"track":"game_media"}'),
    ('MMT', 'GDM', 'MMT 370', false, 2, '{"track":"game_media"}'),
    ('MMT', 'GDM', 'MMT 325', false, 3, '{"track":"game_media"}')
) AS v(program_code, concentration_code, course_code, is_entry_course, sort_order, meta)
  ON p.program_code = v.program_code
 AND c.concentration_code = v.concentration_code
ON CONFLICT (concentration_id, course_code) DO UPDATE SET
  is_required = EXCLUDED.is_required,
  is_entry_course = EXCLUDED.is_entry_course,
  sort_order = EXCLUDED.sort_order,
  meta = cc.meta || EXCLUDED.meta;

-- ============================================================
-- 8) Course offerings and advisories for benchmark terms
-- ============================================================

INSERT INTO acad.course_offerings AS off (term_code, course_code, is_open, registration_channel, meta)
VALUES
  (20262, 'MKTG 336', true, 'portal', '{"school":"School of Business"}'::jsonb),
  (20262, 'MKTG 363', true, 'portal', '{"school":"School of Business"}'::jsonb),
  (20262, 'MKTG 460', true, 'portal', '{"school":"School of Business"}'::jsonb),
  (20262, 'MIS 311',  true, 'portal', '{"school":"School of Business"}'::jsonb),
  (20262, 'MIS 315',  true, 'portal', '{"school":"School of Business"}'::jsonb),
  (20262, 'MIS 446',  true, 'portal', '{"school":"School of Business"}'::jsonb),
  (20262, 'FIN 319',  true, 'portal', '{"school":"School of Business"}'::jsonb),
  (20262, 'FIN 352',  true, 'portal', '{"school":"School of Business"}'::jsonb),
  (20262, 'ACTG 381', true, 'portal', '{"school":"School of Business"}'::jsonb),
  (20262, 'BUS 381',  true, 'faculty_approval', '{"school":"School of Business"}'::jsonb),
  (20262, 'GOV 340',  true, 'portal', '{"school":"School of Governance and Public Affairs"}'::jsonb),
  (20262, 'GOV 350',  true, 'portal', '{"school":"School of Governance and Public Affairs"}'::jsonb),
  (20262, 'GOV 430',  true, 'portal', '{"school":"School of Governance and Public Affairs"}'::jsonb),
  (20262, 'GOV 450',  true, 'field_placement', '{"school":"School of Governance and Public Affairs"}'::jsonb),
  (20262, 'RES 301',  true, 'portal', '{"school":"School of Governance and Public Affairs"}'::jsonb),
  (20262, 'MMT 345',  true, 'portfolio_review', '{"school":"School of Design and Digital Media"}'::jsonb),
  (20262, 'MMT 350',  true, 'portal', '{"school":"School of Design and Digital Media"}'::jsonb),
  (20262, 'MMT 340',  true, 'studio_enrolment', '{"school":"School of Design and Digital Media"}'::jsonb),
  (20262, 'MMT 335',  true, 'studio_enrolment', '{"school":"School of Design and Digital Media"}'::jsonb),
  (20262, 'MMT 360',  true, 'studio_enrolment', '{"school":"School of Design and Digital Media"}'::jsonb),
  (20262, 'MMT 365',  true, 'studio_enrolment', '{"school":"School of Design and Digital Media"}'::jsonb),
  (20262, 'MMT 370',  true, 'studio_enrolment', '{"school":"School of Design and Digital Media"}'::jsonb),
  (20262, 'MMT 215',  true, 'portal', '{"school":"School of Design and Digital Media"}'::jsonb),
  (20262, 'MMT 220',  true, 'portal', '{"school":"School of Design and Digital Media"}'::jsonb),
  (20262, 'MMT 230',  true, 'portal', '{"school":"School of Design and Digital Media"}'::jsonb),
  (20271, 'BUS 450',  true, 'manual_email', '{"school":"School of Business"}'::jsonb),
  (20271, 'BUS 496',  true, 'portal', '{"school":"School of Business"}'::jsonb),
  (20271, 'GOV 498',  true, 'committee_approval', '{"school":"School of Governance and Public Affairs"}'::jsonb),
  (20271, 'MMT 420',  true, 'portfolio_review', '{"school":"School of Design and Digital Media"}'::jsonb),
  (20271, 'MMT 410',  true, 'portal', '{"school":"School of Design and Digital Media"}'::jsonb),
  (20271, 'MMT 498',  true, 'committee_approval', '{"school":"School of Design and Digital Media"}'::jsonb)
ON CONFLICT (term_code, course_code) DO UPDATE SET
  is_open = EXCLUDED.is_open,
  registration_channel = EXCLUDED.registration_channel,
  meta = off.meta || EXCLUDED.meta;

INSERT INTO acad.course_advisories (course_code, advisory_type, rule_json, note)
SELECT v.course_code, v.advisory_type, v.rule_json::jsonb, v.note
FROM (
  VALUES
    ('GOV 450', 'field_placement', '{"registrationChannel":"field_placement","requiresAdvisorApproval":true}', 'Government internship requires advisor approval and host confirmation'),
    ('GOV 498', 'oral_defense', '{"committee":"public_policy_panel"}', 'Capstone includes an oral defense scheduled by the school office'),
    ('MMT 420', 'portfolio_review', '{"registrationChannel":"portfolio_review","minimumPieces":6}', 'Internship registration opens after portfolio review'),
    ('MMT 498', 'capstone_load_limit', '{"max_parallel_courses":2}', 'Graduation project should be paired with no more than two extra courses')
) AS v(course_code, advisory_type, rule_json, note)
WHERE NOT EXISTS (
  SELECT 1
  FROM acad.course_advisories a
  WHERE a.course_code = v.course_code
    AND a.advisory_type = v.advisory_type
);

-- ============================================================
-- 9) Equivalency sets used by audit benchmarks
-- ============================================================

INSERT INTO acad.equivalency_sets (program_id, title, note)
SELECT p.program_id, v.title, v.note
FROM acad.programs p
JOIN (
  VALUES
    ('BBS', 'BBS Transfer Equivalencies', 'Common transfer articulations for writing and law requirements'),
    ('PM',  'PM Transfer Equivalencies',  'Accepted transfer mappings for public governance students')
) AS v(program_code, title, note)
  ON p.program_code = v.program_code
WHERE NOT EXISTS (
  SELECT 1
  FROM acad.equivalency_sets es
  WHERE es.program_id = p.program_id
    AND es.title = v.title
);

INSERT INTO acad.equivalencies
  (equiv_set_id, course_code, equivalent_course_code, cohort_id, note)
SELECT es.equiv_set_id, v.course_code, v.equivalent_course_code, co.cohort_id, v.note
FROM acad.equivalency_sets es
JOIN acad.programs p
  ON p.program_id = es.program_id
JOIN acad.cohorts co
  ON co.program_id = p.program_id
JOIN (
  VALUES
    ('BBS Transfer Equivalencies', 'K13', 'WRT 122', 'WRT 187', 'Academic writing transfer accepted from legacy writing catalog'),
    ('BBS Transfer Equivalencies', 'K13', 'LAWS 122', 'LAWS 121', 'Legacy 3-credit laws module accepted for K13 benchmark cases'),
    ('PM Transfer Equivalencies',  'K13', 'LAW 320', 'LAWS 122', 'Transfer students may satisfy public law with Vietnamese laws foundation'),
    ('PM Transfer Equivalencies',  'K14', 'LAW 320', 'LAWS 122', 'Transfer students may satisfy public law with Vietnamese laws foundation'),
    ('PM Transfer Equivalencies',  'K15', 'LAW 320', 'LAWS 122', 'Transfer students may satisfy public law with Vietnamese laws foundation')
) AS v(title, cohort_code, course_code, equivalent_course_code, note)
  ON es.title = v.title
 AND co.cohort_code = v.cohort_code
WHERE NOT EXISTS (
  SELECT 1
  FROM acad.equivalencies e
  WHERE e.equiv_set_id = es.equiv_set_id
    AND e.course_code = v.course_code
    AND e.equivalent_course_code = v.equivalent_course_code
    AND e.cohort_id = co.cohort_id
);

-- ============================================================
-- 10) Curated benchmark students
-- Small quantity, high diversity of academic progress
-- ============================================================

INSERT INTO acad.students
  (student_id, last_name, first_name, program_id, cohort_id, status, english_level, ielts_score, meta)
SELECT v.student_id, v.last_name, v.first_name, p.program_id, co.cohort_id, v.status, v.english_level, v.ielts_score, v.meta::jsonb
FROM acad.programs p
JOIN acad.cohorts co
  ON co.program_id = p.program_id
JOIN (
  VALUES
    ('20230011', 'Nguyen', 'Minh Khang', 'BBS', 'K13', 'active', 6, 6.5, '{"benchmarkGroup":"advanced","advisor":"Dr. Le Thanh","homeCity":"Da Nang"}'),
    ('20230012', 'Tran',   'Bao Chau',   'BBS', 'K13', 'active', 6, 6.0, '{"benchmarkGroup":"mid","advisor":"Dr. Le Thanh","homeCity":"Hue"}'),
    ('20230013', 'Pham',   'Gia Huy',    'BBS', 'K13', 'active', 5, 5.5, '{"benchmarkGroup":"policy_blocked","advisor":"Ms. Nguyen Phuong","homeCity":"Quang Nam"}'),
    ('20210021', 'Do',     'Minh Quan',  'PM',  'K13', 'active', 6, 6.5, '{"benchmarkGroup":"advanced","advisor":"Mr. Vo Hai","homeCity":"Quang Tri"}'),
    ('20220022', 'Hoang',  'Ngoc Anh',   'PM',  'K14', 'active', 5, 6.0, '{"benchmarkGroup":"mid","advisor":"Mr. Vo Hai","homeCity":"Da Nang"}'),
    ('20230023', 'Vu',     'Thanh Lam',  'PM',  'K15', 'active', 4, 5.0, '{"benchmarkGroup":"early","advisor":"Mr. Vo Hai","homeCity":"Quang Ngai"}'),
    ('20230071', 'Le',     'Gia Han',    'MMT', 'K15', 'active', 6, 6.5, '{"benchmarkGroup":"advanced","advisor":"Ms. Truong Linh","homeCity":"Ho Chi Minh City"}'),
    ('20230072', 'Bui',    'Tuan Anh',   'MMT', 'K15', 'active', 6, 6.0, '{"benchmarkGroup":"mid","advisor":"Ms. Truong Linh","homeCity":"Hue"}'),
    ('20240073', 'Nguyen', 'Khanh Linh', 'MMT', 'K16', 'active', 5, 5.5, '{"benchmarkGroup":"early","advisor":"Ms. Truong Linh","homeCity":"Da Nang"}')
) AS v(student_id, last_name, first_name, program_code, cohort_code, status, english_level, ielts_score, meta)
  ON p.program_code = v.program_code
 AND co.cohort_code = v.cohort_code
ON CONFLICT (student_id) DO UPDATE SET
  last_name = EXCLUDED.last_name,
  first_name = EXCLUDED.first_name,
  program_id = EXCLUDED.program_id,
  cohort_id = EXCLUDED.cohort_id,
  status = EXCLUDED.status,
  english_level = EXCLUDED.english_level,
  ielts_score = EXCLUDED.ielts_score,
  meta = EXCLUDED.meta;

INSERT INTO acad.student_concentrations (student_id, concentration_id, approved_term_code, status)
SELECT v.student_id, c.concentration_id, v.approved_term_code, 'active'
FROM acad.programs p
JOIN acad.concentrations c
  ON c.program_id = p.program_id
JOIN (
  VALUES
    ('20230011', 'BBS', 'MKT', 20251),
    ('20230012', 'BBS', 'BAN', 20251),
    ('20230013', 'BBS', 'FIN', 20251),
    ('20230071', 'MMT', 'UX',  20252),
    ('20230072', 'MMT', 'ANI', 20252)
) AS v(student_id, program_code, concentration_code, approved_term_code)
  ON p.program_code = v.program_code
 AND c.concentration_code = v.concentration_code
ON CONFLICT (student_id, concentration_id) DO NOTHING;

-- ============================================================
-- 11) Course attempts
-- Each program gets:
-- - one advanced student
-- - one mid-progress student
-- - one early or blocked student
-- ============================================================

INSERT INTO acad.course_attempts
  (student_id, course_code, term_code, term_seq, attempt_no, credits, grade_letter, is_completed,
   snapshot_cum_credits, snapshot_target_credits, snapshot_cum_gpa, source_file, source_rowkey, raw_record)
VALUES
  ('20230011', 'BUS 101', 20231, 1, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-01', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'WRT 122', 20231, 1, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-02', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'ECO 110', 20231, 1, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-03', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'MTH 171', 20231, 1, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-04', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'BUS 198', 20232, 2, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-05', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'BUS 214', 20232, 2, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-06', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'ECO 204', 20232, 2, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-07', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'ACTG 240',20232, 2, 1, 4, 'B-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-08', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'ACTG 243',20241, 3, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-09', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'BUS 303', 20241, 3, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-10', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'BUS 314', 20241, 3, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-11', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'BUS 332', 20241, 3, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-12', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'BUS 328', 20242, 4, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-advanced-13', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230011', 'BUS 359', 20242, 4, 1, 4, 'A-', true, 56, 195, 3.42, '60_seed_fake_data.sql', 'bbs-advanced-14', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),

  ('20230012', 'BUS 101', 20231, 1, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-mid-01', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230012', 'WRT 122', 20231, 1, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-mid-02', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230012', 'ECO 110', 20231, 1, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-mid-03', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230012', 'MTH 171', 20231, 1, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-mid-04', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230012', 'BUS 198', 20232, 2, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-mid-05', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230012', 'BUS 214', 20232, 2, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-mid-06', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230012', 'ECO 204', 20232, 2, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-mid-07', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230012', 'ACTG 240',20232, 2, 1, 4, 'C+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-mid-08', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230012', 'BUS 303', 20241, 3, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-mid-09', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230012', 'BUS 314', 20241, 3, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-mid-10', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230012', 'BUS 332', 20241, 3, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-mid-11', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230012', 'STA 228', 20241, 3, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-mid-12', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230012', 'MIS 301', 20242, 4, 1, 4, 'A-', true, 52, 195, 3.36, '60_seed_fake_data.sql', 'bbs-mid-13', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),

  ('20230013', 'BUS 101', 20231, 1, 1, 4, 'B-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-blocked-01', '{"seed":"60_seed_fake_data","progress":"blocked"}'::jsonb),
  ('20230013', 'WRT 122', 20231, 1, 1, 4, 'C+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-blocked-02', '{"seed":"60_seed_fake_data","progress":"blocked"}'::jsonb),
  ('20230013', 'ECO 110', 20231, 1, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-blocked-03', '{"seed":"60_seed_fake_data","progress":"blocked"}'::jsonb),
  ('20230013', 'MTH 171', 20231, 1, 1, 4, 'B-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-blocked-04', '{"seed":"60_seed_fake_data","progress":"blocked"}'::jsonb),
  ('20230013', 'BUS 198', 20232, 2, 1, 4, 'C+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-blocked-05', '{"seed":"60_seed_fake_data","progress":"blocked"}'::jsonb),
  ('20230013', 'BUS 214', 20232, 2, 1, 4, 'B-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-blocked-06', '{"seed":"60_seed_fake_data","progress":"blocked"}'::jsonb),
  ('20230013', 'ECO 204', 20232, 2, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-blocked-07', '{"seed":"60_seed_fake_data","progress":"blocked"}'::jsonb),
  ('20230013', 'ACTG 240',20232, 2, 1, 4, 'C',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-blocked-08', '{"seed":"60_seed_fake_data","progress":"blocked"}'::jsonb),
  ('20230013', 'ACTG 243',20241, 3, 1, 4, 'C+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-blocked-09', '{"seed":"60_seed_fake_data","progress":"blocked"}'::jsonb),
  ('20230013', 'BUS 303', 20241, 3, 1, 4, 'B-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-blocked-10', '{"seed":"60_seed_fake_data","progress":"blocked"}'::jsonb),
  ('20230013', 'BUS 314', 20241, 3, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'bbs-blocked-11', '{"seed":"60_seed_fake_data","progress":"blocked"}'::jsonb),
  ('20230013', 'BUS 328', 20241, 3, 1, 4, 'B-', true, 48, 195, 2.91, '60_seed_fake_data.sql', 'bbs-blocked-12', '{"seed":"60_seed_fake_data","progress":"blocked"}'::jsonb),

  ('20210021', 'POLS 101', 20211, 1, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-01', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'COMS 101', 20211, 1, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-02', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'HIST 240', 20211, 1, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-03', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'SOC 144',  20211, 1, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-04', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'STA 228',  20212, 2, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-05', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'ECO 205',  20212, 2, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-06', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'GOV 220',  20212, 2, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-07', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'LAW 320',  20212, 2, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-08', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'POLS 240', 20221, 3, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-09', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'RES 301',  20221, 3, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-10', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'GOV 330',  20221, 3, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-11', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'GOV 340',  20221, 3, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-12', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'GOV 350',  20222, 4, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-advanced-13', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20210021', 'POLS 410', 20222, 4, 1, 4, 'B+', true, 56, 84, 3.48, '60_seed_fake_data.sql', 'pm-advanced-14', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),

  ('20220022', 'POLS 101', 20221, 1, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-mid-01', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20220022', 'COMS 101', 20221, 1, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-mid-02', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20220022', 'HIST 240', 20221, 1, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-mid-03', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20220022', 'SOC 144',  20221, 1, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-mid-04', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20220022', 'STA 228',  20222, 2, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-mid-05', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20220022', 'ECO 205',  20222, 2, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-mid-06', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20220022', 'GOV 220',  20222, 2, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-mid-07', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20220022', 'LAW 320',  20222, 2, 1, 4, 'B-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-mid-08', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20220022', 'POLS 240', 20231, 3, 1, 4, 'B+', true, 36, 84, 3.18, '60_seed_fake_data.sql', 'pm-mid-09', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),

  ('20230023', 'POLS 101', 20231, 1, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-early-01', '{"seed":"60_seed_fake_data","progress":"early"}'::jsonb),
  ('20230023', 'COMS 101', 20231, 1, 1, 4, 'B-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-early-02', '{"seed":"60_seed_fake_data","progress":"early"}'::jsonb),
  ('20230023', 'HIST 240', 20231, 1, 1, 4, 'C+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'pm-early-03', '{"seed":"60_seed_fake_data","progress":"early"}'::jsonb),
  ('20230023', 'SOC 144',  20232, 2, 1, 4, 'B',  true, 16, 84, 2.84, '60_seed_fake_data.sql', 'pm-early-04', '{"seed":"60_seed_fake_data","progress":"early"}'::jsonb),

  ('20230071', 'DSGN 101', 20231, 1, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-advanced-01', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230071', 'MMT 110',  20231, 1, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-advanced-02', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230071', 'MMT 120',  20231, 1, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-advanced-03', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230071', 'MMT 130',  20231, 1, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-advanced-04', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230071', 'MMT 215',  20232, 2, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-advanced-05', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230071', 'MMT 220',  20232, 2, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-advanced-06', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230071', 'MMT 210',  20232, 2, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-advanced-07', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230071', 'MMT 230',  20232, 2, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-advanced-08', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230071', 'MMT 240',  20241, 3, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-advanced-09', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230071', 'MMT 310',  20241, 3, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-advanced-10', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230071', 'MMT 320',  20241, 3, 1, 4, 'A-', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-advanced-11', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),
  ('20230071', 'MMT 325',  20241, 3, 1, 4, 'B+', true, 48, 92, 3.51, '60_seed_fake_data.sql', 'mmt-advanced-12', '{"seed":"60_seed_fake_data","progress":"advanced"}'::jsonb),

  ('20230072', 'DSGN 101', 20231, 1, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-mid-01', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230072', 'MMT 110',  20231, 1, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-mid-02', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230072', 'MMT 120',  20231, 1, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-mid-03', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230072', 'MMT 130',  20231, 1, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-mid-04', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230072', 'MMT 210',  20232, 2, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-mid-05', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230072', 'MMT 220',  20232, 2, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-mid-06', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230072', 'MMT 230',  20232, 2, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-mid-07', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),
  ('20230072', 'MMT 240',  20232, 2, 1, 4, 'B',  true, 32, 92, 3.12, '60_seed_fake_data.sql', 'mmt-mid-08', '{"seed":"60_seed_fake_data","progress":"mid"}'::jsonb),

  ('20240073', 'DSGN 101', 20241, 1, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-early-01', '{"seed":"60_seed_fake_data","progress":"early"}'::jsonb),
  ('20240073', 'MMT 110',  20241, 1, 1, 4, 'B',  true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-early-02', '{"seed":"60_seed_fake_data","progress":"early"}'::jsonb),
  ('20240073', 'MMT 120',  20242, 2, 1, 4, 'B+', true, NULL, NULL, NULL, '60_seed_fake_data.sql', 'mmt-early-03', '{"seed":"60_seed_fake_data","progress":"early"}'::jsonb),
  ('20240073', 'MMT 130',  20242, 2, 1, 4, 'B',  true, 16, 92, 3.04, '60_seed_fake_data.sql', 'mmt-early-04', '{"seed":"60_seed_fake_data","progress":"early"}'::jsonb)
ON CONFLICT (student_id, course_code, term_code, attempt_no) DO UPDATE SET
  term_seq = EXCLUDED.term_seq,
  credits = EXCLUDED.credits,
  grade_letter = EXCLUDED.grade_letter,
  is_completed = EXCLUDED.is_completed,
  snapshot_cum_credits = EXCLUDED.snapshot_cum_credits,
  snapshot_target_credits = EXCLUDED.snapshot_target_credits,
  snapshot_cum_gpa = EXCLUDED.snapshot_cum_gpa,
  source_file = EXCLUDED.source_file,
  source_rowkey = EXCLUDED.source_rowkey,
  raw_record = EXCLUDED.raw_record;

-- ============================================================
-- 12) Forward-looking plans used by validation and recommendation endpoints
-- ============================================================

INSERT INTO acad.student_plans (student_id, term_code, course_code, status, note)
VALUES
  ('20230011', 20262, 'MKTG 336', 'planned', 'Concentration entry course'),
  ('20230011', 20262, 'MKTG 363', 'planned', 'Consumer behavior sequence'),
  ('20230011', 20262, 'MKTG 460', 'planned', 'Marketing research before internship'),
  ('20230012', 20262, 'MIS 311',  'planned', 'Analytics pathway core'),
  ('20230012', 20262, 'MIS 315',  'planned', 'Statistics-backed analytics methods'),
  ('20230012', 20262, 'MIS 446',  'planned', 'Visualization elective for analytics track'),
  ('20230013', 20262, 'FIN 319',  'planned', 'Finance concentration entry'),
  ('20230013', 20262, 'FIN 352',  'planned', 'Investment sequence'),
  ('20230013', 20262, 'ACTG 381', 'planned', 'Accounting support for finance track'),
  ('20210021', 20262, 'GOV 430',  'planned', 'Ethics block'),
  ('20210021', 20262, 'GOV 450',  'planned', 'Field internship placement'),
  ('20220022', 20262, 'RES 301',  'planned', 'Research methods bridge course'),
  ('20220022', 20262, 'GOV 330',  'planned', 'Budgeting sequence'),
  ('20220022', 20262, 'PSYC 154', 'planned', 'Elective to broaden public affairs perspective'),
  ('20230023', 20262, 'STA 228',  'planned', 'Quantitative literacy catch-up'),
  ('20230023', 20262, 'ECO 205',  'planned', 'Macroeconomics foundation'),
  ('20230023', 20262, 'GOV 220',  'planned', 'Public administration entry'),
  ('20230071', 20262, 'MMT 345',  'planned', 'UX concentration entry'),
  ('20230071', 20262, 'MMT 350',  'planned', 'Design system studio'),
  ('20230071', 20262, 'MMT 340',  'planned', 'Service design studio'),
  ('20230072', 20262, 'MMT 335',  'planned', 'Animation concentration entry'),
  ('20230072', 20262, 'MMT 360',  'planned', 'Character animation sequence'),
  ('20230072', 20262, 'MMT 330',  'planned', 'Animation production before advanced studio'),
  ('20240073', 20262, 'MMT 215',  'planned', 'Foundation UX step'),
  ('20240073', 20262, 'MMT 220',  'planned', 'Web design studio'),
  ('20240073', 20262, 'MMT 230',  'planned', 'Audio-video production')
ON CONFLICT (student_id, term_code, course_code) DO UPDATE SET
  status = EXCLUDED.status,
  note = EXCLUDED.note;

COMMIT;
