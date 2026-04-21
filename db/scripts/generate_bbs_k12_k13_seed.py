#!/usr/bin/env python3
"""Generate db/50_seed_bbs_k12_k13_curriculum.sql from the K12 and K13 BBS
audit CSVs under db/input_csv/.

The generated SQL:
  1. Inserts BBS cohorts K12 and K13.
  2. Inserts every distinct course code into acad.courses.
  3. Inserts per-cohort curriculum categories into acad.curriculum_categories
     (cohort-prefixed so K12 and K13 can co-exist under program BBS).
  4. Inserts acad.curriculum_requirements rows of kind='course' tying each
     course to its (cohort, category).

All INSERTs use ON CONFLICT ... DO NOTHING / DO UPDATE so reruns are safe.
"""
from __future__ import annotations

import csv
import json
import re
from pathlib import Path
from typing import Dict, List, Optional, Tuple

HERE = Path(__file__).resolve().parent
DB_DIR = HERE.parent
CSV_DIR = DB_DIR / "input_csv"
OUT_PATH = DB_DIR / "50_seed_bbs_k12_k13_curriculum.sql"

# course_code domain regex (mirrors acad.course_code CHECK)
COURSE_RE = re.compile(r"^[A-Z]{2,8}[ ]?[0-9]{1,4}$")
# pattern for raw codes found in the CSV (may include a single hyphen)
RAW_CODE_RE = re.compile(r"^[A-Z]{2,8}(?:-[A-Z])?[ ]?[0-9]{1,4}$")


def normalize_code(raw: str) -> Optional[str]:
    """Normalize a raw course code so it satisfies the acad.course_code domain.

    Examples:
      'LNGS-J101' -> 'LNGSJ 101'
      'FIN-B 439' -> 'FINB 439'
      'BUS 101'   -> 'BUS 101'
    Returns None if the code can't be normalized.
    """
    raw = raw.strip()
    if not raw:
        return None
    s = raw.replace("-", "")
    # Insert a space between the trailing digits and the letter prefix for
    # readability, if no space is present yet.
    m = re.match(r"^([A-Z]+)([0-9]+)$", s)
    if m:
        s = f"{m.group(1)} {m.group(2)}"
    if COURSE_RE.match(s):
        return s
    return None


def split_prereq_tokens(text: str) -> List[str]:
    """Split a prerequisites string on commas / 'and' / slashes and return the
    normalized course codes found inside. Anything that isn't a course code
    (e.g. 'IELTS 6.0', '(HT)', 'chỉ <K8') is dropped.
    """
    if not text:
        return []
    cleaned = re.sub(r"\(.*?\)", " ", text)
    parts = re.split(r"[,/]|\s+and\s+|\s+AND\s+", cleaned)
    out: List[str] = []
    for p in parts:
        token = p.strip().rstrip(".")
        if not token:
            continue
        code = normalize_code(token)
        if code and code not in out:
            out.append(code)
    return out


def build_prereq_rule(prereq_text: str, ielts_text: str) -> Optional[dict]:
    """Translate the Prerequisites (+ optional IELTS) cells of the CSV into
    the polymorphic JSONB rule understood by the schema. Returns None when no
    rule can be inferred.
    """
    args: List[dict] = []
    for code in split_prereq_tokens(prereq_text or ""):
        args.append({"op": "COMPLETED", "course": code})

    if ielts_text:
        m = re.search(r"IELTS\s*([0-9]+(?:\.[0-9]+)?)", ielts_text)
        if m:
            ielts = float(m.group(1))
            level_m = re.search(r"Level\s*([0-9]+)", ielts_text)
            level = int(level_m.group(1)) if level_m else 5
            args.append({"op": "ENGLISH", "min_level": level, "min_ielts": ielts})

    if not args:
        return None
    if len(args) == 1:
        return args[0]
    return {"op": "AND", "args": args}


# ---------------------------------------------------------------------------
# Parsers
# ---------------------------------------------------------------------------

CONCENTRATION_LABELS = {
    "Supply Chain",
    "Accounting",
    "Entrepreneurship",
    "Finance",
    "Human Resources",
    "Marketing",
    "Hospitality",
    "Business Analytics",
}


def clean_header(text: str) -> str:
    return re.sub(r"\s+", " ", (text or "").replace("\n", " ").strip())


