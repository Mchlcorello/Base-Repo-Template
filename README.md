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

Phases landed so far: 0 (skeleton), 1 (API shell), 2 (Postgres via docker compose). See `.claude/plans/base-repo-implementation.md` for the roadmap.
