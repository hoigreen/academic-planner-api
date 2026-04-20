-- ============================================================
-- ORDBMS Extension: knowledge_block composite type + curricula table
-- Uses PostgreSQL composite type arrays and JSONB course mapping
-- ============================================================

BEGIN;

-- Composite type for flexible curriculum structure blocks
DO $$ BEGIN
  CREATE TYPE acad.knowledge_block AS (
    block_name       TEXT,
    min_credits_required INT,
    is_mandatory     BOOLEAN,
    description      TEXT
  );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- Curricula table linking major + cohort to a flexible structure
CREATE TABLE IF NOT EXISTS acad.curricula (
  curriculum_id  bigserial PRIMARY KEY,
  program_id     bigint NOT NULL REFERENCES acad.programs(program_id) ON DELETE RESTRICT,
  cohort_id      bigint NOT NULL REFERENCES acad.cohorts(cohort_id) ON DELETE RESTRICT,
  major_name     text NOT NULL,
  cohort_code    text NOT NULL,

  -- ORDBMS: Array of composite type for curriculum structure blocks
  structure      acad.knowledge_block[] NOT NULL DEFAULT '{}',

  -- JSONB: Dynamic mapping of block names to course codes
  -- e.g. {"Business Core": ["BUS101", "STA202"], "Electives": ["MKT301"]}
  course_mapping jsonb NOT NULL DEFAULT '{}'::jsonb,

  total_credits  numeric(6,1) CHECK (total_credits IS NULL OR total_credits >= 0),
  meta           jsonb NOT NULL DEFAULT '{}'::jsonb,
  created_at     timestamptz NOT NULL DEFAULT now(),
  updated_at     timestamptz NOT NULL DEFAULT now(),

  UNIQUE(program_id, cohort_id)
);

CREATE INDEX IF NOT EXISTS gin_curricula_course_mapping
ON acad.curricula USING GIN (course_mapping);

CREATE INDEX IF NOT EXISTS idx_curricula_program
ON acad.curricula (program_id);

-- Function to auto-populate curricula from existing curriculum_categories + requirements
CREATE OR REPLACE FUNCTION acad.sync_curriculum_structure(p_program_id bigint, p_cohort_id bigint)
RETURNS bigint AS $$
DECLARE
  v_curriculum_id bigint;
  v_program_code text;
  v_cohort_code text;
  v_blocks acad.knowledge_block[];
  v_mapping jsonb := '{}'::jsonb;
  v_total numeric(6,1) := 0;
  rec RECORD;
BEGIN
  SELECT program_code INTO v_program_code FROM acad.programs WHERE program_id = p_program_id;
  SELECT cohort_code INTO v_cohort_code FROM acad.cohorts WHERE cohort_id = p_cohort_id;

  FOR rec IN
    SELECT
      cc.category_name,
      cc.min_credits,
      COALESCE(
        jsonb_agg(DISTINCT cr.course_code) FILTER (WHERE cr.course_code IS NOT NULL),
        '[]'::jsonb
      ) AS course_codes,
      bool_or(cr.is_required) AS has_required
    FROM acad.curriculum_categories cc
    LEFT JOIN acad.curriculum_requirements cr
      ON cr.category_id = cc.category_id AND cr.cohort_id = p_cohort_id
    WHERE cc.program_id = p_program_id
    GROUP BY cc.category_id, cc.category_name, cc.min_credits, cc.sort_order
    ORDER BY cc.sort_order NULLS LAST
  LOOP
    v_blocks := array_append(v_blocks, ROW(
      rec.category_name,
      COALESCE(rec.min_credits::int, 0),
      COALESCE(rec.has_required, false),
      NULL
    )::acad.knowledge_block);
    v_mapping := v_mapping || jsonb_build_object(rec.category_name, rec.course_codes);
    v_total := v_total + COALESCE(rec.min_credits, 0);
  END LOOP;

  INSERT INTO acad.curricula (program_id, cohort_id, major_name, cohort_code, structure, course_mapping, total_credits)
  VALUES (p_program_id, p_cohort_id, v_program_code, v_cohort_code, COALESCE(v_blocks, '{}'), v_mapping, v_total)
  ON CONFLICT (program_id, cohort_id) DO UPDATE SET
    structure = EXCLUDED.structure,
    course_mapping = EXCLUDED.course_mapping,
    total_credits = EXCLUDED.total_credits,
    updated_at = now()
  RETURNING curriculum_id INTO v_curriculum_id;

  RETURN v_curriculum_id;
END;
$$ LANGUAGE plpgsql;

COMMIT;
