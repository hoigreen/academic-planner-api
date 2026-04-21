BEGIN;

DO $$
DECLARE
  rec record;
BEGIN
  FOR rec IN
    SELECT DISTINCT p.program_id, c.cohort_id
    FROM acad.programs p
    JOIN acad.cohorts c
      ON c.program_id = p.program_id
    JOIN acad.curriculum_categories cat
      ON cat.program_id = p.program_id
    JOIN acad.curriculum_requirements req
      ON req.cohort_id = c.cohort_id
     AND req.category_id = cat.category_id
    ORDER BY p.program_id, c.cohort_id
  LOOP
    PERFORM acad.sync_curriculum_structure(rec.program_id, rec.cohort_id);
  END LOOP;
END $$;

COMMIT;
