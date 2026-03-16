BEGIN;

-- ============================================================
-- BBS / QTKD seed data for acad schema
-- Source basis: uploaded BBS guidance document + placeholder
-- catalog items where the guide does not enumerate the full official curriculum.
-- This seed is intended for development/demo of audit + roadmap APIs.
-- ============================================================

-- ============================================================
-- 1) Program / Cohort
-- ============================================================

INSERT INTO acad.programs (program_code, program_name, degree_level, default_target_credits, meta)
VALUES
('BBS', 'Bachelor of Business Studies', 'Bachelor', 195, '{"faculty": "Business Administration", "source": "Uploaded BBS guidance document", "note": "Seed contains official courses explicitly mentioned in the guide and placeholder catalog courses for the remaining buckets"}'::jsonb);

INSERT INTO acad.cohorts (program_id, cohort_code, start_year, note)
SELECT program_id, 'BBS-2025', 2025, 'BBS/QTKD cohort seeded from uploaded guidance document'
FROM acad.programs
WHERE program_code = 'BBS';

-- ============================================================
-- 2) Terms
-- ============================================================

INSERT INTO acad.terms (term_code, start_date, end_date)
VALUES
(20251, '2025-01-13', '2025-05-09'),
(20252, '2025-05-26', '2025-08-15'),
(20253, '2025-09-01', '2025-12-19'),
(20261, '2026-01-12', '2026-05-08'),
(20262, '2026-05-25', '2026-08-14'),
(20263, '2026-09-07', '2026-12-18'),
(20271, '2027-01-11', '2027-05-07'),
(20272, '2027-05-24', '2027-08-13'),
(20273, '2027-09-06', '2027-12-17');

-- ============================================================
-- 3) Course catalog
-- ============================================================

