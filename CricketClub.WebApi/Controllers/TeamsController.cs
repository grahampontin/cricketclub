#nullable disable
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    /// <summary>
    /// Manages opposition cricket teams
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TeamsController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IDao _database;

        public TeamsController(IDao database)
        {
            _database = database;
        }

        /// <summary>
        /// Gets all teams (excluding "Us")
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<TeamV1>), StatusCodes.Status200OK)]
        public IActionResult GetAllTeams()
        {
            var teams = Team.GetAll(_database)
                .Where(t => !t.IsUs)
                .Select(TeamV1.FromInternal)
                .OrderBy(t => t.Name)
                .ToList();

            return Ok(teams);
        }

        /// <summary>
        /// Gets a specific team by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TeamV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetTeam(int id)
        {
            var team = new Team(id, _database);
            return Ok(TeamV1.FromInternal(team));
        }

        /// <summary>
        /// Creates a new team
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(TeamV1), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateTeam([FromBody] TeamV1 teamData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var team = Team.CreateNewTeam(teamData.Name, _database);
            var result = TeamV1.FromInternal(team);
            
            return CreatedAtAction(nameof(GetTeam), new { id = team.ID }, result);
        }

        /// <summary>
        /// Updates an existing team
        /// </summary>
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(TeamV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateTeam([FromBody] TeamV1 teamData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var team = new Team(teamData.Id, _database) { Name = teamData.Name };
            team.Save();
            
            return Ok(teamData);
        }

        /// <summary>
        /// Deletes a team by ID
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult DeleteTeam(int id)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Team deletion is not implemented");
        }
    }
}
