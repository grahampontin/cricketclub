using CricketClub.WebApi.Domain;
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

        public TeamsController(IDao database, IWebHostEnvironment environment)
        {
            _database = database;
            _environment = environment;
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
                return ResultV1.FromInternal(m, report);
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
                DifficultyRating = difficultyMap.TryGetValue(id, out var diff) ? diff : "green",
                Matches          = resultList
            });
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
            return Ok(teamData);
        }

        /// <summary>
        /// Deletes a team by ID
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult DeleteTeam(int id)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Team deletion is not implemented");
        }

        // ── private helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// Mirrors the player image pattern: looks for Assets/TeamImages/{teamId}.png;
        /// falls back to Assets/TeamImages/0.png (placeholder) if the file does not exist.
        /// </summary>
        private string ResolveTeamLogoUrl(int teamId, string baseUrl)
        {
            var imageRoot = Path.Combine(_environment.ContentRootPath, "Assets", "TeamImages");
            var imagePath = Path.Combine(imageRoot, $"{teamId}.png");
            var resolvedId = System.IO.File.Exists(imagePath) ? teamId : 0;
            return new Uri(new Uri(baseUrl), $"/images/teams/{resolvedId}.png").ToString();
        }

        private static Dictionary<int, string> BuildDifficultyMap(Dictionary<int, TeamStatsCacheData> allStats)
        {
            var ranked = allStats.Values
                .Where(s => s.Played > 0)
                .OrderBy(s => s.WinPercentage)
                .ToList();

            var map = new Dictionary<int, string>();
            if (ranked.Count == 0) return map;

            var third = ranked.Count / 3;
            for (int i = 0; i < ranked.Count; i++)
            {
                map[ranked[i].TeamId] = i < third     ? "red"   :
                                        i < third * 2 ? "amber" :
                                                        "green";
            }
            return map;
        }
    }
}