INSERT INTO acad.courses
(course_code, course_name, credits, subject_prefix, course_level, is_language_prep)
VALUES
('IELP 1', 'Intensive English Level 1', 0, 'IELP', 0, true),
('IELP 2', 'Intensive English Level 2', 0, 'IELP', 0, true),
('IELP 3', 'Intensive English Level 3', 0, 'IELP', 0, true),
('IELP 4', 'Intensive English Level 4', 0, 'IELP', 0, true),
('IELP 5', 'Intensive English Level 5', 0, 'IELP', 0, true),
('POLS 132', 'Foundations of Political Theory', 3, 'POLS', 100, false),
('POLS 133', 'Ho Chi Minh Thought', 3, 'POLS', 100, false),
('POLS 134', 'History of the Communist Party of Vietnam', 3, 'POLS', 100, false),
('POLS 135', 'Scientific Socialism', 3, 'POLS', 100, false),
('POLS 136', 'Vietnamese State and Society', 3, 'POLS', 100, false),
('LAWS 122', 'Business Law Fundamentals', 3, 'LAWS', 100, false),
('BUS 101', 'Introduction to Business', 3, 'BUS', 100, false),
('WRT 122', 'Academic Writing', 3, 'WRT', 100, false),
('MTH 171', 'Business Mathematics', 3, 'MTH', 100, false),
('ECO 204', 'Principles of Economics', 3, 'ECO', 200, false),
('ACTG 240', 'Financial Accounting', 3, 'ACTG', 200, false),
('BUS 102', 'Business Communication', 4, 'BUS', 100, false),
('BUS 120', 'Digital Literacy for Business', 3, 'BUS', 100, false),
('BUS 130', 'Critical Thinking for Managers', 3, 'BUS', 100, false),
('BUS 201', 'Principles of Management', 4, 'BUS', 200, false),
('BUS 202', 'Organizational Behavior', 3, 'BUS', 200, false),
('BUS 210', 'Business Statistics', 3, 'BUS', 200, false),
('BUS 220', 'Operations Fundamentals', 3, 'BUS', 200, false),
('BUS 230', 'Business Research Basics', 3, 'BUS', 200, false),
('BUS 240', 'Marketing Principles', 3, 'BUS', 200, false),
('BUS 250', 'Managerial Accounting', 3, 'BUS', 200, false),
('BUS 260', 'Business Information Systems', 3, 'BUS', 200, false),
('BUS 100', 'Business Fundamentals (Legacy)', 3, 'BUS', 100, false),
('WRT 120', 'Writing Foundations (Legacy)', 3, 'WRT', 100, false),
('MATH 170', 'College Mathematics (Legacy)', 3, 'MATH', 100, false),
('ARTS 101', 'Introduction to Arts', 3, 'ARTS', 100, false),
('HIST 101', 'World History', 3, 'HIST', 100, false),
('PSYC 101', 'Introduction to Psychology', 3, 'PSYC', 100, false),
('SOCI 101', 'Introduction to Sociology', 3, 'SOCI', 100, false),
('COMM 101', 'Public Speaking', 3, 'COMM', 100, false),
('COMP 101', 'Computer Applications', 4, 'COMP', 100, false),
('STAD 101', 'Student Development', 3, 'STAD', 100, false),
('MKTG 201', 'Introduction to Marketing', 3, 'MKTG', 200, false),
('FIN 201', 'Introduction to Finance', 3, 'FIN', 200, false),
('MGMT 201', 'Introduction to Management Topics', 3, 'MGMT', 200, false),
('DATA 201', 'Data Literacy for Business', 3, 'DATA', 200, false),
('SCM 201', 'Introduction to Supply Chains', 3, 'SCM', 200, false),
('BUS 303', 'Business Research Methods', 3, 'BUS', 300, false),
('BUS 304', 'Business Ethics', 3, 'BUS', 300, false),
('BUS 305', 'Strategic Management Foundations', 3, 'BUS', 300, false),
('BUS 306', 'International Business', 3, 'BUS', 300, false),
('BUS 307', 'Business Analytics', 3, 'BUS', 300, false),
('BUS 308', 'Managerial Economics', 3, 'BUS', 300, false),
('BUS 309', 'Corporate Governance', 3, 'BUS', 300, false),
('BUS 310', 'Project Management', 3, 'BUS', 300, false),
('BUS 311', 'Leadership and Change', 3, 'BUS', 300, false),
('BUS 312', 'Innovation Management', 3, 'BUS', 300, false),
('BUS 313', 'Business Negotiation', 3, 'BUS', 300, false),
('BUS 315', 'Service Operations Management', 3, 'BUS', 300, false),
('BUS 316', 'Entrepreneurial Finance', 3, 'BUS', 300, false),
('BUS 450', 'Internship / Applied Business Project', 4, 'BUS', 400, false),
('BUS 496', 'Capstone Project', 4, 'BUS', 400, false),
('BUS 332', 'Marketing Concentration Gateway', 4, 'BUS', 300, false),
('MKTG 341', 'Consumer Behavior', 4, 'MKTG', 300, false),
('MKTG 342', 'Digital Marketing', 4, 'MKTG', 300, false),
('MKTG 441', 'Brand Management', 4, 'MKTG', 400, false),
('MKTG 442', 'Marketing Analytics', 4, 'MKTG', 400, false),
('BUS 359', 'Supply Chain Concentration Gateway', 4, 'BUS', 300, false),
('SCM 341', 'Procurement Management', 4, 'SCM', 300, false),
('SCM 342', 'Logistics Management', 4, 'SCM', 300, false),
('SCM 441', 'Supply Chain Analytics', 4, 'SCM', 400, false),
('SCM 442', 'Global Supply Networks', 4, 'SCM', 400, false),
('BUS 314', 'HRM/Hospitality Concentration Gateway', 4, 'BUS', 300, false),
('HRM 341', 'Talent Acquisition', 4, 'HRM', 300, false),
('HRM 342', 'Training and Development', 4, 'HRM', 300, false),
('HRM 441', 'Performance Management', 4, 'HRM', 400, false),
('HRM 442', 'Compensation and Benefits', 4, 'HRM', 400, false),
('HSP 341', 'Hospitality Operations', 4, 'HSP', 300, false),
('HSP 342', 'Hotel Revenue Management', 4, 'HSP', 300, false),
('HSP 441', 'Hospitality Service Design', 4, 'HSP', 400, false),
('HSP 442', 'Hospitality Experience Management', 4, 'HSP', 400, false),
('BUS 328', 'Finance/Accounting Concentration Gateway', 4, 'BUS', 300, false),
('FIN 341', 'Corporate Finance', 4, 'FIN', 300, false),
('FIN 342', 'Investment Analysis', 4, 'FIN', 300, false),
('ACTG 341', 'Management Accounting', 4, 'ACTG', 300, false),
('FIN 441', 'Financial Modeling', 4, 'FIN', 400, false),
('BUS 357', 'Entrepreneurship Concentration Gateway', 4, 'BUS', 300, false),
('ENTR 341', 'New Venture Creation', 4, 'ENTR', 300, false),
('ENTR 342', 'Startup Growth Strategy', 4, 'ENTR', 300, false),
('ENTR 441', 'Business Model Innovation', 4, 'ENTR', 400, false),
('ENTR 442', 'Entrepreneurial Leadership', 4, 'ENTR', 400, false),
('BUSE 310', 'Business Elective: Digital Commerce', 4, 'BUSE', 300, false),
('BUSE 320', 'Business Elective: Retail Strategy', 3, 'BUSE', 300, false),
('BUSE 330', 'Business Elective: Sustainable Business', 3, 'BUSE', 300, false),
('BUSE 340', 'Business Elective: Service Innovation', 3, 'BUSE', 300, false),
('BUSE 350', 'Business Elective: Customer Experience', 3, 'BUSE', 300, false),
('BUSE 360', 'Business Elective: Advanced Spreadsheet Modeling', 3, 'BUSE', 300, false),
('BUSE 370', 'Business Elective: Business Storytelling', 3, 'BUSE', 300, false),
('BUSE 380', 'Business Elective: Career Portfolio', 3, 'BUSE', 300, false),
('BUSE 410', 'Business Elective: ESG in Practice', 3, 'BUSE', 400, false),
('BUSE 420', 'Business Elective: Negotiation Lab', 3, 'BUSE', 400, false),
('BUSE 430', 'Business Elective: Emerging Markets', 3, 'BUSE', 400, false);

