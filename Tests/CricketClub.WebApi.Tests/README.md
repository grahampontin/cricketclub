# TheVillageCC.WebApi.Tests

This test project contains unit tests for the TheVillageCC.WebApi ASP.NET Core Web API.

## Test Structure

Tests are organized by controller:
- `Controllers/` - Contains test classes for each controller

## Test Helpers

### TestControllerContextFactory

A lightweight implementation of `IHandlerContext` for unit tests. This maintains compatibility with the original handler testing approach while working with the new controller architecture.

Usage:
```csharp
var context = TestControllerContextFactory.CreateHttpContext("GET", "http://test.com/api/awards");
controller.ProcessRequest(context);
```

## Porting Tests from TheVillageCC.Web.Tests

When porting tests from the old project:

1. **Update namespaces**: Change `TheVillageCC.Web.HttpHandlers` to `TheVillageCC.WebApi.Controllers`
2. **Update class names**: Change `*Handler` to `*Controller`
3. **Update URLs**: Add `/api/` prefix to URLs (e.g., `/awards` becomes `/api/awards`)
4. **Update serialization**: Replace `JavaScriptSerializer` with `System.Text.Json.JsonSerializer`
5. **Use TestControllerContextFactory**: Replace `TestHandlerContextFactory` with `TestControllerContextFactory`

## Running Tests

```bash
dotnet test
```

## Test Coverage

Current test coverage includes:
- [x] AwardsController
- [x] CommitteeController
- [x] FixturesController
- [x] ResultsController
- [ ] MatchesController
- [ ] PlayersController
- [ ] TeamsController
- [ ] VenuesController
- [ ] MatchReportsController
- [ ] ScorecardsController
- [ ] StatsController
- [ ] LiveScoringController

## Dependencies

Tests use:
- xUnit for test framework
- Moq for mocking
- Microsoft.AspNetCore.Mvc.Testing for integration testing support
