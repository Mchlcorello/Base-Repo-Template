# Base Repo Implementation Plan

Plan for implementing the base repository defined in `base-repo-architecture.md`.

The goal is a clone-and-go starter: ASP.NET Core Minimal API + PostgreSQL + React (Vite/TS) + OpenTelemetry + Prometheus + Grafana, wired together via Docker Compose, with a minimal but production-minded set of conventions in place.

---

## Phase 0 — Repo Skeleton

Lay down the top-level structure before any code. Doing this first keeps later phases from drifting on naming.

**Files / folders to create:**

- `README.md` — quickstart only (filled in incrementally as phases land).
- `.gitignore` — covers `bin/`, `obj/`, `node_modules/`, `.env`, `dist/`, Rider/VS user files, `*.user`.
- `.editorconfig` — 4 spaces C#, 2 spaces TS/JSON/YAML, LF line endings, UTF-8, trim trailing whitespace.
- `.env.example` — placeholder for connection strings and any local-only config.
- `docker-compose.yml` — empty stub (`services:` placeholder), filled in by later phases.
- `src/api/`, `src/web/`, `tests/api/`, `tests/web/`, `observability/`, `scripts/`, `docs/` — directory stubs (with `.gitkeep` where empty).

**Done when:** structure matches the tree in `base-repo-architecture.md` and `git status` shows a clean tracked skeleton.

---

## Phase 1 — ASP.NET Core Minimal API

Build the API shell. No EF, no observability yet — just a runnable service with health and info endpoints.

**Steps:**

1. `dotnet new sln -n Base` at repo root. Pin SDK with a `global.json` targeting .NET 10.
2. `dotnet new web -n Base.Api -o src/api/Base.Api -f net10.0` (Minimal API template).
3. Add `Base.Api` to the solution.
4. Configure `appsettings.json` + `appsettings.Development.json` with placeholder `ConnectionStrings:AppDb` and `Observability:ServiceName`. Configure Kestrel to bind plain HTTP on `http://localhost:5000` and remove the HTTPS redirect — the local dev loop never needs TLS, and Phase 6's Prometheus scrape targets this port.
5. Create feature-first folder scaffolding under `src/api/Base.Api/`:
   - `Features/` (empty, with `.gitkeep`)
   - `Infrastructure/DependencyInjection.cs` — single `AddAppServices(this IServiceCollection)` extension that future phases compose into.
   - `Contracts/`, `Common/`, `Middleware/`, `Auth/`, `Health/`, `Observability/`, `Data/` — all stubbed.
6. In `Program.cs`:
   - Add CORS policy `LocalReact` allowing `http://localhost:5173`.
   - Add Swagger/OpenAPI via `Microsoft.AspNetCore.OpenApi` + `Swashbuckle.AspNetCore`.
   - Add global exception handler returning `ProblemDetails`.
   - Map `GET /api/info` returning service name + version + environment.
7. Verify with `dotnet run --project src/api/Base.Api` → hit `/swagger` and `/api/info`.

**Done when:** API starts cleanly, Swagger renders, `/api/info` returns 200 with JSON payload.

---

## Phase 2 — PostgreSQL via Docker Compose

Stand up Postgres locally before wiring EF Core, so connection strings can be validated against a real instance.

**Steps:**

1. Add `postgres` service to `docker-compose.yml` per architecture doc (Postgres 17, `base_app`/`postgres`/`postgres`, port 5432, named volume).
2. Put the connection string in `.env.example`:
   `ConnectionStrings__AppDb=Host=localhost;Port=5432;Database=base_app;Username=postgres;Password=postgres`
3. Document `docker compose up postgres` in `README.md`.
4. Add `scripts/reset-db.ps1` — drops and recreates the volume (with a confirmation prompt).

> **Env loading:** `.env` is consumed by `docker compose` only. The API process itself reads `appsettings.Development.json` and `dotnet user-secrets` — do not add a .NET package to load `.env` files.

**Done when:** `docker compose up postgres` works and a client (psql / DataGrip) can connect.

---

## Phase 3 — EF Core + Migrations

Add the data layer. Keep entities minimal — a `User` entity is enough to exercise the migration pipeline.

**Steps:**

