# Copilot workspace instructions (cricketclub)

These are repository-specific conventions for automated changes.

## C# / .NET conventions

- Prefer nullable enabled in production projects. **Do not add `#nullable disable`** to new or edited files.
- Follow .NET naming conventions:
  - private fields: `camelCase`
  - properties/methods: `PascalCase`
  - locals/parameters: `camelCase`
- Use explicit ASP.NET Core annotations on controller actions (`[HttpGet]`, `[HttpPost]`, `[ProducesResponseType]`, etc.).

## Test conventions

- Tests should be deterministic and isolated.
- Always clear the global `CricketClubMiddle.InternalCache` at test start:
  - Call `TestDefaults.ResetInternalCache()` in test constructors/fixtures.
- When tests involve `MatchV1`/`VenueV1` mapping, always ensure venues returned by mocks have **non-null `Coordinates`**:
  - Call `TestDefaults.SetupSafeVenueAndTeamLookups(mockDao)`.

## Logging

- Never log secrets.
- If logging connection strings, redact passwords/credentials.