def parse_csv(path: Path, cohort: str) -> Tuple[
    Dict[str, Tuple[str, Optional[int]]],  # courses: code -> (name, credits)
    List[dict],  # categories: [{name, min_credits, sort_order}]
    List[dict],  # requirements: [{category_name, course_code, is_required, prereq_rule, original_code, ielts}]
]:
    """Return (courses, categories, requirements) extracted from one CSV.

    * K13 columns: Category, Course, Course name, Grade, Grade, Prereq, IELTS, CRH, ...
    * K12 columns: Category, Course, Course name, Grade, Grade, Prereq, Credit, ...
    """
    is_k13 = cohort == "K13"
    courses: Dict[str, Tuple[str, Optional[int]]] = {}
    categories: Dict[str, dict] = {}
    requirements: List[dict] = []

    current_category: Optional[str] = None
    current_concentration: Optional[str] = None
    sort_order = 0
    # Within a category, an ",Electives,at least N credits,..." row flips this
    # flag so subsequent courses in the same category are treated as electives.
    electives_section: bool = False

    def ensure_category(name: str, *, min_credits: Optional[int] = None) -> None:
        nonlocal sort_order
        if name in categories:
            if min_credits is not None and categories[name]["min_credits"] is None:
                categories[name]["min_credits"] = min_credits
            return
        sort_order += 1
        categories[name] = {
            "name": name,
            "min_credits": min_credits,
            "sort_order": sort_order,
        }

    with path.open(newline="", encoding="utf-8") as fh:
        reader = csv.reader(fh)
        rows = list(reader)

    # Row 0 is the MSSV / metadata preamble (note that the original CSVs have a
    # quoted multi-line cell there which the csv module already merges into a
    # single row). Every row after that can contain either a category header,
    # a column-header ("Course, Course name, ..."), an empty separator, or a
    # real course entry.
    for row in rows[1:]:
        if not row:
            continue
        row = (row + [""] * 8)[:8]
        cat_cell = clean_header(row[0])
        col_code = row[1].strip()
        col_name = row[2].strip()
        prereq_raw = row[5].strip() if len(row) > 5 else ""
        if is_k13:
            ielts_raw = row[6].strip() if len(row) > 6 else ""
            credits_raw = row[7].strip() if len(row) > 7 else ""
        else:
            ielts_raw = ""
            credits_raw = row[6].strip() if len(row) > 6 else ""

        # Category transitions -------------------------------------------------
        if cat_cell:
            electives_section = False
            cat_upper = cat_cell.upper()
            if cat_upper in {"0", "MSSV"}:
                current_concentration = None
            elif cat_cell in CONCENTRATION_LABELS:
                current_concentration = cat_cell
                current_category = f"{cohort}: Concentration - {cat_cell}"
                ensure_category(current_category)
            else:
                current_concentration = None
                normalized = cat_cell.strip()
                if normalized.upper().startswith("CONCENTRATION"):
                    current_category = f"{cohort}: Concentration"
                elif normalized.upper().startswith("ADVANCED CORE"):
                    current_category = f"{cohort}: Advanced Core"
                    min_cr = 47 if "47" in normalized else None
                    ensure_category(current_category, min_credits=min_cr)
                    continue
                elif normalized.upper().startswith("ELECTIVES 300-400"):
                    current_category = f"{cohort}: Electives 300-400"
                elif normalized.upper().startswith("BUSINESS FOUNDATION CORE"):
                    current_category = f"{cohort}: Business Foundation Core"
                elif normalized.upper().startswith("FOUNDATION CORE"):
                    current_category = f"{cohort}: Foundation Core"
                    min_cr = 32 if "32" in normalized else None
                    ensure_category(current_category, min_credits=min_cr)
                    continue
                elif normalized.upper().startswith("GENERAL"):
                    current_category = f"{cohort}: General"
                    min_cr = 35 if "35" in normalized else None
                    ensure_category(current_category, min_credits=min_cr)
                    continue
                elif re.match(r"^[1-6]\.", normalized):
                    current_category = f"{cohort}: " + normalized.lstrip("0123456789. ").strip()
                else:
                    current_category = f"{cohort}: {normalized}"
                ensure_category(current_category)

        if current_category is None:
            continue

        # Concentration sub-group labels live in column 1 (col_code) rather
        # than column 0, e.g. ",Supply Chain,28 Credits ELECTIVES,...". If we
        # are currently inside the CONCENTRATION block, treat them as
        # sub-category transitions.
        if (
            current_category == f"{cohort}: Concentration"
            and col_code in CONCENTRATION_LABELS
        ):
            current_concentration = col_code
            sub_name = f"{cohort}: Concentration - {col_code}"
            ensure_category(sub_name)
            # try to pull min_credits from "NN Credits ELECTIVES" in col_name
            m = re.search(r"([0-9]+)\s*Credits?", col_name or "", re.IGNORECASE)
            if m:
                categories[sub_name]["min_credits"] = int(m.group(1))
            continue
        if current_concentration is not None:
            # we already descended into a sub-category; use it as the target
            target_category = f"{cohort}: Concentration - {current_concentration}"
        else:
            target_category = current_category

        # Sub-group markers inside a category (e.g. "Electives at least 4 credits").
        # From this point in the current category, subsequent course rows are
        # electives rather than required courses.
        if col_code.lower() == "electives":
            m = re.search(r"at least\s+([0-9]+)\s+credit", (col_name or "").lower())
            if m:
                ensure_category(target_category, min_credits=int(m.group(1)))
            electives_section = True
            continue

        if col_code.lower() in {"course", ""}:
            continue
        if col_code.startswith("*") or col_code.startswith("**"):
            continue

        code = normalize_code(col_code)
        if code is None:
            if not RAW_CODE_RE.match(col_code):
                continue
            continue

        ensure_category(target_category)

        # Course catalog entry
        try:
            credits = int(credits_raw) if credits_raw and credits_raw.isdigit() else None
        except ValueError:
            credits = None
        name = col_name or courses.get(code, ("", None))[0]
        if code not in courses or (name and not courses[code][0]):
            courses[code] = (name, credits)
        elif credits is not None and courses[code][1] is None:
            courses[code] = (courses[code][0], credits)

        # Requirement
        requirements.append({
            "cohort": cohort,
            "category_name": target_category,
            "course_code": code,
            "original_code": col_code,
            "prereq_rule": build_prereq_rule(prereq_raw, ielts_raw),
            "ielts": ielts_raw or None,
            "credits": credits,
            "is_required": not electives_section,
        })

    return courses, list(categories.values()), requirements


