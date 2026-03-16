# Academic Planner API (Roadmap/Audit/Recommendation)

## Run with Docker

```bash
docker compose up --build
```

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Health:
  - `GET /health/live`
  - `GET /health/ready`

## Database

Init scripts are in `db/`:

- `schema.sql`: core ORDBMS schema (`acad`)
- `seed_data.sql`: demo seed
- `10_extensions.sql`: concentrations/offerings/advisories extensions
- `20_seed_bbs.sql`: representative BBS seed

## Main API groups

- `/api/v1/programs`
- `/api/v1/courses`
- `/api/v1/students`
- `/api/v1/students/{studentId}/audit/*`
- `/api/v1/students/{studentId}/recommendations/*`
- `/api/v1/students/{studentId}/plans/*`
- `/api/v1/programs/{programCode}/concentrations`
- `/api/v1/students/{studentId}/concentration`
- `/api/v1/admin/*`

## Notes

- This version intentionally has no authentication/authorization.
- Error format uses ASP.NET ProblemDetails for failures and envelope response for successful calls.
