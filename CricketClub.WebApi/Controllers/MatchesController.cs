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

        public MatchesController(IDao database)
        {
            _database = database;
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
                    .Select(MatchV1.FromInternal)
                    .ToList();
                
                return Ok(matches);
            }

            var allMatches = Match.GetAll(_database)
                .OrderBy(m => m.MatchDate)
                .Select(MatchV1.FromInternal)
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
            return Ok(MatchV1.FromInternal(match));
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
            
            var result = MatchV1.FromInternal(match);
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
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult DeleteMatch(int id)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Match deletion is not implemented");
        }
    }
}
