# Tech Debt: N+1 / O(xN) Database Query Issues

Audit performed: 2026-05-20  
High-priority items have been addressed in the same session — see commit history.  
The items below remain open.

---

## ✅ Fixed (High Priority)

| # | Location | What was fixed |
|---|---|---|
| H1 | `StatsProvider.Query` — "innings" | `Player.GetAll()` → `Player.GetAll(true, dao)` — was O(3N) lazy loads |
| H2 | `StatsProvider.Query` — "teams" | `Team.GetAll()` → `Team.GetAll(dao)` — was bypassing injected DAO |
| H3 | `StatsProvider.Query` — "venues" | `Venue.GetAll()` → `Venue.GetAll(dao)` — was bypassing injected DAO |
| H4 | `StatsProvider.GetPlayerBattingStatsBreakDown` | Added `PreloadMatchCaptainsAndKeepers` + `IDao.GetPlayerDataBulk` + `Player.PrewarmCache`. Captain/Keeper lookups are now a single bulk query instead of O(N) |
| H5 | `StatsProvider.GetPlayerBowlingStatsBreakDown` | Same bulk pre-load as H4 |
| H6 | `StatsProvider` — "As Captain" / "As Keeper" pivots | Use `match.CaptainID` / `match.WicketKeeperID` passthroughs instead of constructing a `Player` object just to read its ID |

---

## 🟡 Open (Medium Priority)

### TODO-1 — `GET /api/stats/leadingplayers` — double iteration of per-player stats
**File:** `StatsController.cs` lines ~131–163  
**Issue:** Each of `GetRunsScored()`, `GetWicketsTaken()`, `GetCatchesTaken()` is evaluated twice per player — once in `.Max(a => a.GetX())` and again in `.Where(a => a.GetX() == mostX)`. This is in-memory (no DB), but wastes CPU for large rosters.  
**Suggested fix:** Materialise each aggregate into a `Dictionary<int, int>` (playerId → value) once before the max/filter chain.
```csharp
var runsById = players.ToDictionary(p => p.Id, p => p.GetRunsScored());
var mostRuns = runsById.Values.Max();
var topRunScorers = players.Where(p => runsById[p.Id] == mostRuns)...
```

---

### TODO-2 — `GET /api/stats/familytree` — O(N²) RingerOf inner scan
**File:** `StatsController.cs` lines ~99–106  
**Issue:** Inside the `.Select()` projection, `allPlayers.Where(c => c.RingerOf != null && c.RingerOf.Id == p.Id)` does a full linear scan of all players for every player — O(N²) in-memory.  
**Suggested fix:** Build a lookup before the projection:
```csharp
var capsByParentId = allPlayers
    .Where(c => c.RingerOf != null)
    .GroupBy(c => c.RingerOf!.Id)
    .ToDictionary(g => g.Key, g => g.Sum(c => c.Caps));

var familyTreeNodes = allPlayers.Select(p => new FamilyTreeNode
{
    ...
    ResponsibleCaps = (capsByParentId.TryGetValue(p.Id, out var childCaps) ? childCaps : 0) + p.Caps
}).ToList();
```

---

### TODO-3 — `GET /api/livescoring/matches` (no season) — `GetCurrentBallByBallState()` per in-progress match
**File:** `LiveScoringController.cs` lines ~57–66  
**Issue:** For each fixture that `GetIsBallByBallInProgress()` returns true for, `GetCurrentBallByBallState()` is called — each call loads all ball-by-ball data for that match from the DB. In practice this is bounded to the number of simultaneously in-progress games (usually 0–1 for a club), so the practical impact is very low.  
**Suggested fix:** Only worth addressing if the club ever runs multiple simultaneous live-scored matches. If so, add `IDao.GetBallByBallStateBulk(IEnumerable<int> matchIds)` and process the results in-memory.

---

### TODO-4 — `GET /api/livescoring/matches` (with `?season=`) — cold-cache Team/Venue lookups
**File:** `LiveScoringController.cs` lines ~48–54  
**Issue:** `ToV1(match)` → `MatchV1.FromInternal(match, ...)` accesses `match.Opposition` and `match.Venue`. Both are backed by `InternalCache` (24h TTL), so repeated calls are cache hits. On first call (cold cache) this is O(distinct teams + venues in the season) queries.  
**Suggested fix:** Acceptable for current dataset size. If it becomes a problem, pre-call `_matchService.GetTeam`/`GetVenue` for all teams/venues referenced in the season before the `.Select(ToV1)` loop, or use the `IMatchService` bulk pattern instead of `Match.FromInternal`.

---

### TODO-5 — `GET /api/venues/{id}/details` — redundant `GetAllVenueStatsCache()` call inside `GetCachedStats()`
**File:** `VenuesController.cs` lines ~162–189 + `Venue.cs` `GetCachedStats()`  
**Issue:** `GetVenueDetails` calls `GetAllVenueStatsCache()` (via `venue.GetCachedStats()`) and then calls `venue.GetMatches()`. `GetCachedStats()` internally calls `myDAO.GetAllVenueStatsCache()` — a full table scan — just to look up one row for the current venue.  
**Suggested fix:** Either (a) overload `GetCachedStats(Dictionary<int, VenueStatsCacheData> allStats)` so the controller can pass the already-fetched dictionary, or (b) add `IDao.GetVenueStatsCache(int venueId)` for single-venue lookup.

---

### TODO-6 — `GET /api/venues/{id}/details` and `GET /api/teams/{id}/details` — cold-cache Opposition Team lookups per match
**File:** `VenuesController.cs` + `TeamsController.cs` detail endpoints  
**Issue:** `ResultV1.FromInternal(m, report)` accesses `m.Opposition.Name` → `new Team(id, dao)` per match. `Team` uses `InternalCache` (24h TTL) so only a cold-start issue. For a venue/team with matches against 20+ distinct opponents, this could be 20+ queries on first hit.  
**Suggested fix:** Now that `Player.PrewarmCache` and `GetPlayerDataBulk` patterns exist, the same approach could be applied to teams. Add `Team.PrewarmCache(IEnumerable<TeamData>)` + `IDao.GetTeamDataBulk(IEnumerable<int>)`, then bulk-warm before the `venueMatches.Select(...)` loop. Low priority given the existing InternalCache coverage.

