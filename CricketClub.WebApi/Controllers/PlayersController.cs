#nullable disable
using System.Collections.Specialized;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class PlayersController : EntityControllerBase<PlayerV1>
    {
        public PlayersController(IDao database) : base(database)
        {
        }

        [HttpGet]
        [HttpGet("{id}")]
        [HttpPost]
        [HttpPut]
        [HttpDelete("{id}")]
        public async Task<IActionResult> HandleRequest()
        {
            return await ProcessRequest();
        }

        protected override PlayerV1 UpdateEntity(PlayerV1 entity)
        {
            var player = new Player(entity.playerId, database);
            UpdatePlayerFields(player, entity);
            return entity;
        }

        protected override void DeleteEntity(int id)
        {
            throw new NotImplementedException();
        }

        protected override PlayerV1 CreateEntity(PlayerV1 deserializeRequestBody)
        {
            var fullName = string.IsNullOrWhiteSpace(deserializeRequestBody.firstName) && string.IsNullOrWhiteSpace(deserializeRequestBody.surname)
                ? "Unknown Player"
                : $"{deserializeRequestBody.firstName} {deserializeRequestBody.surname}".Trim();
            var player = Player.CreateNewPlayer(fullName, database);
            UpdatePlayerFields(player, deserializeRequestBody);
            return PlayerV1.FromInternal(new Player(player.Id, database));
        }

        private void UpdatePlayerFields(Player player, PlayerV1 entity)
        {
            player.Nickname = entity.nickname;
            player.BattingStyle = entity.battingStyle;
            player.BowlingStyle = entity.bowlingStyle;
            player.IsActive = entity.isActive;
            player.FirstName = entity.firstName;
            player.Surname = entity.surname;
            player.MiddleInitials = entity.middleInitials;
            if (entity.clubConnection != null)
            {
                player.RingerOf = new Player(entity.clubConnection.playerId, database);
            }
            player.IsRightHandBat = entity.isRightHandBat;
            player.Save();
        }

        protected override List<PlayerV1> GetAllEntities(NameValueCollection requestQueryString)
        {
            var inactiveQueryParam = requestQueryString.GetValues("includeInactive");
            var includeInactive = inactiveQueryParam != null && inactiveQueryParam.Length > 0 &&
                                  inactiveQueryParam[0] == "true";

            return Player.GetAll(true, database).Where(p => (p.IsActive || includeInactive) && p.Id > 0)
                .OrderByDescending(p => p.NumberOfMatchesPlayedThisSeason)
                .ThenBy(p => !p.IsActive)
                .ThenBy(p => p.Surname)
                .Select(PlayerV1.FromInternal).ToList();
        }

        protected override PlayerV1 GetEntity(int id)
        {
            throw new NotImplementedException();
        }

        public override string GetTypeName()
        {
            return "players";
        }
    }
}
