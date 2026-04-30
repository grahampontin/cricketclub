#nullable disable
using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Services;
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
        private readonly IMatchService _matchService;
        private readonly IWebHostEnvironment _environment;

        public FixturesController(IMatchService matchService, IWebHostEnvironment environment)
        {
            _matchService = matchService;
            _environment  = environment;
        }

        /// <summary>
        /// Gets upcoming fixtures, optionally filtered by season
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<MatchV1>), StatusCodes.Status200OK)]
        public IActionResult GetFixtures([FromQuery] int? season)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            var matches = season.HasValue
                ? _matchService.GetBySeason(season.Value).Where(m => m.Date >= DateTime.Today)
                : _matchService.GetFixtures();

            var fixtures = matches
                .Select(m => MatchV1.FromData(
                    m,
                    _matchService.GetTeam(m.OppositionID),
                    _matchService.GetVenue(m.VenueID),
                    id => Utils.ResolveTeamLogoUrl(id, _environment.ContentRootPath, baseUrl)))
                .ToList();

            return Ok(fixtures);
        }
    }
}
