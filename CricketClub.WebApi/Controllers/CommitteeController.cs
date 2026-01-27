#nullable disable
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    /// <summary>
    /// Manages committee members
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CommitteeController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IDao _database;

        public CommitteeController(IDao database)
        {
            _database = database;
        }

        /// <summary>
        /// Gets all committee members, optionally filtered by season or year
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<CommitteePostV1>), StatusCodes.Status200OK)]
        public IActionResult GetAllCommitteeMembers([FromQuery] int? season, [FromQuery] int? year)
        {
            var filterYear = season ?? year;
            var allEntities = _database.GetAllCommitteeData().Select(CommitteePostV1.ToExternal).ToList();
            
            if (filterYear.HasValue)
            {
                allEntities = allEntities.Where(a => a.Year == filterYear.Value).ToList();
            }

            return Ok(allEntities);
        }

        /// <summary>
        /// Gets a specific committee member by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CommitteePostV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCommitteeMember(int id)
        {
            var committeeData = _database.GetCommitteeData(id);
            if (committeeData == null)
            {
                return NotFound();
            }
            
            return Ok(CommitteePostV1.ToExternal(committeeData));
        }

        /// <summary>
        /// Creates a new committee member
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CommitteePostV1), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateCommitteeMember([FromBody] CommitteePostV1 committeeMember)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdId = _database.CreateNewCommittee(CommitteePostV1.ToInternal(committeeMember));
            var created = CommitteePostV1.ToExternal(_database.GetCommitteeData(createdId));
            
            return CreatedAtAction(nameof(GetCommitteeMember), new { id = createdId }, created);
        }

        /// <summary>
        /// Updates an existing committee member
        /// </summary>
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CommitteePostV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateCommitteeMember([FromBody] CommitteePostV1 committeeMember)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _database.UpdateCommittee(CommitteePostV1.ToInternal(committeeMember));
            return Ok(committeeMember);
        }

        /// <summary>
        /// Deletes a committee member by ID
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteCommitteeMember(int id)
        {
            _database.DeleteCommittee(id);
            return NoContent();
        }
    }
}