1. Add NuGet packages to `Base.Api`:
   - `Microsoft.EntityFrameworkCore`
   - `Microsoft.EntityFrameworkCore.Design`
   - `Npgsql.EntityFrameworkCore.PostgreSQL`
2. Create `Data/AppDbContext.cs` with a `DbSet<User>`. Define `User` in `Features/Users/User.cs` (Id, Email, CreatedAt).
3. Move entity configuration to `Data/EntityConfigurations/UserConfiguration.cs` (IEntityTypeConfiguration pattern).
4. Wire `AddDbContext<AppDbContext>(options => options.UseNpgsql(...))` in `Infrastructure/DependencyInjection.cs`.
5. Create the first migration: `dotnet ef migrations add InitialCreate -p src/api/Base.Api -o Data/Migrations`.
6. Add `scripts/migrate.ps1` — runs `dotnet ef database update`.
7. Add a development-only seed hook (`Data/DevelopmentSeeder.cs`) invoked from `Program.cs` only when `app.Environment.IsDevelopment()`.

**Done when:** `migrate.sh` applies migrations against the docker postgres and the seed inserts at least one row.

---

## Phase 4 — Health Checks

Adds `/health` and a database health check before observability so the readiness signal is real.

**Steps:**

1. Add `AspNetCore.HealthChecks.NpgSql`.
2. Register checks in `Health/HealthCheckExtensions.cs`: liveness (self) + readiness (Npgsql).
3. Map `app.MapHealthChecks("/health")` and `app.MapHealthChecks("/health/ready", ...)` with a tag filter.
4. Confirm both endpoints respond 200 when Postgres is up, 503 when it is not.

**Done when:** stopping the postgres container flips `/health/ready` to 503 within ~15 seconds.

---

## Phase 5 — OpenTelemetry + Prometheus Exporter

Wire the metrics pipeline. No tracing yet — architecture doc explicitly scopes to metrics.

**Steps:**

1. Add NuGet packages:
   - `OpenTelemetry.Extensions.Hosting`
   - `OpenTelemetry.Instrumentation.AspNetCore`
   - `OpenTelemetry.Instrumentation.Http`
   - `OpenTelemetry.Instrumentation.Runtime`
   - `OpenTelemetry.Instrumentation.Process`
   - `OpenTelemetry.Exporter.Prometheus.AspNetCore`
2. Create `Observability/ObservabilityExtensions.cs` per architecture doc — `AddAppObservability` + `MapAppObservability`.
3. Create `Observability/Metrics.cs` (custom `Meter` placeholder, empty for now) and `Observability/ActivitySources.cs` (placeholder for later tracing work).
4. Wire into `Program.cs`: `builder.Services.AddAppObservability(builder.Configuration);` then `app.MapAppObservability();`.
5. Hit `/metrics` — confirm Prometheus exposition format and that `http_server_request_duration_seconds` shows up after a request to `/api/info`.

**Done when:** `/metrics` returns text/plain with ASP.NET Core, runtime, and process metrics.

---

## Phase 6 — Prometheus Service

Now scrape the API.

**Steps:**

1. Create `observability/prometheus/prometheus.yml` per architecture doc.
2. Use `host.docker.internal:5000` as the initial target since the API is not containerized yet. Configure Kestrel to bind plain HTTP on `:5000` and disable the HTTPS redirect locally so the scrape works without certs.
3. Add the `prometheus` service to `docker-compose.yml`.
4. Bring up the stack, browse `http://localhost:9090` → Status → Targets → confirm `base-api` is `UP`.

**Done when:** Prometheus shows the API as a healthy scrape target and can graph `http_server_request_duration_seconds`.

---

## Phase 7 — Grafana + Starter Dashboard

Visualize what Prometheus is collecting.

**Steps:**

1. Add `grafana` service to `docker-compose.yml` (port 3001, admin/admin, provisioning volumes per architecture doc).
2. Create `observability/grafana/provisioning/datasources/datasource.yml` pointing at `http://prometheus:9090`.
3. Create `observability/grafana/provisioning/dashboards/dashboards.yml` with the file provider.
4. Create `observability/grafana/dashboards/api-overview.json` with starter panels:
   - Request rate (`rate(http_server_request_duration_seconds_count[1m])`)
   - Error rate (status code `5xx` ratio)
   - p50 / p95 / p99 latency (`histogram_quantile` over the bucket metric)
   - Requests by endpoint
   - Requests by status code
   - CPU / memory / GC (from runtime instrumentation)
