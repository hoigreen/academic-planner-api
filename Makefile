.PHONY: db-up db-down db-reset db-wait db-logs db-shell db-seed app-up app-down

# Start only PostgreSQL with schema + seed scripts.
db-up:
	docker compose up -d postgres
	$(MAKE) db-wait

# Stop PostgreSQL container (keeps data volume).
db-down:
	docker compose stop postgres

# Recreate PostgreSQL from scratch and re-run all init SQL scripts.
db-reset:
	docker compose down -v
	docker compose up -d postgres
	$(MAKE) db-wait

# Wait until PostgreSQL accepts connections.
db-wait:
	docker compose exec -T postgres sh -c "until pg_isready -U postgres -d academic_planner >/dev/null 2>&1; do sleep 1; done"

# Follow PostgreSQL logs.
db-logs:
	docker compose logs -f postgres

# Open psql inside PostgreSQL container.
db-shell:
	docker compose exec postgres psql -U postgres -d academic_planner

# Re-apply seed data without deleting schema.
db-seed:
	$(MAKE) db-wait
	docker compose exec -T postgres psql -U postgres -d academic_planner -f /docker-entrypoint-initdb.d/20_seed_bbs.sql
	docker compose exec -T postgres psql -U postgres -d academic_planner -f /docker-entrypoint-initdb.d/50_seed_bbs_k12_k13_curriculum.sql
	docker compose exec -T postgres psql -U postgres -d academic_planner -f /docker-entrypoint-initdb.d/60_seed_fake_data.sql
	docker compose exec -T postgres psql -U postgres -d academic_planner -f /docker-entrypoint-initdb.d/zz_sync_curricula.sql

# Start full stack (Postgres + Keycloak + API).
app-up:
	docker compose up -d --build

# Stop full stack.
app-down:
	docker compose down