# ---------------------------------------------------------------------------
# SQL emitter
# ---------------------------------------------------------------------------

def esc(s: Optional[str]) -> str:
    if s is None:
        return "NULL"
    return "'" + s.replace("'", "''") + "'"


def main() -> None:
    k12_courses, k12_cats, k12_reqs = parse_csv(CSV_DIR / "4. BBS audit.xlsx - _K12.csv", "K12")
    k13_courses, k13_cats, k13_reqs = parse_csv(CSV_DIR / "4. BBS audit.xlsx - _K13.csv", "K13")

    all_courses: Dict[str, Tuple[str, Optional[int]]] = {}
    for src in (k12_courses, k13_courses):
        for code, (name, credits) in src.items():
            cur = all_courses.get(code)
            if cur is None:
                all_courses[code] = (name, credits)
            else:
                all_courses[code] = (
                    cur[0] or name,
                    cur[1] if cur[1] is not None else credits,
                )

    lines: List[str] = []
    lines.append("-- Generated by db/scripts/generate_bbs_k12_k13_seed.py")
    lines.append("-- Source: db/input_csv/4. BBS audit.xlsx - _K12.csv")
    lines.append("--         db/input_csv/4. BBS audit.xlsx - _K13.csv")
    lines.append("BEGIN;")
    lines.append("")

    # 1) Cohorts
    lines.append("-- 1) BBS cohorts K12 and K13")
    lines.append(
        "INSERT INTO acad.cohorts (program_id, cohort_code, start_year, note)"
    )
    lines.append(
        "SELECT p.program_id, v.code, v.yr, v.note FROM acad.programs p"
    )
    lines.append("CROSS JOIN (VALUES")
    lines.append("  ('K12', 2020, 'BBS K12 cohort (from 4. BBS audit.xlsx - _K12.csv)'),")
    lines.append("  ('K13', 2023, 'BBS K13 cohort (from 4. BBS audit.xlsx - _K13.csv)')")
    lines.append(") AS v(code, yr, note)")
    lines.append("WHERE p.program_code = 'BBS'")
    lines.append("ON CONFLICT (program_id, cohort_code) DO NOTHING;")
    lines.append("")

    # 2) Courses
    lines.append("-- 2) Course catalog entries (union of K12 + K13)")
    lines.append(
        "INSERT INTO acad.courses (course_code, course_name, credits, subject_prefix, course_level)"
    )
    lines.append("VALUES")
    value_rows: List[str] = []
    for code in sorted(all_courses):
        name, credits = all_courses[code]
        prefix = re.match(r"^([A-Z]+)", code).group(1)
        level_m = re.search(r"([0-9]+)$", code)
        level = int(level_m.group(1)) if level_m else None
        if level is not None:
            level = min(level, 999)
        value_rows.append(
            f"  ({esc(code)}, {esc(name or None)}, "
            f"{'NULL' if credits is None else credits}, "
            f"{esc(prefix)}, {'NULL' if level is None else level})"
        )
    lines.append(",\n".join(value_rows))
    lines.append("ON CONFLICT (course_code) DO UPDATE SET")
    lines.append("  course_name = COALESCE(acad.courses.course_name, EXCLUDED.course_name),")
    lines.append("  credits     = COALESCE(acad.courses.credits, EXCLUDED.credits),")
    lines.append("  subject_prefix = COALESCE(acad.courses.subject_prefix, EXCLUDED.subject_prefix),")
    lines.append("  course_level = COALESCE(acad.courses.course_level, EXCLUDED.course_level);")
    lines.append("")

    # 3) Categories
    lines.append("-- 3) Curriculum categories (per-cohort prefixed under program BBS)")
    all_cats = sorted(k12_cats + k13_cats, key=lambda c: c["sort_order"])
    # re-number sort_order so K12 rows come before K13
    for idx, cat in enumerate(all_cats, start=1):
        cat["sort_order"] = idx
    lines.append(
        "INSERT INTO acad.curriculum_categories (program_id, category_name, min_credits, sort_order)"
    )
    lines.append("SELECT p.program_id, v.cat, v.mc, v.ord FROM acad.programs p")
    lines.append("CROSS JOIN (VALUES")
    cat_rows = []
    for cat in all_cats:
        mc = cat["min_credits"]
        cat_rows.append(
            f"  ({esc(cat['name'])}, {'NULL' if mc is None else mc}, {cat['sort_order']})"
        )
    lines.append(",\n".join(cat_rows))
    lines.append(") AS v(cat, mc, ord)")
    lines.append("WHERE p.program_code = 'BBS'")
    lines.append("ON CONFLICT (program_id, category_name) DO NOTHING;")
    lines.append("")

    # 4) Requirements — wipe K12/K13 course requirements first so that reruns
    # (and updates to is_required / prereq_rule) replace the old rows cleanly.
    lines.append("-- 4) Curriculum requirements per cohort")
    lines.append(
        "DELETE FROM acad.curriculum_requirements cr "
        "USING acad.cohorts co, acad.programs p "
        "WHERE cr.cohort_id = co.cohort_id AND co.program_id = p.program_id "
        "AND p.program_code = 'BBS' AND co.cohort_code IN ('K12','K13') "
        "AND cr.kind = 'course';"
    )
    lines.append("INSERT INTO acad.curriculum_requirements")
    lines.append("  (cohort_id, category_id, kind, course_code, prereq_rule, is_required, note)")
    lines.append("SELECT co.cohort_id, cat.category_id, 'course', v.code, v.rule::jsonb, v.req, v.note")
    lines.append("FROM acad.programs p")
    lines.append("JOIN acad.cohorts co ON co.program_id = p.program_id")
    lines.append("JOIN acad.curriculum_categories cat ON cat.program_id = p.program_id")
    lines.append("JOIN (VALUES")
    req_rows: List[str] = []
    for req in k12_reqs + k13_reqs:
        cohort = req["cohort"]
        cat_name = req["category_name"]
        code = req["course_code"]
        rule = req["prereq_rule"]
        rule_sql = esc(json.dumps(rule, ensure_ascii=False)) if rule is not None else "NULL"
        note_bits = []
        if req.get("original_code") and req["original_code"] != code:
            note_bits.append(f"orig={req['original_code']}")
        if req.get("ielts"):
            note_bits.append(f"ielts={req['ielts']}")
        note_val = "; ".join(note_bits) if note_bits else None
        # prefer the per-row is_required flag set by the parser; fall back to
        # category-name heuristics for the "Electives 300-400" / concentration
        # sub-group buckets that don't have an explicit Electives marker row.
        is_required = req.get("is_required", True)
        lower = cat_name.lower()
        if "electives" in lower or "concentration -" in lower:
            is_required = False
        req_rows.append(
            f"  ({esc(cohort)}, {esc(cat_name)}, {esc(code)}, "
            f"{rule_sql}, {'true' if is_required else 'false'}, {esc(note_val)})"
        )
    lines.append(",\n".join(req_rows))
    lines.append(") AS v(cohort, cat_name, code, rule, req, note)")
    lines.append("  ON co.cohort_code = v.cohort AND cat.category_name = v.cat_name")
    lines.append("WHERE p.program_code = 'BBS'")
    lines.append("ON CONFLICT (cohort_id, course_code)")
    lines.append("  WHERE kind = 'course' AND effective_term_from IS NULL AND effective_term_to IS NULL")
    lines.append("DO NOTHING;")
    lines.append("")

    lines.append("COMMIT;")
    lines.append("")

    OUT_PATH.write_text("\n".join(lines), encoding="utf-8")

    total_courses = len(all_courses)
    total_reqs = len(k12_reqs) + len(k13_reqs)
    print(
        f"Wrote {OUT_PATH} "
        f"(courses={total_courses}, categories={len(all_cats)}, requirements={total_reqs})"
    )


if __name__ == "__main__":
    main()
