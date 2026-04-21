BEGIN;

DO $$
DECLARE
  v_program_id bigint;
  v_cohort_id  bigint;
BEGIN
  -- Sync BBS (if seeded)
  SELECT program_id INTO v_program_id FROM acad.programs WHERE program_code = 'BBS';
  SELECT cohort_id INTO v_cohort_id FROM acad.cohorts
    WHERE program_id = v_program_id AND cohort_code = 'K2025';
  IF v_program_id IS NOT NULL AND v_cohort_id IS NOT NULL THEN
    PERFORM acad.sync_curriculum_structure(v_program_id, v_cohort_id);
  END IF;

  -- Sync PM (if seeded)
  SELECT program_id INTO v_program_id FROM acad.programs WHERE program_code = 'PM';
  SELECT cohort_id INTO v_cohort_id FROM acad.cohorts
    WHERE program_id = v_program_id AND cohort_code = 'K13';
  IF v_program_id IS NOT NULL AND v_cohort_id IS NOT NULL THEN
    PERFORM acad.sync_curriculum_structure(v_program_id, v_cohort_id);
  END IF;
END $$;

COMMIT;