-- Course metadata used by application-layer audit/recommendation rules

UPDATE acad.courses SET meta = '{"track": "english-prep", "counts_toward_degree": false}'::jsonb WHERE subject_prefix = 'IELP';

UPDATE acad.courses SET meta = '{"delivery_language": "vi", "english_gate_exempt": true}'::jsonb WHERE course_code IN ('POLS 132', 'POLS 133', 'POLS 134', 'POLS 135', 'POLS 136', 'LAWS 122');

UPDATE acad.courses SET meta = '{"min_ielts": 5.5, "min_level": 5, "block": "core-100-200"}'::jsonb WHERE course_code IN ('BUS 101', 'WRT 122', 'MTH 171', 'ECO 204', 'ACTG 240', 'BUS 102');

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "core-100-200"}'::jsonb WHERE course_code IN ('BUS 120', 'BUS 130', 'BUS 201', 'BUS 202', 'BUS 210', 'BUS 220', 'BUS 230', 'BUS 240', 'BUS 250', 'BUS 260');

UPDATE acad.courses SET meta = '{"min_ielts": 5.5, "min_level": 5, "block": "elective-100-200"}'::jsonb WHERE course_code IN ('ARTS 101', 'HIST 101', 'PSYC 101', 'SOCI 101', 'COMM 101', 'COMP 101', 'STAD 101');

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "elective-100-200"}'::jsonb WHERE course_code IN ('MKTG 201', 'FIN 201', 'MGMT 201', 'DATA 201', 'SCM 201');

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "major-core-300-400", "priority_recommendation": true}'::jsonb WHERE course_code = 'BUS 303';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "major-core-300-400"}'::jsonb WHERE course_code IN ('BUS 304', 'BUS 305', 'BUS 306', 'BUS 307', 'BUS 308', 'BUS 309', 'BUS 310', 'BUS 311', 'BUS 312', 'BUS 313', 'BUS 315', 'BUS 316');

UPDATE acad.courses SET meta = jsonb_build_object(
  'min_ielts', 6.0,
  'min_level', 6,
  'block', 'major-core-300-400',
  'advisories', jsonb_build_object('stages', jsonb_build_array('start','final')),
  'requires_completion_of_level_100_200', true
)
WHERE course_code = 'BUS 450';

UPDATE acad.courses SET meta = jsonb_build_object(
  'min_ielts', 6.0,
  'min_level', 6,
  'block', 'major-core-300-400',
  'is_capstone', true,
  'advisories', jsonb_build_object('max_companion_courses', 2)
)
WHERE course_code = 'BUS 496';

