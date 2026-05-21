using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Stats;
using CricketClubDAL;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class StatsController : Controller
    {
        private readonly IWebHostEnvironment environment;
        private readonly IDao database;

        public StatsController(IWebHostEnvironment environment, IDao database)
        {
            this.environment = environment;
            this.database = database;
        }

        [HttpPost("query")]
        [ProducesResponseType(typeof(StatsDataV1), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult QueryStats([FromBody] StatsQueryV1 query)
        {
            var statsData = StatsProvider.Query(query, database);
            return Ok(statsData);
        }

        [HttpGet("player/{playerId}/detail")]
        [ProducesResponseType(typeof(PlayerDetailV1), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult GetPlayerDetail(int playerId)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var playerDetailV1 = StatsProvider.QueryPlayer(
                playerId,
                imagePlayerId => new Uri(new Uri(baseUrl), $"/images/players/{imagePlayerId}.png").ToString(),
                environment.ContentRootPath,
                database);

            return Ok(playerDetailV1);
        }

        [HttpGet("player/{playerId}/summary")]
        [ProducesResponseType(typeof(PlayerSummaryV1), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult GetPlayerSummary(int playerId)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var summary = StatsProvider.QueryPlayerSummary(
                playerId,
                imagePlayerId => new Uri(new Uri(baseUrl), $"/images/players/{imagePlayerId}.png").ToString(),
                environment.ContentRootPath,
                database);

            return Ok(summary);
        }

        [HttpGet("player/{playerId}/{statsType}")]
        [ProducesResponseType(typeof(IEnumerable<StatsDataV1>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult GetPlayerStats(int playerId, string statsType)
        {
            var dataCollection = StatsProvider.GetPlayerStatsBreakDown(playerId, statsType, database);
            return Ok(dataCollection);
        }

        [HttpGet("chart/{playerId}/{chartType}")]
        [ProducesResponseType(typeof(CricketClub.WebApi.Charts.ChartJsConfig), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult GetChartData(int playerId, string chartType)
        {
            var chartData = StatsProvider.BuildChartData(playerId, chartType);
            return Ok(chartData);
        }

        [HttpGet("playermatches/{playerId}")]
        [ProducesResponseType(typeof(StatsDataV1), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult GetPlayerMatches(int playerId)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var data = StatsProvider.QueryPlayerMatches(
                playerId,
                id => Utils.ResolveTeamLogoUrl(id, environment.ContentRootPath, baseUrl),
                database);
            return Ok(data);
        }

        [HttpGet("familytree")]
        [ProducesResponseType(typeof(List<FamilyTreeNode>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult GetFamilyTree()
        {
            var allPlayers = Player.GetAll(true, database);

            // Build a lookup of parentId → total caps of all ringers for that parent (TODO-2).
            // This replaces an O(N²) inner scan with a single O(N) pass.
            var capsByParentId = allPlayers
                .Where(c => c.RingerOf != null)
                .GroupBy(c => c.RingerOf!.Id)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Caps));

            var familyTreeNodes = allPlayers.Select(p => new FamilyTreeNode()
            {
                Id = p.Id,
                ParentId = p.RingerOf == null ? -2 : p.RingerOf.Id,
                Name = p.FirstName + " " + p.Surname,
                Caps = p.Caps,
                ResponsibleCaps = (capsByParentId.TryGetValue(p.Id, out var childCaps) ? childCaps : 0) + p.Caps
            }).ToList();
            familyTreeNodes.Add(new FamilyTreeNode()
            {
                Id = -2,
                Name = "The Village CC"
            });

            return Ok(familyTreeNodes);
        }

        [HttpGet("leadingplayers")]
        [ProducesResponseType(typeof(List<LeadingPlayerCategoryV1>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult GetLeadingPlayers()
        {
            var players = Player.GetAll(true, database).Where(a => a.Id > 0).ToList();

            if (!players.Any())
            {
                return Ok(new List<LeadingPlayerCategoryV1>());
            }

            var categories = new List<LeadingPlayerCategoryV1>();

            // Materialise each aggregate once to avoid evaluating the same method twice per player
            // (once in .Max() and again in .Where()) — TODO-1.
            var runsById       = players.ToDictionary(p => p.Id, p => p.GetRunsScored());
            var wicketsById    = players.ToDictionary(p => p.Id, p => p.GetWicketsTaken());
            var catchesById    = players.ToDictionary(p => p.Id, p => p.GetCatchesTaken());

            // Most Runs
            var mostRuns = runsById.Values.Max();
            var topRunScorers = players.Where(a => runsById[a.Id] == mostRuns)
                .Select(p => CreateLeadingPlayerEntry(p, runsById[p.Id])).ToList();
            categories.Add(new LeadingPlayerCategoryV1
            {
                Category = "Most Runs",
                Players = topRunScorers
            });

            // Most Wickets
            var mostWickets = wicketsById.Values.Max();
            var topWicketTakers = players.Where(a => wicketsById[a.Id] == mostWickets)
                .Select(p => CreateLeadingPlayerEntry(p, wicketsById[p.Id])).ToList();
            categories.Add(new LeadingPlayerCategoryV1
            {
                Category = "Most Wickets",
                Players = topWicketTakers
            });

            // Most Catches
            var mostCatches = catchesById.Values.Max();
            var topCatchTakers = players.Where(a => catchesById[a.Id] == mostCatches)
                .Select(p => CreateLeadingPlayerEntry(p, catchesById[p.Id])).ToList();
            categories.Add(new LeadingPlayerCategoryV1
            {
                Category = "Most Catches",
                Players = topCatchTakers
            });

            // Most Appearances
            var mostAppearances = players.Max(a => a.Caps);
            var topAppearances = players.Where(a => a.Caps == mostAppearances)
                .Select(p => CreateLeadingPlayerEntry(p, p.Caps)).ToList();
            categories.Add(new LeadingPlayerCategoryV1
            {
                Category = "Most Appearances",
                Players = topAppearances
            });

            return Ok(categories);
        }

        private static LeadingPlayerEntryV1 CreateLeadingPlayerEntry(Player player, int value)
        {
            return new LeadingPlayerEntryV1
            {
                PlayerId = player.Id,
                PlayerName = player.FirstName + " " + player.Surname,
                Value = value
            };
        }
    }

    public class FamilyTreeNode
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public int Caps { get; set; }
        public int ResponsibleCaps { get; set; }
    }
}
