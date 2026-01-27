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
    public class ResultsController : ControllerBase
    {
        private readonly IDao database;

        public ResultsController(IDao database)
        {
            this.database = database;
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
            
            var matches = Match.GetResults(database);
            var filteredMatches = matches
                .Where(m => m.MatchDate >= startDate && m.MatchDate <= endDate)
                .ToList();

            // Fetch all match reports in one query for efficiency
            var allMatchReports = database.GetAllMatchReports();

            var results = filteredMatches.Select(m =>
            {
                allMatchReports.TryGetValue(m.ID, out var report);
                return ResultV1.FromInternal(m, report);
            }).ToList();

            return Ok(results);
        }

        /// <summary>
        /// Gets the most recent N match results across all seasons.
        /// </summary>
        /// <param name="count">Number of results to return (default 10, max 100)</param>
        [HttpGet("recent")]
        [ProducesResponseType(typeof(List<ResultV1>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetRecentResults([FromQuery] int count = 10)
        {
            if (count <= 0)
            {
                return BadRequest("count must be greater than 0");
            }

            // Avoid accidental huge payloads.
            var safeCount = Math.Min(count, 100);

            var matches = Match.GetResults(database)
                .OrderByDescending(m => m.MatchDate)
                .Take(safeCount)
                .ToList();

            var allMatchReports = database.GetAllMatchReports();

            var results = matches.Select(m =>
            {
                allMatchReports.TryGetValue(m.ID, out var report);
                return ResultV1.FromInternal(m, report);
            }).ToList();

            return Ok(results);
        }
    }
}
