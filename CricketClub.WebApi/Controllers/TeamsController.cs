using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Services;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    /// <summary>
    /// Manages opposition cricket teams
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TeamsController : ControllerBase
    {
        private readonly IDao _database;
        private readonly IWebHostEnvironment _environment;
        private readonly IMatchService _matchService;

        public TeamsController(IDao database, IWebHostEnvironment environment, IMatchService matchService)
        {
            _database = database;
            _environment = environment;
            _matchService = matchService;
        }

        /// <summary>
        /// Gets all teams (excluding "Us"), each with a resolved logo URL.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<TeamV1>), StatusCodes.Status200OK)]
        public IActionResult GetAllTeams()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var teams = Team.GetAll(_database)
                .Where(t => !t.IsUs)
                .Select(t => TeamV1.FromInternal(t, id => ResolveTeamLogoUrl(id, baseUrl)))
                .OrderBy(t => t.Name)
                .ToList();
            return Ok(teams);
        }

        /// <summary>
        /// Gets a specific team by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TeamV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetTeam(int id)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var team = new Team(id, _database);
            return Ok(TeamV1.FromInternal(team, teamId => ResolveTeamLogoUrl(teamId, baseUrl)));
        }

        /// <summary>
        /// Gets detailed information for a specific team including past matches and difficulty rating.
        /// Difficulty is calculated relative to all other opposition teams using the pre-computed
        /// team_stats_cache: bottom 33% win rate = red (hardest), middle = amber, top 33% = green (easiest).
        /// </summary>
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(TeamDetailV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetTeamDetails(int id)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var allStats = _database.GetAllTeamStatsCache();
            var difficultyMap = BuildDifficultyMap(allStats);
            var team = new Team(id, _database);

            string? homeVenueName = null;
            if (team.HomeVenueId.HasValue)
            {
                try { homeVenueName = new Venue(team.HomeVenueId.Value, _database).Name; }
                catch { /* venue not found */ }
            }

            var teamMatches = team.GetMatches()
                .Where(m => m.MatchDate <= DateTime.Today)
                .OrderByDescending(m => m.MatchDate)
                .ToList();

            var matchReports = _database.GetAllMatchReports();
            var resultList = teamMatches.Select(m =>
            {
                matchReports.TryGetValue(m.ID, out var report);
                return ResultV1.FromInternal(m, report, id => ResolveTeamLogoUrl(id, baseUrl));
            }).ToList();

            allStats.TryGetValue(id, out var myStats);

            return Ok(new TeamDetailV1
            {
                Id               = team.ID,
                Name             = team.Name,
                LogoUrl          = ResolveTeamLogoUrl(team.ID, baseUrl),
                WebsiteUrl       = team.WebsiteUrl,
                HomeVenueId      = team.HomeVenueId,
                HomeVenueName    = homeVenueName,
                WinPercentage    = myStats?.WinPercentage ?? 0.0,
                DifficultyRating = difficultyMap.TryGetValue(id, out var diff) ? diff : "unknown",
                DifficultyScore  = myStats is { Played: >= 3 } ? myStats.DifficultyScore : null,
                Matches          = resultList
            });
        }

        /// <summary>
        /// Returns a lightweight summary for every opposition team in a single response.
        /// Includes pre-computed stats (played, won, lost, noResult, winPercentage) and a
        /// traffic-light difficulty rating, suitable for a public landing page without
        /// requiring per-team requests.
        /// </summary>
        [HttpGet("summaries")]
        [ProducesResponseType(typeof(List<TeamSummaryV1>), StatusCodes.Status200OK)]
        public IActionResult GetTeamSummaries()
        {
            var allStats    = _database.GetAllTeamStatsCache();
            var difficultyMap = BuildDifficultyMap(allStats);

            // Build a venue-name lookup in a single bulk call to avoid per-team queries.
            var venueNames = _database.GetAllVenueData()
                .ToDictionary(v => v.ID, v => v.Name);

            var summaries = Team.GetAll(_database)
                .Where(t => !t.IsUs)
                .Select(t =>
                {
                    string? homeVenueName = null;
                    if (t.HomeVenueId.HasValue)
                        venueNames.TryGetValue(t.HomeVenueId.Value, out homeVenueName);

                    allStats.TryGetValue(t.ID, out var stats);

                    return new TeamSummaryV1
                    {
                        Id               = t.ID,
                        Name             = t.Name,
                        HomeVenueName    = homeVenueName,
                        DifficultyRating = difficultyMap.TryGetValue(t.ID, out var diff) ? diff : "unknown",
                        DifficultyScore  = stats is { Played: >= 3 } ? stats.DifficultyScore : null,
                        WinPercentage    = stats?.WinPercentage ?? 0.0,
                        Played           = stats?.Played   ?? 0,
                        Won              = stats?.Won      ?? 0,
                        Lost             = stats?.Lost     ?? 0,
                        NoResult         = stats?.Abandoned ?? 0
                    };
                })
                .OrderBy(s => s.Name)
                .ToList();

            return Ok(summaries);
        }

        /// <summary>
        /// Admin endpoint: forces a full recalculation of team_stats_cache for all teams.
        /// Normally kept current by Match.Save(); use this after bulk data imports or manual DB edits.
        /// </summary>
        [HttpPost("recalculate-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult RecalculateStats()
        {
            TeamStatsRecalculator.RecalculateAll(_database);
            return Ok(new { message = "Team stats cache recalculated successfully." });
        }

        /// <summary>
        /// Creates a new team
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(TeamV1), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateTeam([FromBody] TeamV1 teamData)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var team = Team.CreateNewTeam(teamData.Name, _database);
            _matchService.InvalidateTeamsCache();
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var result = TeamV1.FromInternal(team, id => ResolveTeamLogoUrl(id, baseUrl));
            return CreatedAtAction(nameof(GetTeam), new { id = team.ID }, result);
        }

        /// <summary>
        /// Updates an existing team
        /// </summary>
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(TeamV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateTeam([FromBody] TeamV1 teamData)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var team = new Team(teamData.Id, _database)
            {
                Name        = teamData.Name,
                WebsiteUrl  = teamData.WebsiteUrl,
                HomeVenueId = teamData.HomeVenueId
            };
            team.Save();
            InternalCache.GetInstance().Remove($"team{teamData.Id}");
            _matchService.InvalidateTeamsCache();
            return Ok(teamData);
        }

        /// <summary>
        /// Deletes a team by ID. The team must have no associated matches.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult DeleteTeam(int id)
        {
            try
            {
                _database.DeleteTeam(id);
                InternalCache.GetInstance().Remove($"team{id}");
                _matchService.InvalidateTeamsCache();
                return NoContent();
            }
            catch (Exception ex) when (ex.Message.Contains("REFERENCE") || ex.Message.Contains("FK") || ex.Message.Contains("foreign key"))
            {
                return Conflict(new { message = $"Team {id} cannot be deleted because it has associated matches or other records." });
            }
        }

        // ── private helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// Mirrors the player image pattern: looks for Assets/TeamImages/{teamId}.png;
        /// falls back to Assets/TeamImages/0.png (placeholder) if the file does not exist.
        /// </summary>
        private string ResolveTeamLogoUrl(int teamId, string baseUrl) =>
            Utils.ResolveTeamLogoUrl(teamId, _environment.ContentRootPath, baseUrl);

        private static Dictionary<int, string> BuildDifficultyMap(Dictionary<int, TeamStatsCacheData> allStats)
        {
            var map = new Dictionary<int, string>();

            // Teams with fewer than 3 completed matches do not have enough data to rate.
            foreach (var s in allStats.Values.Where(s => s.Played < 3))
                map[s.TeamId] = "unknown";

            // Rank eligible teams by DifficultyScore (ascending = easiest first).
            // The score is the mean normalised run margin in the opposition's favour:
            //   negative → we regularly outscored them (easier)
            //   positive → they regularly outscored us (harder)
            var ranked = allStats.Values
                .Where(s => s.Played >= 3)
                .OrderBy(s => s.DifficultyScore)
                .ToList();

            if (ranked.Count == 0) return map;

            var third = ranked.Count / 3;
            for (int i = 0; i < ranked.Count; i++)
            {
                map[ranked[i].TeamId] = i < third     ? "green"  :   // easiest third
                                        i < third * 2 ? "amber"  :   // middle third
                                                        "red";        // hardest third
            }
            return map;
        }
    }
}
