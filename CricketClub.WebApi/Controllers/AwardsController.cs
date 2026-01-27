#nullable disable
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    /// <summary>
    /// Manages cricket awards
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AwardsController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IDao _database;

        public AwardsController(IDao database)
        {
            _database = database;
        }

        /// <summary>
        /// Gets all awards, optionally filtered by season
        /// </summary>
        /// <param name="season">Filter by season year (optional)</param>
        /// <returns>List of awards</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<AwardV1>), StatusCodes.Status200OK)]
        public IActionResult GetAllAwards([FromQuery] int? season)
        {
            var allEntities = _database.GetAllAwardsData().Select(AwardV1.FromInternal).ToList();
            
            if (season.HasValue)
            {
                allEntities = allEntities.Where(a => a.Year == season.Value).ToList();
            }

            return Ok(allEntities);
        }

        /// <summary>
        /// Gets a specific award by ID
        /// </summary>
        /// <param name="id">The award ID</param>
        /// <returns>The award</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AwardV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetAward(int id)
        {
            var awardData = _database.GetAwardData(id);
            if (awardData == null)
            {
                return NotFound();
            }
            
            return Ok(AwardV1.FromInternal(awardData));
        }

        /// <summary>
        /// Creates a new award
        /// </summary>
        /// <param name="award">The award to create</param>
        /// <returns>The created award</returns>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(AwardV1), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateAward([FromBody] AwardV1 award)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdId = Utils.ParseEnumOrThrow<Award, int>(award.Award,
                parsed => _database.CreateNewAward(
                    parsed,
                    award.Year,
                    award.PlayerId,
                    award.Data
                ));
            
            var createdAward = AwardV1.FromInternal(_database.GetAwardData(createdId));
            return CreatedAtAction(nameof(GetAward), new { id = createdId }, createdAward);
        }

        /// <summary>
        /// Updates an existing award
        /// </summary>
        /// <param name="award">The award to update</param>
        /// <returns>The updated award</returns>
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(AwardV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateAward([FromBody] AwardV1 award)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _database.UpdateAward(AwardV1.ToInternal(award));
            return Ok(award);
        }

        /// <summary>
        /// Deletes an award by ID
        /// </summary>
        /// <param name="id">The award ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteAward(int id)
        {
            _database.DeleteAward(id);
            return NoContent();
        }
    }
}
