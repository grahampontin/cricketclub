#nullable disable
using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Services;
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
        private readonly IMatchService _matchService;
        private readonly IWebHostEnvironment _environment;

        public MatchesController(IDao database, IWebHostEnvironment environment, IMatchService matchService)
        {
            _database     = database;
            _environment  = environment;
            _matchService = matchService;
        }

        private MatchV1 ToV1(MatchData m)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            return MatchV1.FromData(
                m,
                _matchService.GetTeam(m.OppositionID),
                _matchService.GetVenue(m.VenueID),
                id => Utils.ResolveTeamLogoUrl(id, _environment.ContentRootPath, baseUrl));
        }

        /// <summary>
        /// Gets all matches, optionally filtered by season
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<MatchV1>), StatusCodes.Status200OK)]
        public IActionResult GetAllMatches([FromQuery] int? season)
        {
            var matches = season.HasValue
                ? _matchService.GetBySeason(season.Value)
                : _matchService.GetAll();

            return Ok(matches.OrderBy(m => m.Date).Select(ToV1).ToList());
        }

        /// <summary>
        /// Gets a specific match by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MatchV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetMatch(int id)
        {
            var m = _matchService.GetById(id);
            if (m == null) return NotFound();
            return Ok(ToV1(m));
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

            var matchType  = EnumMappers.ParseMatchType(matchData.Type);
            var homeOrAway = matchData.IsHome ? HomeOrAway.Home : HomeOrAway.Away;

            var newId  = _matchService.Create(matchData.Opposition.Id, DateTime.Parse(matchData.Date),
                matchData.Venue.Id, (int)matchType, homeOrAway);
            var created = _matchService.GetById(newId)!;

            return CreatedAtAction(nameof(GetMatch), new { id = newId }, ToV1(created));
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

            var data = new MatchData
            {
                ID          = matchData.Id,
                OppositionID = matchData.Opposition.Id,
                VenueID     = matchData.Venue.Id,
                Date        = DateTime.Parse(matchData.Date),
                HomeOrAway  = matchData.IsHome ? "H" : "A",
                MatchType   = (int)EnumMappers.ParseMatchType(matchData.Type)
            };
            _matchService.Update(data);

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

            _matchService.Delete(id);
            return NoContent();
        }
    }
}
