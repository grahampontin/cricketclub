# AGENTS.md — Coding Agent Guide for cricketclub

## Overview

This document serves as a comprehensive guide for coding agents working on the cricket club management system. It covers architecture, conventions, key patterns, and critical files to ensure consistency and maintainability across the codebase.

The system exposes a RESTful API which is consumed by a React frontend (not included in this codebase). The front end relies on the swagger.json file for API contract, so any changes to API endpoints or DTOs must be reflected in the swagger.json (auto-generated on build) and committed to source control.

A development database is available with connection string `Server=localhost;Database=thevilla_admin;Trusted_Connection=True;` (SQL Server). The database schema includes tables for matches, players, venues, teams, and related entities. Tests using this database should be annotated with '[Category("RequiresDatabase")]' to exclude them from CI pipelines that do not have access to the database.

Compilation should always succeed before completing a task and all unit tests must pass. Code should adhere to the conventions outlined in this document, and any significant architectural changes should be discussed with the team before implementation.

## Database & SQL conventions

- **Prefer C# over SQL stored procedures** for business logic. Keep computation in the application layer (e.g. `CricketClubMiddle`) so it is readable, testable, and version-controlled alongside the code.
- SQL in `DAO.cs` should be limited to: plain SELECT/INSERT/UPDATE/DELETE, simple JOINs, and aggregations needed for efficient bulk reads. Avoid triggers, computed columns, and stored procedures.
- When adding new DB columns or tables, create a numbered migration SQL file under `database/migrations/` (e.g. `003_my_feature.sql`). Include idempotency guards (`IF NOT EXISTS`).
- One-off/manual SQL cleanup scripts that are **not** schema migrations should go under `scripts/` (for example `scripts/cleanup-test-teams.sql`), not under `database/migrations/`.
- **Tests annotated `[Category("RequiresDatabase")]`** use the dev SQL Server (`Server=localhost;Database=thevilla_admin;Trusted_Connection=True;`). All other tests must work without a database (use Moq for `IDao`).

## File placement & solution item conventions

- **DB schema/data migrations** always go in `database/migrations/` and should use the next numeric prefix (`004_...`, `005_...`, etc.).
- **Spec / design / handoff documents** for agents or frontend/backend implementation notes should go in `docs/`.
- **Operational helper scripts** (cleanup scripts, maintenance scripts, ad-hoc SQL, PowerShell helpers) should go in `scripts/`.
- When you add a new file in any of these categories, also add it to `CricketClub.sln` as a **Solution Item** so it is visible in the IDE:
  - add new spec/docs files under the `Docs` solution folder
  - add new migration SQL files under the `SQL` solution folder
  - add non-migration SQL helper scripts under the `SQL` solution folder as well
- Do not leave newly created `.md` or `.sql` files unlisted in the solution. If you create one, update the corresponding `ProjectSection(SolutionItems)` entry in `CricketClub.sln` in the same change.

## Architecture Overview

A layered .NET 9 cricket club management system:

```
CricketClub.WebApi (ASP.NET Core 9)
    └── CricketClubMiddle   ← business logic, domain objects (Match, Player, Venue, Team)
        └── CricketClubDAL  ← data access: IDao / Dao (SQL Server via Db helper)
            └── CricketClubDomain  ← plain data-transfer objects (MatchData, VenueData, …)
```

- **Controllers** call `CricketClubMiddle` classes (e.g. `Match`, `Player`, `Venue`).
- **`CricketClubMiddle`** reads/writes via `IDao` and caches aggressively in the global singleton `InternalCache`.
- **`CricketClubDAL.Dao`** executes raw SQL against SQL Server (`thevilla_admin` / `dbo` schemas).
- **`CricketClub.WebApi/Domain/`** contains versioned API DTOs (`MatchV1`, `VenueV1`, `ResultV1`, …) with `static FromInternal(…)` factory methods that convert from `CricketClubMiddle` types.

### ⚠️ Legacy Architecture Warning — N+1 and Static Constructors

The `CricketClubMiddle` / `CricketClubDAL` interaction is **legacy and prone to serious performance problems**, particularly N+1 query patterns. Key symptoms to watch for:

- **Static or implicit constructors** in `CricketClubMiddle` domain objects (e.g. `Match`, `Venue`, `Player`) that lazily load data per-instance trigger one DB round-trip per object when iterating a list — classic N+1.
- Methods like `GetIsBallByBallInProgress(matchId)`, `GetCurrentBallByBallState()`, or any `IDao` call inside a `.Select(...)` / `.Where(...)` LINQ chain over a collection of domain objects will silently fan out into N queries.
- `InternalCache` patching (adding a 30s TTL cache around a per-item DAO call) **only helps repeat loads** — first-time callers still pay the full N+1 cost.

**When making any change that touches the DAO/Middle boundary, agents must actively consider whether a bulk DAO method exists (or should be added) to replace per-item calls.** The preferred pattern is:

