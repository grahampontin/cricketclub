#nullable disable
using System.Text.Json;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    /// <summary>
    /// Provides match results
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ResultsController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IDao _database;

        public ResultsController(IDao database)
        {
            _database = database;
        }

        /// <summary>
        /// Gets match results, optionally filtered by season
        /// </summary>
        /// <param name="season">Season year to filter by (optional)</param>
        /// <returns>List of match results</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ResultV1>), StatusCodes.Status200OK)]
        public IActionResult GetResults([FromQuery] int? season)
        {
            var seasonYear = season ?? DateTime.Now.Year;
            var startDate = new DateTime(seasonYear, 1, 1);
            var endDate = new DateTime(seasonYear, 12, 31);
            
            var matches = Match.GetResults(_database);
            var filteredMatches = matches
                .Where(m => m.MatchDate >= startDate && m.MatchDate <= endDate)
                .ToList();

            // Fetch all match reports in one query for efficiency
            var allMatchReports = _database.GetAllMatchReports();

            var results = filteredMatches.Select(m =>
            {
                allMatchReports.TryGetValue(m.ID, out var report);
                return ResultV1.FromInternal(m, report);
            }).ToList();

            return Ok(results);
        }
    }
}