5. Bring up the stack, log in to Grafana at `http://localhost:3001`, confirm dashboard auto-loads.

**Done when:** the `API Overview` dashboard renders live data from Prometheus on first boot of a fresh Grafana volume.

---

## Phase 8 — React + Vite + TypeScript Frontend

Build the web app shell. Wire it to the API last so CORS is exercised end-to-end.

**Steps:**

1. `npm create vite@latest web -- --template react-ts` inside `src/`.
2. Set up the feature-first folder structure from the architecture doc (`app/`, `pages/`, `features/`, `shared/`).
3. Install deps:
   - `@tanstack/react-query` + devtools
   - `react-router-dom`
   - `react-hook-form`
   - `zod`
   - `vitest`, `@testing-library/react`, `@testing-library/jest-dom`, `jsdom`
4. Create `shared/api/client.ts` with a small `fetch` wrapper that reads `VITE_API_URL` from env.
5. Create `pages/HomePage.tsx` that calls `GET /api/info` via TanStack Query and displays the result.
6. Add `app/router.tsx`, `app/providers.tsx` (QueryClientProvider), `app/config.ts`.
7. Add a smoke test under `tests/web/` that renders `HomePage` with a mocked client.
8. Document `npm run dev` in `README.md` (default port 5173, matches the CORS policy from Phase 1).

**Done when:** `npm run dev` boots the SPA and the home page shows the live `/api/info` payload from the running API.

---

## Phase 9 — Tests

Minimal test projects so future contributions have a place to land tests.

**Steps:**

1. `dotnet new xunit -n Base.Api.Tests -o tests/api/Base.Api.Tests`, add to solution, reference `Base.Api`.
2. Add `Microsoft.AspNetCore.Mvc.Testing` + write one integration test against `/api/info` using `WebApplicationFactory`.
3. Confirm Vitest setup from Phase 8 runs via `npm test`.

**Done when:** both test suites run green via a single command (`dotnet test` and `npm test`).

---

## Phase 10 — Developer Scripts + README

Tie everything together so a fresh clone is genuinely one-command-up.

**Steps:**

1. `scripts/dev.ps1` — brings up the compose stack, applies migrations, prints next-step URLs (API, Swagger, Prometheus, Grafana, web).
2. Flesh out `README.md`:
   - Prereqs (Docker, .NET 10 SDK, `dotnet-ef` installed globally via `dotnet tool install -g dotnet-ef`, Node 20+, PowerShell 7+)
   - Quickstart (`./scripts/dev.ps1`)
   - Where things live (link to `docs/architecture.md` and `docs/conventions.md`)
   - Port map
3. Move `base-repo-architecture.md` → `docs/architecture.md`.
4. Add `docs/conventions.md` distilling the conventions sections from the architecture doc (API conventions, React conventions, DB practices).

**Done when:** a fresh clone followed by `./scripts/dev.ps1` produces a fully working stack with no manual extra steps.

---

## Out of Scope (per architecture doc)

Explicitly **not** included in this plan — defer until a real project asks for them:

- Loki / Tempo / Datadog / New Relic / Sentry
- RabbitMQ / Kafka / Redis
- Kubernetes manifests, Helm charts
- Auth implementation beyond stubbed endpoints (the architecture doc lists `POST /api/auth/login|logout|refresh` but does not specify a scheme — leave as 501 stubs until a project picks JWT vs cookies vs OIDC)
- Generic repository abstraction over EF Core
- Multi-project Clean Architecture layering

---

## Implementation Order Summary

```
0.  Repo skeleton
1.  ASP.NET Minimal API
2.  Postgres via docker-compose
3.  EF Core + migrations
4.  Health checks
5.  OpenTelemetry + /metrics
6.  Prometheus service
7.  Grafana + starter dashboard
8.  React + Vite + TS
9.  Tests
10. Dev scripts + README
```

Each phase is independently shippable — if a clone is forked partway through, the stack still runs to the most recent completed phase.
