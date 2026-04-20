# Academic Planner API (Roadmap/Audit/Recommendation)

## Run with Docker

```bash
docker compose up --build
```

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Keycloak Admin: `http://localhost:8180` (admin/admin)
- Health:
  - `GET /health/live`
  - `GET /health/ready`

## Database

Init scripts are in `db/`:

- `schema.sql`: core ORDBMS schema (`acad`) with domains, enums, JSONB prereqs
- `seed_data.sql`: demo seed
- `10_extensions.sql`: concentrations/offerings/advisories extensions
- `20_seed_bbs.sql`: representative BBS seed
- `30_curricula.sql`: `knowledge_block` composite type + `curricula` table (ORDBMS array of composite types + JSONB course mapping)

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
