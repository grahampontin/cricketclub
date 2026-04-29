#nullable disable
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;
using Match = CricketClubMiddle.Match;

namespace CricketClub.WebApi.Controllers
{
    /// <summary>
    /// Manages cricket matches
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MatchesController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IDao _database;
        private readonly IWebHostEnvironment _environment;

        public MatchesController(IDao database, IWebHostEnvironment environment)
        {
            _database = database;
            _environment = environment;
        }

        private MatchV1 ToV1(Match match)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            return MatchV1.FromInternal(match, id => Utils.ResolveTeamLogoUrl(id, _environment.ContentRootPath, baseUrl));
        }

        /// <summary>
        /// Gets all matches, optionally filtered by season
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<MatchV1>), StatusCodes.Status200OK)]
        public IActionResult GetAllMatches([FromQuery] int? season)
        {
            if (season.HasValue)
            {
                var matches = Match.GetAll(
                    new DateTime(season.Value, 1, 1),
                    new DateTime(season.Value, 12, 31),
                    null, null, _database)
                    .OrderBy(m => m.MatchDate)
                    .Select(ToV1)
                    .ToList();
                
                return Ok(matches);
            }

            var allMatches = Match.GetAll(_database)
                .OrderBy(m => m.MatchDate)
                .Select(ToV1)
                .ToList();

            return Ok(allMatches);
        }

        /// <summary>
        /// Gets a specific match by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MatchV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetMatch(int id)
        {
            var match = new Match(id, _database);
            return Ok(ToV1(match));
        }

        /// <summary>
        /// Creates a new match
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchV1), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateMatch([FromBody] MatchV1 matchData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var matchType = EnumMappers.ParseMatchType(matchData.Type);
            var homeOrAway = matchData.IsHome ? HomeOrAway.Home : HomeOrAway.Away;
            
            var match = Match.CreateNewMatch(
                new Team(matchData.Opposition.Id, _database),
                DateTime.Parse(matchData.Date),
                new Venue(matchData.Venue.Id, _database),
                matchType,
                homeOrAway,
                _database);
            
            var result = ToV1(match);
            return CreatedAtAction(nameof(GetMatch), new { id = match.ID }, result);
        }

        /// <summary>
        /// Updates an existing match
        /// </summary>
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateMatch([FromBody] MatchV1 matchData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var match = new Match(matchData.Id, _database)
            {
                OppositionID = matchData.Opposition.Id,
                VenueID = matchData.Venue.Id,
                MatchDate = DateTime.Parse(matchData.Date),
                HomeOrAway = matchData.IsHome ? HomeOrAway.Home : HomeOrAway.Away,
                Type = EnumMappers.ParseMatchType(matchData.Type)
            };
            match.Save();
            
            return Ok(matchData);
        }

        /// <summary>
        /// Deletes a match by ID
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult DeleteMatch(int id)
        {
            var blockingReasons = new List<string>();

            if (_database.GetBattingCard(id, ThemOrUs.Us).Any() || _database.GetBattingCard(id, ThemOrUs.Them).Any() ||
                _database.GetBowlingStats(id, ThemOrUs.Us).Any() || _database.GetBowlingStats(id, ThemOrUs.Them).Any())
            {
                blockingReasons.Add("scorecard data");
            }

            if (_database.GetAllBallsForMatch(id).Any())
            {
                blockingReasons.Add("ball by ball data");
            }

            var matchReport = _database.GetMatchReport(id);
            if (matchReport != MatchReportAndConditions.None)
            {
                blockingReasons.Add("a match report");
            }

            if (blockingReasons.Count > 0)
            {
                var reasons = string.Join(", ", blockingReasons);
                var verb = blockingReasons.Count == 1 ? "is" : "are";
                return Conflict($"Cannot delete match {id} because there {verb} {reasons} associated with it. Please remove this data first if you really want to delete the match.");
            }

            _database.DeleteMatch(id);
            return NoContent();
        }
    }
}
