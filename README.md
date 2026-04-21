# Academic Planner API (Roadmap/Audit/Recommendation)

## Team Setup (Start From Scratch)

This setup gives every teammate:

- their own local Docker PostgreSQL database
- the same schema and seed data
- repeatable reset commands when data gets out of sync

### Prerequisites

- Docker + Docker Compose
- `make`

### First-time onboarding

```bash
git clone <repo-url>
cd academic-planner/academic-planner-api
make db-reset
```

After `make db-reset`, your local DB is rebuilt from scratch and all SQL init scripts in `db/` are applied.

If `5433` is already occupied on your machine, override the published Postgres port for that shell:

```bash
POSTGRES_HOST_PORT=5434 make db-reset
```

### Run full backend stack

```bash
make app-up
```

Endpoints:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Keycloak Admin: `http://localhost:8180` (admin/admin)
- Health:
  - `GET /health/live`
  - `GET /health/ready`

### Daily commands

- `make db-up`: start PostgreSQL only
- `make db-down`: stop PostgreSQL (keep data)
- `make db-reset`: rebuild PostgreSQL and re-run all SQL scripts in `db/`
- `make db-wait`: wait until PostgreSQL is ready
- `make db-seed`: re-apply BBS + PM + student seed scripts and sync curricula
- `make db-logs`: stream PostgreSQL logs
- `make db-shell`: open `psql` in container
- `make app-up`: start full stack (Postgres + Keycloak + API)
- `make app-down`: stop full stack

### If your local data is broken

Run:

```bash
make db-reset
```

This is the team standard way to get back to a clean, shared baseline dataset.

## Database

Init scripts are in `db/` (executed in filename order on fresh DB):

- `00_schema.sql`: core ORDBMS schema (`acad`) with domains, enums, JSONB prereqs
- `01_keycloak_db.sql`: creates `keycloak_db` for identity server
- `10_extensions.sql`: concentrations/offerings/advisories extensions
- `20_seed_bbs.sql`: representative BBS seed
- `30_curricula.sql`: `knowledge_block` composite type + `curricula` table (ORDBMS array of composite types + JSONB course mapping)
- `40_seed_bbs_students.sql`: legacy bulk BBS student seed kept for reference; no longer part of the default init flow
- `60_seed_fake_data.sql`: curated benchmark seed with a small but diverse student mix across programs/cohorts
- `seed_data.sql`: legacy demo seed kept for reference, no longer part of the default init flow
- `99_sync_curricula.sql`: syncs `acad.curricula` from seeded requirements

`seed_data_bbs.sql` is kept for reference and is not part of the default Docker init flow.

## Authentication

JWT Bearer authentication via Keycloak with RBAC:

- **Roles:** `CVHT` (Advisor), `SV` (Student), `Admin` (Training Admin)
- **Policies:** `RequireAdvisor`, `RequireStudent`, `RequireAdmin`
- If `Keycloak:Authority` is empty, auth is bypassed (dev mode)

### Keycloak Setup

1. Start services: `docker compose up --build`
2. Open `http://localhost:8180`, login as `admin`/`admin`
3. Create realm `academic-planner`
4. Create client `academic-planner` (confidential, Standard flow)
5. Create roles: `CVHT`, `SV`, `Admin`
6. Assign roles to users

## Main API Groups

- `/api/v1/programs` — programs, cohorts, curriculum overview
- `/api/v1/courses` — course catalog, prerequisites, equivalencies
- `/api/v1/students` — student profiles, transcripts, latest attempts
- `/api/v1/students/{studentId}/audit/*` — full audit, summary, missing courses, eligibility, progress by category
- `/api/v1/students/{studentId}/recommendations/*` — next-term heuristic AI recommendations
- `/api/v1/students/{studentId}/plans/*` — CRUD for student term plans + validation
- `/api/v1/curriculum/{programCode}/{cohortCode}` — curriculum structure with knowledge blocks and course mapping
- `/api/v1/curriculum/students/{studentId}/eligible-courses` — eligible courses (prereqs checked)
- `/api/v1/curriculum/students/search` — paginated student search
- `/api/v1/programs/{programCode}/concentrations`
- `/api/v1/students/{studentId}/concentration`
- `/api/v1/admin/*`

## Architecture

- **Backend:** .NET 8 modular monolith
- **Database:** PostgreSQL 16 with ORDBMS features (composite types, JSONB, course_code[] arrays, GIN indexes)
- **Auth:** Keycloak 24 (JWT Bearer, realm roles mapped to .NET ClaimTypes.Role)
- **Services:**
  - `PrerequisiteEvaluator` — Parses JSONB prereq rules (none, course, english, and, or)
  - `StudentAuditService` — Audit snapshot, progress by category, eligibility
  - `RoadmapRecommendationService` — Heuristic AI ranking (required core > concentration > elective)
  - `PlanValidationService` — Credit limits, stage requirements, capstone checks
- Error format: ASP.NET ProblemDetails for failures, `ApiEnvelope<T>` for success
