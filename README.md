# Base Repo Template

Clone-and-go starter: ASP.NET Core Minimal API + PostgreSQL + React (Vite/TS) + OpenTelemetry + Prometheus + Grafana, wired together via Docker Compose.

## Quickstart

### Prerequisites

- Docker Desktop
- (Optional, for editor support / running outside containers) .NET 10 SDK, Node.js 20+, PowerShell 7+

### One command

```powershell
docker compose up
```

That's the whole stack — Postgres, API (with hot reload via `dotnet watch`), web (Vite dev), Prometheus, Grafana. Add `-d` to detach. Migrations run automatically on API start in Development; the dev seeder inserts a sample `Item` row.

| Service     | URL                                                              |
|-------------|------------------------------------------------------------------|
| Web (SPA)   | http://localhost:5173                                            |
| API         | http://localhost:5000 (`/api/info`, `/swagger`, `/health`, `/metrics`) |
| Prometheus  | http://localhost:9090 (target `base-api` scrapes `api:5000`)     |
| Grafana     | http://localhost:3001 (admin / admin) — `API Overview` dashboard |
| Postgres    | localhost:5432 (db `base_app`, user/pass `postgres`/`postgres`)  |

### Reset the database

```powershell
./scripts/reset-db.ps1
```

Stops the postgres container, drops the volume, brings it back up. The API will re-migrate on its next start.

### Run the API or web outside the container

The full stack works out of the box. If you'd rather run the API or web on the host (e.g. for richer debugger integration):

```powershell
# API on host (requires .NET 10 SDK)
docker compose up postgres -d
dotnet run --project src/api/Base.Api

# Web on host (requires Node 20+)
docker compose up postgres api -d
cd src/web
npm install
npm run dev
```

`appsettings.Development.json` and `src/web/src/app/config.ts` default to `localhost` URLs that work in both modes.

### Tests

```powershell
dotnet test                # API tests (Phase 9)
npm --workspace web test   # web tests (Vitest)
```

## Layout

```
src/api/         ASP.NET Core Minimal API (Base.Api)
src/web/         React + Vite frontend
tests/api/       API test projects
tests/web/       Frontend test projects
observability/   Prometheus + Grafana config
scripts/         Developer scripts
docs/            Architecture + conventions
```

## Status

Phases landed so far: 0 (skeleton), 1 (API shell), 2 (Postgres via docker compose), 3 (EF Core + migrations + Item entity), 4 (health checks), 5 (OpenTelemetry + /metrics), 6 (Prometheus scrape), 7 (Grafana + API Overview dashboard), 8 (React + Vite + TS web app). See `.claude/plans/base-repo-implementation.md` for the roadmap.