UPDATE acad.courses SET meta = '{"legacy_equivalent": true}'::jsonb WHERE course_code IN ('BUS 100','WRT 120','MATH 170');

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Marketing", "is_concentration_gateway": true}'::jsonb WHERE course_code = 'BUS 332';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Marketing"}'::jsonb WHERE course_code = 'MKTG 341';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Marketing"}'::jsonb WHERE course_code = 'MKTG 342';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Marketing"}'::jsonb WHERE course_code = 'MKTG 441';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Marketing"}'::jsonb WHERE course_code = 'MKTG 442';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Supply Chain Management", "is_concentration_gateway": true}'::jsonb WHERE course_code = 'BUS 359';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Supply Chain Management"}'::jsonb WHERE course_code = 'SCM 341';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Supply Chain Management"}'::jsonb WHERE course_code = 'SCM 342';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Supply Chain Management"}'::jsonb WHERE course_code = 'SCM 441';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Supply Chain Management"}'::jsonb WHERE course_code = 'SCM 442';

UPDATE acad.courses SET meta = jsonb_build_object(
  'min_ielts', 6.0,
  'min_level', 6,
  'block', 'concentration-300-400',
  'concentration', jsonb_build_array('Human Resource Management','Hospitality Management'),
  'is_concentration_gateway', true
)
WHERE course_code = 'BUS 314';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Human Resource Management"}'::jsonb WHERE course_code = 'HRM 341';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Human Resource Management"}'::jsonb WHERE course_code = 'HRM 342';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Human Resource Management"}'::jsonb WHERE course_code = 'HRM 441';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Human Resource Management"}'::jsonb WHERE course_code = 'HRM 442';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Hospitality Management"}'::jsonb WHERE course_code = 'HSP 341';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Hospitality Management"}'::jsonb WHERE course_code = 'HSP 342';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Hospitality Management"}'::jsonb WHERE course_code = 'HSP 441';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Hospitality Management"}'::jsonb WHERE course_code = 'HSP 442';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Finance and Accounting", "is_concentration_gateway": true}'::jsonb WHERE course_code = 'BUS 328';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Finance and Accounting"}'::jsonb WHERE course_code = 'FIN 341';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Finance and Accounting"}'::jsonb WHERE course_code = 'FIN 342';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Finance and Accounting"}'::jsonb WHERE course_code = 'ACTG 341';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Finance and Accounting"}'::jsonb WHERE course_code = 'FIN 441';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Entrepreneurship", "is_concentration_gateway": true}'::jsonb WHERE course_code = 'BUS 357';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Entrepreneurship"}'::jsonb WHERE course_code = 'ENTR 341';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Entrepreneurship"}'::jsonb WHERE course_code = 'ENTR 342';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Entrepreneurship"}'::jsonb WHERE course_code = 'ENTR 441';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "concentration-300-400", "concentration": "Entrepreneurship"}'::jsonb WHERE course_code = 'ENTR 442';

UPDATE acad.courses SET meta = '{"min_ielts": 6.0, "min_level": 6, "block": "elective-300-400"}'::jsonb WHERE course_code IN ('BUSE 310', 'BUSE 320', 'BUSE 330', 'BUSE 340', 'BUSE 350', 'BUSE 360', 'BUSE 370', 'BUSE 380', 'BUSE 410', 'BUSE 420', 'BUSE 430');

-- ============================================================
-- 4) Curriculum categories
-- ============================================================

INSERT INTO acad.curriculum_categories (program_id, category_name, min_credits, sort_order)
VALUES
((SELECT program_id FROM acad.programs WHERE program_code = 'BBS'), 'LLCT & Pháp luật', 18, 1),
((SELECT program_id FROM acad.programs WHERE program_code = 'BBS'), 'Core 100-200 Required', 50, 2),
((SELECT program_id FROM acad.programs WHERE program_code = 'BBS'), 'Electives 100-200', 32, 3),
((SELECT program_id FROM acad.programs WHERE program_code = 'BBS'), 'Major Core 300-400 Required', 47, 4),
((SELECT program_id FROM acad.programs WHERE program_code = 'BBS'), 'Concentration 300-400', 20, 5),
((SELECT program_id FROM acad.programs WHERE program_code = 'BBS'), 'Electives 300-400', 28, 6);

