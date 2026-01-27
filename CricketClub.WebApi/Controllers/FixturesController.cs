#nullable disable
using System.Text.Json;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    /// <summary>
    /// Provides upcoming cricket fixtures
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FixturesController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IDao _database;

        public FixturesController(IDao database)
        {
            _database = database;
        }

        /// <summary>
        /// Gets upcoming fixtures, optionally filtered by season
        /// </summary>
        /// <param name="season">Season year to filter by (optional)</param>
        /// <returns>List of upcoming fixtures</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<MatchV1>), StatusCodes.Status200OK)]
        public IActionResult GetFixtures([FromQuery] int? season)
        {
            var matches = Match.GetFixtures(_database);

            if (season.HasValue)
            {
                var startDate = new DateTime(season.Value, 1, 1);
                var endDate = new DateTime(season.Value, 12, 31);
                matches = matches.Where(m => m.MatchDate >= startDate && m.MatchDate <= endDate).ToList();
            }

            var fixtures = matches.Select(MatchV1.FromInternal).ToList();
            return Ok(fixtures);
        }
    }
}
