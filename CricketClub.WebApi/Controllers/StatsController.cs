#nullable disable
using System.Text.Json;
using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Stats;
using CricketClubDAL;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public IActionResult QueryStats([FromBody] StatsQueryV1 query)
        {
            try
            {
                var statsData = StatsProvider.Query(query);
                return Ok(statsData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        [HttpGet("player/{playerId}/detail")]
        public IActionResult GetPlayerDetail(int playerId)
        {
            try
            {
                var playerDetailV1 = StatsProvider.QueryPlayer(playerId, (s) => Path.Combine(environment.WebRootPath, s.TrimStart('~', '/')));
                return Ok(playerDetailV1);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        [HttpGet("player/{playerId}/{statsType}")]
        public IActionResult GetPlayerStats(int playerId, string statsType)
        {
            try
            {
                var dataCollection = StatsProvider.GetPlayerStatsBreakDown(playerId, statsType);
                return Ok(dataCollection);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        [HttpGet("chart/{playerId}/{chartType}")]
        public IActionResult GetChartData(int playerId, string chartType)
        {
            try
            {
                var chartData = StatsProvider.BuildChartData(playerId, chartType);
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        [HttpGet("playermatches/{playerId}")]
        public IActionResult GetPlayerMatches(int playerId)
        {
            try
            {
                var data = StatsProvider.QueryPlayerMatches(playerId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        [HttpGet("familytree")]
        public IActionResult GetFamilyTree()
        {
            try
            {
                var allPlayers = Player.GetAll(true, database);
                var familyTreeNodes = allPlayers.Select(p => new FamilyTreeNode()
                {
                    Id = p.Id,
                    ParentId = p.RingerOf == null ? -2 : p.RingerOf.Id,
                    Name = p.FirstName + " " + p.Surname,
                    Caps = p.Caps,
                    ResponsibleCaps = allPlayers.Where(c => c.RingerOf != null && c.RingerOf.Id == p.Id).Sum(c => c.Caps) + p.Caps
                }).ToList();
                familyTreeNodes.Add(new FamilyTreeNode()
                {
                    Id = -2,
                    Name = "The Village CC"
                });

                return Ok(familyTreeNodes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        [HttpGet("leadingplayers")]
        public IActionResult GetLeadingPlayers()
        {
            try
            {
                var players = Player.GetAll(true, database).Where(a => a.Id > 0).ToList();
                
                if (!players.Any())
                {
                    return Ok(new List<LeadingPlayerCategoryV1>());
                }
                
                var categories = new List<LeadingPlayerCategoryV1>();

                // Most Runs
                var mostRuns = players.Max(a => a.GetRunsScored());
                var topRunScorers = players.Where(a => a.GetRunsScored() == mostRuns)
                    .Select(p => CreateLeadingPlayerEntry(p, p.GetRunsScored())).ToList();
                categories.Add(new LeadingPlayerCategoryV1
                {
                    Category = "Most Runs",
                    Players = topRunScorers
                });

                // Most Wickets
                var mostWickets = players.Max(a => a.GetWicketsTaken());
                var topWicketTakers = players.Where(a => a.GetWicketsTaken() == mostWickets)
                    .Select(p => CreateLeadingPlayerEntry(p, p.GetWicketsTaken())).ToList();
                categories.Add(new LeadingPlayerCategoryV1
                {
                    Category = "Most Wickets",
                    Players = topWicketTakers
                });

                // Most Catches
                var mostCatches = players.Max(a => a.GetCatchesTaken());
                var topCatchTakers = players.Where(a => a.GetCatchesTaken() == mostCatches)
                    .Select(p => CreateLeadingPlayerEntry(p, p.GetCatchesTaken())).ToList();
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
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message + Environment.NewLine + ex.StackTrace);
            }
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