-- ============================================================
-- 5) Curriculum requirements
--    Totals encoded from the guide:
--      - Level 100-200 = 68 required + 32 elective = 100 credits
--      - Level 300-400 = 47 required + 20 concentration + 28 elective = 95 credits
-- ============================================================

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'POLS 132', NULL, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'LLCT & Pháp luật';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'POLS 133', NULL, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'LLCT & Pháp luật';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'POLS 134', NULL, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'LLCT & Pháp luật';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'POLS 135', NULL, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'LLCT & Pháp luật';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'POLS 136', NULL, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'LLCT & Pháp luật';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'LAWS 122', NULL, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'LLCT & Pháp luật';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 101', '{"type": "english", "min_level": 5, "min_ielts": 5.5, "raw": "IELTS 5.5 or English Level 5"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'WRT 122', '{"type": "english", "min_level": 5, "min_ielts": 5.5, "raw": "IELTS 5.5 or English Level 5"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'MTH 171', '{"type": "english", "min_level": 5, "min_ielts": 5.5, "raw": "IELTS 5.5 or English Level 5"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'ECO 204', '{"type": "english", "min_level": 5, "min_ielts": 5.5, "raw": "IELTS 5.5 or English Level 5"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'ACTG 240', '{"type": "english", "min_level": 5, "min_ielts": 5.5, "raw": "IELTS 5.5 or English Level 5"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 102', '{"type": "english", "min_level": 5, "min_ielts": 5.5, "raw": "IELTS 5.5 or English Level 5"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 120', '{"type": "english", "min_level": 6, "min_ielts": 6.0, "raw": "IELTS 6.0 or higher"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 130', '{"type": "english", "min_level": 6, "min_ielts": 6.0, "raw": "IELTS 6.0 or higher"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 201', '{"type": "course", "courses": [{"code": "BUS 101", "min_grade": "C"}], "raw": "BUS 101 (C or higher); English IELTS 6.0 handled in course metadata"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 202', '{"type": "course", "courses": [{"code": "ACTG 240", "min_grade": "C"}], "raw": "ACTG 240 (C or higher); English IELTS 6.0 handled in course metadata"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 210', '{"type": "course", "courses": [{"code": "MTH 171", "min_grade": "C"}], "raw": "MTH 171 (C or higher); English IELTS 6.0 handled in course metadata"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 220', '{"type": "course", "courses": [{"code": "BUS 101", "min_grade": "C"}], "raw": "BUS 101 (C or higher); English IELTS 6.0 handled in course metadata"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 230', '{"type": "course", "courses": [{"code": "WRT 122", "min_grade": "C"}], "raw": "WRT 122 (C or higher); English IELTS 6.0 handled in course metadata"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 240', '{"type": "course", "courses": [{"code": "BUS 101", "min_grade": "C"}], "raw": "BUS 101 (C or higher); English IELTS 6.0 handled in course metadata"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 250', '{"type": "course", "courses": [{"code": "ACTG 240", "min_grade": "C"}], "raw": "ACTG 240 (C or higher); English IELTS 6.0 handled in course metadata"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 260', '{"type": "course", "courses": [{"code": "BUS 102", "min_grade": "C"}], "raw": "BUS 102 (C or higher); English IELTS 6.0 handled in course metadata"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Core 100-200 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, min_credits, allowed_courses, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'credit_bucket', 32, ARRAY['ARTS 101', 'HIST 101', 'PSYC 101', 'SOCI 101', 'COMM 101', 'COMP 101', 'STAD 101', 'MKTG 201', 'FIN 201', 'MGMT 201', 'DATA 201', 'SCM 201']::acad.course_code[], '{"type": "policy", "raw": "Pool contains 100-200 electives; per-course English thresholds are stored in acad.courses.meta"}'::jsonb, 'Complete at least 32 credits from the approved 100-200 elective pool.'
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Electives 100-200';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 303', '{"type": "course", "courses": [{"code": "BUS 230", "min_grade": "C"}], "raw": "BUS 230 (C or higher); student should already have completed 100 credits at level 100-200"}'::jsonb, 'Strongly recommended in the first 300-level term.'
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 304', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); IELTS 6.0 and 100 credits at level 100-200 handled at application layer"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 305', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); IELTS 6.0 and 100 credits at level 100-200 handled at application layer"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 306', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); IELTS 6.0 and 100 credits at level 100-200 handled at application layer"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 307', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); IELTS 6.0 and 100 credits at level 100-200 handled at application layer"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 308', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); IELTS 6.0 and 100 credits at level 100-200 handled at application layer"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 309', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); IELTS 6.0 and 100 credits at level 100-200 handled at application layer"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 310', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); IELTS 6.0 and 100 credits at level 100-200 handled at application layer"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 311', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); IELTS 6.0 and 100 credits at level 100-200 handled at application layer"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 312', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); IELTS 6.0 and 100 credits at level 100-200 handled at application layer"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 313', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); IELTS 6.0 and 100 credits at level 100-200 handled at application layer"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 315', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); IELTS 6.0 and 100 credits at level 100-200 handled at application layer"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 316', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); IELTS 6.0 and 100 credits at level 100-200 handled at application layer"}'::jsonb, NULL
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 450', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); start/final internship stages are handled by faculty workflow"}'::jsonb, 'BUS 450 has start/final stages; final registration is usually performed officially in AAO.'
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, course_code, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'course', 'BUS 496', '{"type": "course", "courses": [{"code": "BUS 303", "min_grade": "C"}], "raw": "BUS 303 (C or higher); capstone workload policy handled at application layer"}'::jsonb, 'Capstone course; faculty recommends taking only 1-2 additional courses in the same term.'
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Major Core 300-400 Required';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, min_credits, allowed_courses, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'credit_bucket', 20, ARRAY['BUS 332', 'MKTG 341', 'MKTG 342', 'MKTG 441', 'MKTG 442', 'BUS 359', 'SCM 341', 'SCM 342', 'SCM 441', 'SCM 442', 'BUS 314', 'HRM 341', 'HRM 342', 'HRM 441', 'HRM 442', 'HSP 341', 'HSP 342', 'HSP 441', 'HSP 442', 'BUS 328', 'FIN 341', 'FIN 342', 'ACTG 341', 'FIN 441', 'BUS 357', 'ENTR 341', 'ENTR 342', 'ENTR 441', 'ENTR 442']::acad.course_code[], '{"type": "policy", "raw": "Application filters this pool by the concentration selected by the student (stored in student meta or an extension table)."}'::jsonb, 'Complete at least 20 concentration credits. Typical concentrations in the guide are Marketing, Supply Chain Management, HRM, Hospitality, Finance/Accounting, Entrepreneurship.'
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Concentration 300-400';

INSERT INTO acad.curriculum_requirements
(cohort_id, category_id, kind, min_credits, allowed_courses, prereq_rule, note)
SELECT c.cohort_id, cat.category_id, 'credit_bucket', 28, ARRAY['BUSE 310', 'BUSE 320', 'BUSE 330', 'BUSE 340', 'BUSE 350', 'BUSE 360', 'BUSE 370', 'BUSE 380', 'BUSE 410', 'BUSE 420', 'BUSE 430']::acad.course_code[], '{"type": "english", "min_level": 6, "min_ielts": 6.0, "raw": "IELTS 6.0 or higher"}'::jsonb, 'Complete at least 28 credits from approved level 300-400 electives.'
FROM acad.cohorts c
JOIN acad.curriculum_categories cat ON cat.program_id = c.program_id
WHERE c.cohort_code = 'BBS-2025'
  AND cat.category_name = 'Electives 300-400';

-- ============================================================
-- 6) Legacy equivalencies
-- ============================================================

INSERT INTO acad.equivalency_sets (program_id, title, note)
SELECT program_id, 'BBS Legacy Equivalencies', 'Legacy catalog codes accepted by audit engine'
FROM acad.programs
WHERE program_code = 'BBS';

INSERT INTO acad.equivalencies (equiv_set_id, course_code, equivalent_course_code, cohort_id, note)
SELECT es.equiv_set_id, 'BUS 101', 'BUS 100', c.cohort_id, 'Legacy equivalent accepted for introduction to business'
FROM acad.equivalency_sets es
JOIN acad.cohorts c ON c.program_id = es.program_id
WHERE es.title = 'BBS Legacy Equivalencies' AND c.cohort_code = 'BBS-2025';

INSERT INTO acad.equivalencies (equiv_set_id, course_code, equivalent_course_code, cohort_id, note)
SELECT es.equiv_set_id, 'WRT 122', 'WRT 120', c.cohort_id, 'Legacy equivalent accepted for academic writing'
FROM acad.equivalency_sets es
JOIN acad.cohorts c ON c.program_id = es.program_id
WHERE es.title = 'BBS Legacy Equivalencies' AND c.cohort_code = 'BBS-2025';

INSERT INTO acad.equivalencies (equiv_set_id, course_code, equivalent_course_code, cohort_id, note)
SELECT es.equiv_set_id, 'MTH 171', 'MATH 170', c.cohort_id, 'Legacy equivalent accepted for business mathematics'
FROM acad.equivalency_sets es
JOIN acad.cohorts c ON c.program_id = es.program_id
WHERE es.title = 'BBS Legacy Equivalencies' AND c.cohort_code = 'BBS-2025';

-- ============================================================
-- 7) Students
-- ============================================================

INSERT INTO acad.students
(student_id, last_name, first_name, program_id, cohort_id, status, english_level, ielts_score, meta)
SELECT
  '20250001',
  'Nguyen',
  'An',
  p.program_id,
  c.cohort_id,
  'active',
  6,
  6.5,
  '{"concentration": "Marketing", "notes": "Demo student already meets IELTS 6.0 from entry"}'::jsonb
FROM acad.programs p
JOIN acad.cohorts c ON c.program_id = p.program_id
WHERE p.program_code = 'BBS'
  AND c.cohort_code = 'BBS-2025';

INSERT INTO acad.students
(student_id, last_name, first_name, program_id, cohort_id, status, english_level, ielts_score, meta)
SELECT
  '20250002',
  'Tran',
  'Binh',
  p.program_id,
  c.cohort_id,
  'active',
  5,
  5.5,
  '{"concentration": "Supply Chain Management", "notes": "Demo student at IELTS 5.5 / Level 5 stage"}'::jsonb
FROM acad.programs p
JOIN acad.cohorts c ON c.program_id = p.program_id
WHERE p.program_code = 'BBS'
  AND c.cohort_code = 'BBS-2025';

INSERT INTO acad.students
(student_id, last_name, first_name, program_id, cohort_id, status, english_level, ielts_score, meta)
SELECT
  '20250003',
  'Le',
  'Chi',
  p.program_id,
  c.cohort_id,
  'active',
  2,
  NULL,
  '{"notes": "Demo student starting from English preparation pathway"}'::jsonb
FROM acad.programs p
JOIN acad.cohorts c ON c.program_id = p.program_id
WHERE p.program_code = 'BBS'
  AND c.cohort_code = 'BBS-2025';

-- ============================================================
-- 8) Course attempts
-- ============================================================

INSERT INTO acad.course_attempts
(student_id, course_code, term_code, term_seq, attempt_no, credits, grade_letter, is_completed)
VALUES
('20250001', 'BUS 101', 20251, 1, 1, 3, 'B', true),
('20250001', 'WRT 122', 20251, 1, 1, 3, 'B+', true),
('20250001', 'MTH 171', 20251, 1, 1, 3, 'A-', true),
('20250001', 'POLS 132', 20251, 1, 1, 3, 'B', true),
('20250001', 'POLS 133', 20251, 1, 1, 3, 'B', true),
('20250001', 'ECO 204', 20252, 2, 1, 3, 'B', true),
('20250001', 'ACTG 240', 20252, 2, 1, 3, 'B-', true),
('20250001', 'BUS 102', 20252, 2, 1, 4, 'B+', true),
('20250001', 'POLS 134', 20252, 2, 1, 3, 'B', true),
('20250001', 'LAWS 122', 20252, 2, 1, 3, 'B', true),
('20250001', 'BUS 120', 20253, 3, 1, 3, 'B', true),
('20250001', 'BUS 130', 20253, 3, 1, 3, 'B+', true),
('20250001', 'BUS 201', 20253, 3, 1, 4, 'B', true),
('20250001', 'BUS 202', 20253, 3, 1, 3, 'B-', true),
('20250001', 'POLS 135', 20253, 3, 1, 3, 'B', true),
('20250001', 'POLS 136', 20253, 3, 1, 3, 'B', true),
('20250001', 'BUS 210', 20261, 4, 1, 3, 'B+', true),
('20250001', 'BUS 220', 20261, 4, 1, 3, 'B', true),
('20250001', 'BUS 230', 20261, 4, 1, 3, 'B', true),
('20250001', 'BUS 240', 20261, 4, 1, 3, 'B+', true),
('20250001', 'BUS 250', 20261, 4, 1, 3, 'B', true),
('20250001', 'HIST 101', 20261, 4, 1, 3, 'A', true),
('20250001', 'PSYC 101', 20261, 4, 1, 3, 'A-', true),
('20250001', 'BUS 260', 20262, 5, 1, 3, 'B', true),
('20250001', 'COMM 101', 20262, 5, 1, 3, 'A-', true),
('20250001', 'COMP 101', 20262, 5, 1, 4, 'B+', true),
('20250001', 'MKTG 201', 20262, 5, 1, 3, 'A', true),
('20250001', 'FIN 201', 20262, 5, 1, 3, 'B+', true),
('20250001', 'DATA 201', 20262, 5, 1, 3, 'A', true),
('20250001', 'SOCI 101', 20262, 5, 1, 3, 'A-', true),
('20250001', 'STAD 101', 20263, 6, 1, 3, 'A', true),
('20250001', 'SCM 201', 20263, 6, 1, 3, 'A-', true),
('20250001', 'MGMT 201', 20263, 6, 1, 3, 'A', true),
('20250001', 'BUS 303', 20263, 6, 1, 3, 'B+', true),
('20250001', 'BUS 332', 20263, 6, 1, 4, 'B', true),
('20250001', 'BUSE 310', 20263, 6, 1, 4, 'B', true),
('20250002', 'IELP 5', 20251, 1, 1, 0, 'P', true),
('20250002', 'BUS 100', 20251, 1, 1, 3, 'B', true),
('20250002', 'POLS 132', 20251, 1, 1, 3, 'C+', true),
('20250002', 'POLS 133', 20251, 1, 1, 3, 'B', true),
('20250002', 'LAWS 122', 20251, 1, 1, 3, 'B', true),
('20250002', 'WRT 120', 20252, 2, 1, 3, 'B', true),
('20250002', 'MTH 171', 20252, 2, 1, 3, 'C', true),
('20250002', 'ECO 204', 20252, 2, 1, 3, 'C+', true),
('20250002', 'ACTG 240', 20252, 2, 1, 3, 'B-', true),
('20250002', 'POLS 134', 20252, 2, 1, 3, 'B', true),
('20250002', 'BUS 102', 20253, 3, 1, 4, 'B-', true),
('20250002', 'BUS 120', 20253, 3, 1, 3, 'C+', true),
('20250002', 'BUS 130', 20253, 3, 1, 3, 'B-', true),
('20250002', 'POLS 135', 20253, 3, 1, 3, 'B', true),
('20250002', 'POLS 136', 20253, 3, 1, 3, 'B', true),
('20250003', 'IELP 1', 20251, 1, 1, 0, 'P', true),
('20250003', 'POLS 132', 20251, 1, 1, 3, 'B', true),
('20250003', 'IELP 2', 20252, 2, 1, 0, 'P', true),
('20250003', 'POLS 133', 20252, 2, 1, 3, 'B', true),
('20250003', 'IELP 3', 20253, 3, 1, 0, 'P', true);

-- ============================================================
-- 9) Student plans
-- ============================================================

INSERT INTO acad.student_plans
(student_id, term_code, course_code, status, note)
VALUES
('20250001', 20271, 'MKTG 341', 'planned', 'Marketing concentration continuation'),
('20250001', 20271, 'MKTG 342', 'planned', 'Marketing concentration continuation'),
('20250001', 20271, 'BUS 304', 'planned', 'Major core'),
('20250001', 20271, 'BUSE 320', 'planned', '300-400 elective'),
('20250002', 20261, 'BUS 201', 'planned', 'Continue 200-level core after completing BUS 100 equivalent'),
('20250002', 20261, 'BUS 202', 'planned', 'Continue 200-level core'),
('20250002', 20261, 'BUS 210', 'planned', 'Continue 200-level core'),
('20250002', 20261, 'HIST 101', 'planned', '100-200 elective'),
('20250002', 20261, 'FIN 201', 'planned', '100-200 elective'),
('20250003', 20261, 'IELP 4', 'planned', 'Continue English preparation'),
('20250003', 20261, 'POLS 134', 'planned', 'Vietnamese-taught requirement can be taken without IELTS 6.0');

COMMIT;
