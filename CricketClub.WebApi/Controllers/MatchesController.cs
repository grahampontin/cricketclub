#nullable disable
using System.Collections.Specialized;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;
using Match = CricketClubMiddle.Match;
using MatchType = CricketClubDomain.MatchType;

namespace CricketClub.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class MatchesController : EntityControllerBase<MatchV1>
    {
        public MatchesController(IDao database) : base(database)
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

        protected override MatchV1 UpdateEntity(MatchV1 entity)
        {
            var match = new Match(entity.Id, database)
            {
                OppositionID = entity.Opposition.Id,
                VenueID = entity.Venue.Id,
                MatchDate = DateTime.Parse(entity.Date),
                HomeOrAway = entity.IsHome ? HomeOrAway.Home : HomeOrAway.Away,
                Type = (MatchType)Enum.Parse(typeof(MatchType), entity.Type, true)
            };
            match.Save();
            return entity;
        }

        protected override void DeleteEntity(int id)
        {
            throw new NotImplementedException();
        }

        protected override MatchV1 CreateEntity(MatchV1 deserializeRequestBody)
        {
            var matchType = (MatchType)Enum.Parse(typeof(MatchType), deserializeRequestBody.Type, true);
            var homeOrAway = deserializeRequestBody.IsHome ? HomeOrAway.Home : HomeOrAway.Away;
            var match = Match.CreateNewMatch(
                new Team(deserializeRequestBody.Opposition.Id, database),
                DateTime.Parse(deserializeRequestBody.Date),
                new Venue(deserializeRequestBody.Venue.Id, database),
                matchType,
                homeOrAway,
                database);
            return MatchV1.FromInternal(match);
        }

        protected override List<MatchV1> GetAllEntities(NameValueCollection requestQueryString)
        {
            var season = requestQueryString["season"];
            
            if (season != null && int.TryParse(season, out var seasonAsInt))
            {
                return Match.GetAll(new DateTime(seasonAsInt, 1, 1), new DateTime(seasonAsInt, 12, 31), null, null, database)
                    .OrderBy(m => m.MatchDate).Select(MatchV1.FromInternal).ToList();
            }

            return Match.GetAll(database)
                .OrderBy(m => m.MatchDate).Select(MatchV1.FromInternal).ToList();

        }

        protected override MatchV1 GetEntity(int id)
        {
            var match = new Match(id, database);
            return MatchV1.FromInternal(match);
        }

        public override string GetTypeName()
        {
            return "matches";
        }
    }
}
