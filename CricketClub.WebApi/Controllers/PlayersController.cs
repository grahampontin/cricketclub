#nullable disable
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    /// <summary>
    /// Manages cricket players
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PlayersController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IDao _database;

        public PlayersController(IDao database)
        {
            _database = database;
        }

        /// <summary>
        /// Gets all players, optionally including inactive players
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<PlayerV1>), StatusCodes.Status200OK)]
        public IActionResult GetAllPlayers([FromQuery] bool includeInactive = false)
        {
            var players = Player.GetAll(true, _database)
                .Where(p => (p.IsActive || includeInactive) && p.Id > 0)
                .OrderByDescending(p => p.NumberOfMatchesPlayedThisSeason)
                .ThenBy(p => !p.IsActive)
                .ThenBy(p => p.Surname)
                .Select(PlayerV1.FromInternal)
                .ToList();

            return Ok(players);
        }

        /// <summary>
        /// Gets a specific player by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PlayerV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPlayer(int id)
        {
            var player = new Player(id, _database);
            if (player.Id == 0)
            {
                return NotFound();
            }
            
            return Ok(PlayerV1.FromInternal(player));
        }

        /// <summary>
        /// Creates a new player
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(PlayerV1), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreatePlayer([FromBody] PlayerV1 playerData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var fullName = string.IsNullOrWhiteSpace(playerData.FirstName) && string.IsNullOrWhiteSpace(playerData.Surname)
                ? "Unknown Player"
                : $"{playerData.FirstName} {playerData.Surname}".Trim();
            
            var player = Player.CreateNewPlayer(fullName, _database);
            UpdatePlayerFields(player, playerData);
            
            var result = PlayerV1.FromInternal(new Player(player.Id, _database));
            return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, result);
        }

        /// <summary>
        /// Updates an existing player
        /// </summary>
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(PlayerV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdatePlayer([FromBody] PlayerV1 playerData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var player = new Player(playerData.PlayerId, _database);
            UpdatePlayerFields(player, playerData);
            
            return Ok(playerData);
        }

        /// <summary>
        /// Deletes a player by ID
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public IActionResult DeletePlayer(int id)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, "Player deletion is not implemented");
        }

        private void UpdatePlayerFields(Player player, PlayerV1 entity)
        {
            player.Nickname = entity.Nickname;
            player.BattingStyle = entity.BattingStyle;
            player.BowlingStyle = entity.BowlingStyle;
            player.IsActive = entity.IsActive;
            player.FirstName = entity.FirstName;
            player.Surname = entity.Surname;
            player.MiddleInitials = entity.MiddleInitials;
            if (entity.ClubConnection != null)
            {
                player.RingerOf = new Player(entity.ClubConnection.PlayerId, _database);
            }
            player.IsRightHandBat = entity.IsRightHandBat;
            player.Save();
        }
    }
}