1. **Add a bulk `IDao` method** (e.g. `HashSet<int> GetMatchIdsWithBallByBallCoverage(IEnumerable<int> matchIds)`) that returns all needed data in a single query.
2. **Call it once** at the top of the controller action or service method, then filter/join in memory.
3. **Avoid** reaching back into `IDao` inside any loop, LINQ projection, or per-item method call on a collection of domain objects.

Each time you work on a feature that reads a list of entities, treat it as an opportunity to audit and fix any N+1 patterns in the surrounding code, even if they are not the primary focus of the change.

## CI / CD

The CI pipeline is defined in `.github/workflows/ci-build-push.yml` (GitHub Actions). On every push to `master` it:
1. Restores, builds, and runs all non-database tests.
2. Builds the Docker image with `GIT_HASH=${{ github.sha }}` baked in as a build arg.
3. Pushes two tags to GHCR: `latest` and the full commit SHA (`ghcr.io/grahampontin/thevillagecc-api:<sha>`).

**Do not create additional workflow files.** All CI changes should be made to the existing `ci-build-push.yml`.

## Build & Run

```powershell
# One-time: restore local .NET tools (Swashbuckle CLI for swagger.json generation)
dotnet tool restore

# Build – also regenerates CricketClub.WebApi/swagger.json
dotnet build

# Run API (http://localhost:5000, https://localhost:5001, /swagger in Dev)
cd CricketClub.WebApi; dotnet run

# Run all tests
dotnet test

# Run only non-database tests (safe for CI without SQL Server)
dotnet test --filter "Category!=RequiresDatabase"
```

**Agents must always run `dotnet build` and `dotnet test --filter "Category!=RequiresDatabase"` before completing any task. Both must succeed with zero errors and zero non-RequiresDatabase test failures.**


## C# Conventions

- **Nullable enabled** in all production projects — never add `#nullable disable`.
- Naming: `camelCase` private fields, `PascalCase` properties/methods, `camelCase` locals/parameters.
- All controller actions require explicit ASP.NET Core annotations: `[HttpGet]`, `[HttpPost]`, `[ProducesResponseType]`, etc.
- All routes use `[Route("api/[controller]")]`; no legacy `/handler` style paths.

## Test Conventions (critical)

`CricketClubMiddle.InternalCache` is a **process-wide singleton**. Failure to reset it causes cross-test pollution.

**Every test constructor/fixture must:**
```csharp
TestDefaults.ResetInternalCache();   // always
TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);  // when MatchV1/VenueV1 mapping occurs
```

- `TestDefaults` lives in `Tests/CricketClub.WebApi.Tests/Utils/TestDefaults.cs`.
- `SetupSafeVenueAndTeamLookups` ensures `VenueData.Coordinates` is non-null and pre-populates team cache entries, preventing NullReferenceExceptions in `VenueV1.FromInternal` and `Team.OurTeam`.
- Controller unit tests call action methods directly and assert on `OkObjectResult`, `CreatedAtActionResult`, etc.
- Integration tests use `WebApplicationFactory<Program>` with `.WithDao(mockDao.Object)` to override `IDao` in DI.
- Test frameworks: **xUnit** + **Moq** (WebApi.Tests), **NUnit** + **Moq** (CricketClub.Tests).

## Key Patterns

### DTO Mapping
API DTOs in `CricketClub.WebApi/Domain/` expose a `static FromInternal(InternalType x)` factory:
```csharp
VenueV1.FromInternal(venue)   // venue.Coordinates must be non-null
MatchV1.FromInternal(match)   // calls VenueV1.FromInternal internally
ResultV1.FromInternal(match, matchReport)
```
Enum conversion goes through `EnumMappers` (`ToV1` / `ToInternal` / `ParseMatchType`).

### InternalCache
`InternalCache.GetInstance()` is used throughout `CricketClubMiddle` to cache expensive DB reads (matches, venues, stats). Cache keys are string-based (e.g. `"VenueMatchData_" + id`, `"team0"`). Never assume a fresh cache in tests.

### Logging
Log4net throughout. Never log secrets or connection string credentials.

## Key Files

| Path | Purpose |
|------|---------|
| `.github/workflows/ci-build-push.yml` | GitHub Actions CI: build, test, push to GHCR |
| `CricketClub.WebApi/Program.cs` | DI setup, middleware, CORS, Swagger |
| `CricketClub.WebApi/Domain/` | All V1 DTOs and mappers |
| `CricketClubMiddle/InternalCache.cs` | Process-wide cache singleton |
| `CricketClubDAL/CricketClubDAL/IDao.cs` | Full data-access contract |
| `Tests/CricketClub.WebApi.Tests/Utils/TestDefaults.cs` | Shared test bootstrap helpers |
| `CricketClub.WebApi/swagger.json` | Auto-generated on build; commit changes |
| `database/migrations/` | Numbered SQL schema/data migrations |
| `docs/` | Specs, design notes, and agent handoff docs |
| `scripts/` | One-off maintenance and helper scripts |

