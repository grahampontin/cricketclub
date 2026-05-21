using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    /// <summary>
    /// Manages cricket venues
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VenuesController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IDao _database;

        public VenuesController(IDao database)
        {
            _database = database;
        }

        /// <summary>
        /// Gets all venues
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<VenueV1>), StatusCodes.Status200OK)]
        public IActionResult GetAllVenues()
        {
            var venues = Venue.GetAll(_database).Select(VenueV1.FromInternal).OrderBy(v => v.Name).ToList();
            return Ok(venues);
        }

        /// <summary>
        /// Gets a specific venue by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VenueV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetVenue(int id)
        {
            var venue = new Venue(id, _database);
            if (string.IsNullOrEmpty(venue.Name))
            {
                return NotFound();
            }
            
            return Ok(VenueV1.FromInternal(venue));
        }

        /// <summary>
        /// Creates a new venue
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(VenueV1), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateVenue([FromBody] VenueV1 entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Venue.CreateNewVenue(entity.Name, entity.MapUrl, entity.Description, entity.Latitude, entity.Longitude, _database);
            var venues = Venue.GetAll(_database);
            var createdVenue = venues.OrderByDescending(v => v.ID).FirstOrDefault(v => v.Name == entity.Name);
            
            if (createdVenue == null)
            {
                return Ok(entity);
            }
            
            var result = VenueV1.FromInternal(createdVenue);
            return CreatedAtAction(nameof(GetVenue), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates an existing venue
        /// </summary>
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(VenueV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateVenue([FromBody] VenueV1 entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var venue = new Venue(entity.Id, _database)
            {
                Name = entity.Name,
                GoogleMapsLocationURL = entity.MapUrl,
                Description = entity.Description,
                Coordinates = new Tuple<decimal?, decimal?>(entity.Latitude, entity.Longitude)
            };
            venue.Save();
            
            return Ok(entity);
        }

        /// <summary>
        /// Deletes a venue by ID
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteVenue(int id)
        {
            var venue = new Venue(id, _database);
            if (!string.IsNullOrEmpty(venue.Name))
            {
                venue.Delete();
            }
            
            return NoContent();
        }

        /// <summary>
        /// Gets a lightweight summary for every venue including batting-difficulty stats.
        /// Uses venue_stats_cache for efficiency — no per-venue match queries.
        /// </summary>
        [HttpGet("summaries")]
        [ProducesResponseType(typeof(List<VenueSummaryV1>), StatusCodes.Status200OK)]
        public IActionResult GetVenueSummaries()
        {
            var allStats = _database.GetAllVenueStatsCache();

            var summaries = Venue.GetAll(_database)
                .Select(v =>
                {
                    allStats.TryGetValue(v.ID, out var stats);
                    return new VenueSummaryV1
                    {
                        Id          = v.ID,
                        Name        = v.Name,
                        Description = v.Description,
                        Latitude    = v.Coordinates.Item1,
                        Longitude   = v.Coordinates.Item2,
                        MapUrl      = v.GoogleMapsLocationURL,
                        Stats       = VenueStatsV1.FromCache(stats)
                    };
                })
                .OrderBy(v => v.Name)
                .ToList();

            return Ok(summaries);
        }

        /// <summary>
        /// Gets detailed information for a specific venue including all past matches and batting-difficulty rating.
        /// DifficultyScore 0 = minefield (batsmen struggle); 100 = road (batsmen make loads of runs).
        /// </summary>
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(VenueDetailV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetVenueDetails(int id)
        {
            var venue = new Venue(id, _database);
            if (string.IsNullOrEmpty(venue.Name))
                return NotFound();

            // Fetch venue stats once and pass the pre-loaded dictionary to avoid a redundant
            // full GetAllVenueStatsCache() scan inside GetCachedStats() (TODO-5).
            var allVenueStats = _database.GetAllVenueStatsCache();
            var stats = venue.GetCachedStats(allVenueStats);

            var venueMatches = venue.GetMatches()
                .Where(m => m.MatchDate <= DateTime.Today)
                .OrderByDescending(m => m.MatchDate)
                .ToList();

            // Bulk-warm the Team InternalCache for all distinct opposition teams referenced by
            // these matches so that match.Opposition.Name / match.Winner.Name do not trigger N
            // individual GetTeamData queries (TODO-6).
            var oppositionIds = venueMatches.Select(m => m.OppositionID).Distinct();
            var teamBulkData = _database.GetTeamDataBulk(oppositionIds);
            Team.PrewarmCache(teamBulkData.Values);

            var matchReports = _database.GetAllMatchReports();
            var resultList = venueMatches.Select(m =>
            {
                matchReports.TryGetValue(m.ID, out var report);
                return ResultV1.FromInternal(m, report);
            }).ToList();

            return Ok(new VenueDetailV1
            {
                Id          = venue.ID,
                Name        = venue.Name,
                Description = venue.Description,
                Latitude    = venue.Coordinates.Item1,
                Longitude   = venue.Coordinates.Item2,
                MapUrl      = venue.GoogleMapsLocationURL,
                Stats       = VenueStatsV1.FromCache(stats),
                Matches     = resultList
            });
        }

        /// <summary>
        /// Admin endpoint: forces a full recalculation of venue_stats_cache for all venues.
        /// Normally kept current by Match.Save(); use this after bulk data imports or manual DB edits.
        /// </summary>
        [HttpPost("recalculate-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult RecalculateStats()
        {
            VenueStatsRecalculator.RecalculateAll(_database);
            return Ok(new { message = "Venue stats cache recalculated successfully." });
        }
    }
}
