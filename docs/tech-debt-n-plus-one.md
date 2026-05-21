# Tech Debt: N+1 / O(xN) Database Query Issues

Audit performed: 2026-05-20  
High-priority items have been addressed in the same session — see commit history.  
Medium-priority items addressed 2026-05-21 — see commit history.  
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

## ✅ Fixed (Medium Priority)

| # | Location | What was fixed |
|---|---|---|
| M1 | `StatsController.GetLeadingPlayers` | Materialised `runsById`, `wicketsById`, `catchesById` dictionaries once before the max/filter chain — eliminated the double call per player (TODO-1) |
| M2 | `StatsController.GetFamilyTree` | Built `capsByParentId` lookup before the `.Select()` projection — replaced O(N²) inner scan with O(N) pass (TODO-2) |
| M5 | `VenuesController.GetVenueDetails` + `Venue.GetCachedStats()` | Added `GetCachedStats(Dictionary<int,VenueStatsCacheData>)` overload; controller now fetches `GetAllVenueStatsCache()` once and passes the dictionary in — eliminates the redundant full table scan (TODO-5) |
| M6 | `VenuesController.GetVenueDetails` + `TeamsController.GetTeamDetails` | Added `IDao.GetTeamDataBulk` + `Team.PrewarmCache`; both endpoints now bulk-warm the Team InternalCache for all opposition IDs before the match loop — replaces O(N distinct teams) per-match queries with a single batch query on cold cache (TODO-6) |

---

## 🟡 Open (Low Priority)

### TODO-3 — `GET /api/livescoring/matches` (no season) — `GetCurrentBallByBallState()` per in-progress match
**File:** `LiveScoringController.cs` lines ~57–66  
**Issue:** For each fixture that `GetIsBallByBallInProgress()` returns true for, `GetCurrentBallByBallState()` is called — each call loads all ball-by-ball data for that match from the DB. In practice this is bounded to the number of simultaneously in-progress games (usually 0–1 for a club), so the practical impact is very low.  
**Suggested fix:** Only worth addressing if the club ever runs multiple simultaneous live-scored matches. If so, add `IDao.GetBallByBallStateBulk(IEnumerable<int> matchIds)` and process the results in-memory.

---

### TODO-4 — `GET /api/livescoring/matches` (with `?season=`) — cold-cache Team/Venue lookups
**File:** `LiveScoringController.cs` lines ~48–54  
**Issue:** `ToV1(match)` → `MatchV1.FromInternal(match, ...)` accesses `match.Opposition` and `match.Venue`. Both are backed by `InternalCache` (24h TTL), so repeated calls are cache hits. On first call (cold cache) this is O(distinct teams + venues in the season) queries.  
**Suggested fix:** Acceptable for current dataset size. If it becomes a problem, pre-call `_matchService.GetTeam`/`GetVenue` for all teams/venues referenced in the season before the `.Select(ToV1)` loop, or use the `IMatchService` bulk pattern instead of `Match.FromInternal`.

