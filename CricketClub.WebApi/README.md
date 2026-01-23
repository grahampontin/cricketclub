# TheVillageCC.WebApi

ASP.NET Core 9 Web API for The Village Cricket Club management system. This is a migration from the legacy .NET Framework 4.8 HTTP handlers to modern .NET 9 Web API controllers.

## Project Structure

```
TheVillageCC.WebApi/
├── Controllers/          # API controllers (converted from HTTP handlers)
│   ├── ControllerBase.cs            # Base controller with IHandlerContext support
│   ├── EntityControllerBase.cs      # Base for CRUD controllers
│   ├── AwardsController.cs
│   ├── CommitteeController.cs
│   ├── FixturesController.cs
│   ├── LiveScoringController.cs
│   ├── MatchesController.cs
│   ├── MatchReportsController.cs
│   ├── PlayersController.cs
│   ├── ResultsController.cs
│   ├── ScorecardsController.cs
│   ├── StatsController.cs
│   ├── TeamsController.cs
│   └── VenuesController.cs
├── Domain/               # API models (V1 external representations)
├── Stats/                # Statistics calculation and reporting
├── Charts/               # Chart.js configuration builders
├── AGGrid/              # AG Grid configuration
├── Program.cs           # Application startup and DI configuration
├── appsettings.json     # Configuration
└── log4net.config       # Logging configuration
```

## Key Migration Decisions

### Handler Context Pattern
The original project used `IHandlerContext`, `IRequestContext`, and `IResponseContext` interfaces to abstract HTTP context for testability. This pattern has been preserved to maintain compatibility with existing tests and business logic.

**Why?** This allows:
- Minimal changes to existing business logic
- Easy test porting from the old project
- Gradual migration path if needed

### Serialization
- **Old**: `System.Web.Script.Serialization.JavaScriptSerializer`
- **New**: `System.Text.Json.JsonSerializer`

### Dependency Injection
- `IDao` is registered in the DI container and injected into controllers
- No more manual `new Dao()` instantiation

### Routing
Controllers use attribute routing with `/api/` prefix:
```csharp
[Route("api/[controller]")]
```

## API Endpoints

All routes are prefixed with `/api/`.

### Entity CRUD Endpoints

- **Awards** (`/api/awards`) - Standard CRUD + filter by season
- **Committee** (`/api/committee`) - Standard CRUD + filter by season/year
- **Matches** (`/api/matches`) - Standard CRUD + filter by season
- **Players** (`/api/players`) - List, Create, Update (with includeInactive filter)
- **Teams** (`/api/teams`) - Standard CRUD (excludes "Us" team)
- **Venues** (`/api/venues`) - Standard CRUD

### Specialized Endpoints

- **Fixtures** - GET upcoming fixtures
- **Results** - GET match results  
- **Match Reports** - GET/POST match reports
- **Scorecards** - GET/POST scorecards
- **Stats** - Various statistical queries and player data
- **Live Scoring** - Real-time match scoring endpoints

See full endpoint documentation in the detailed README sections above.

## Dependencies

This project requires the CricketClub packages from a private Azure DevOps feed:
- CricketClubAccounts
- CricketClubDAL  
- CricketClubDomain
- CricketClubMiddle

### NuGet Feed Configuration

The packages are available at:
```
https://grahampontin.pkgs.visualstudio.com/109e362b-62db-4727-9d2a-e2d0e8f55904/_packaging/CricketClubCore/nuget/v3/index.json
```

A `nuget.config` file has been added to the solution root that configures this feed.

### Building the Project

To build this project, you need:
1. Network access to the Azure DevOps feed
2. Appropriate authentication credentials for the feed (if required)
3. .NET 9 SDK installed

```bash
cd TheVillageCC.WebApi
dotnet restore
dotnet build
dotnet run
```

The API will be available at:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000
- Swagger UI: https://localhost:5001/swagger

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "CricketClubDatabase": "Data Source=(local);Initial Catalog=CricketClub;Integrated Security=True"
  }
}
```

Update the connection string for your database instance.

## Testing

A corresponding test project (TheVillageCC.WebApi.Tests) has been created that ports the tests from TheVillageCC.Web.Tests.

See `TheVillageCC.WebApi.Tests/README.md` for testing guidelines.

## Logging

Log4net is configured to log to:
- Console
- `logs/thevillagecc-webapi.log` (rolling file, max 10MB)

## CORS

CORS is enabled for all origins in development. Update `Program.cs` for production CORS policy.

## Migration Notes

### What Changed
- ✅ HTTP Handlers → Controllers
- ✅ `JavaScriptSerializer` → `System.Text.Json`
- ✅ Manual instantiation → Dependency Injection
- ✅ `System.Web` → ASP.NET Core abstractions
- ✅ IIS hosting → Kestrel

### What Stayed the Same
- ✅ Business logic in CricketClub packages
- ✅ API models (Domain classes)
- ✅ `IHandlerContext` pattern for testability
- ✅ Routing patterns (with `/api/` prefix added)

### Known Limitations
- Some controllers throw `NotImplementedException` for certain operations (matching original behavior)
- Network access required to restore CricketClub packages

## Future Enhancements
- Add authentication/authorization
- Implement proper API versioning
- Add rate limiting
- Add response caching
- Replace IHandlerContext with native ASP.NET Core testing patterns
- Add health checks
- Add OpenTelemetry for observability
