#nullable disable
using CricketClub.WebApi.Domain;
using CricketClubDAL;
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
    }
}
