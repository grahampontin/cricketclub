# CricketClub.WebApi.Tests

This project contains unit tests and integration tests for `CricketClub.WebApi`.

## Unit test style

Controllers are tested using **direct action method invocation** (e.g., calling `GetResults(...)` and asserting on `OkObjectResult`, `CreatedAtActionResult`, etc.).

This keeps tests aligned with modern ASP.NET Core practices and avoids the legacy handler-style abstraction.

## Integration tests

Integration tests use `WebApplicationFactory<Program>` and override the DI registration for `IDao` to inject a mocked DAO.

## Running Tests

```bash
dotnet test
```

## Test Coverage

Current test coverage includes:
- [x] AwardsController
- [x] CommitteeController
- [x] FixturesController
- [x] ResultsController (includes match report data)
- [ ] MatchesController
- [ ] PlayersController
- [ ] TeamsController
- [ ] VenuesController
- [ ] ScorecardsController
- [ ] StatsController
- [ ] LiveScoringController

## Dependencies

Tests use:
- xUnit for test framework
- Moq for mocking
- Microsoft.AspNetCore.Mvc.Testing for integration testing support
