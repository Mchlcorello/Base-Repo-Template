# Base Repo Template

Clone-and-go starter: ASP.NET Core Minimal API + PostgreSQL + React (Vite/TS) + OpenTelemetry + Prometheus + Grafana, wired together via Docker Compose.

## Quickstart

> Phase 10 will replace this section with a one-command `./scripts/dev.ps1` flow. Until then, run each component manually.

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- Node.js 20+ (added in Phase 8)
- PowerShell 7+

### Start Postgres

```powershell
docker compose up postgres -d
```

Connects on `localhost:5432` with database `base_app`, user `postgres`, password `postgres`. The connection string matches `appsettings.Development.json` out of the box. To wipe and recreate the volume:

```powershell
./scripts/reset-db.ps1
```

### Run the API

```powershell
dotnet run --project src/api/Base.Api
```

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Info: http://localhost:5000/api/info
- Metrics: http://localhost:5000/metrics
- Health: http://localhost:5000/health (and `/health/ready`)

### Start Prometheus

```powershell
docker compose up prometheus -d
```

- Prometheus UI: http://localhost:9090
- Scrapes the API at `host.docker.internal:5000/metrics` every 15s. The API must be running on the host (it isn't containerized yet).
- Confirm the target is healthy at http://localhost:9090/targets.

### Start Grafana

```powershell
docker compose up grafana -d
```

- Grafana UI: http://localhost:3001 (admin / admin)
- The `Prometheus` datasource and `API Overview` dashboard are provisioned on first boot — no manual setup required.
- Dashboard URL: http://localhost:3001/d/base-api-overview/api-overview

## Layout

```
src/api/         ASP.NET Core Minimal API (Base.Api)
src/web/         React + Vite frontend (Phase 8)
tests/api/       API test projects
tests/web/       Frontend test projects
observability/   Prometheus + Grafana config
scripts/         Developer scripts
docs/            Architecture + conventions
```

## Status

Phases landed so far: 0 (skeleton), 1 (API shell), 2 (Postgres via docker compose), 3 (EF Core + migrations + Item entity), 4 (health checks), 5 (OpenTelemetry + /metrics), 6 (Prometheus scrape), 7 (Grafana + API Overview dashboard). See `.claude/plans/base-repo-implementation.md` for the roadmap.
