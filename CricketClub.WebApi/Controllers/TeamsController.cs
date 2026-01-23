#nullable disable
using System.Collections.Specialized;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class TeamsController : EntityControllerBase<TeamV1>
    {
        public TeamsController(IDao database) : base(database)
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

        protected override TeamV1 UpdateEntity(TeamV1 entity)
        {
            var team = new Team(entity.Id, database) { Name = entity.Name };
            team.Save();
            return entity;
        }

        protected override void DeleteEntity(int id)
        {
            throw new NotImplementedException();
        }

        protected override TeamV1 CreateEntity(TeamV1 deserializeRequestBody)
        {
            var team = Team.CreateNewTeam(deserializeRequestBody.Name, database);
            return TeamV1.FromInternal(team);
        }

        protected override List<TeamV1> GetAllEntities(NameValueCollection requestQueryString)
        {
            return Team.GetAll(database).Where(t => !t.IsUs)
                .Select(TeamV1.FromInternal).OrderBy(t => t.Name).ToList();
        }

        protected override TeamV1 GetEntity(int id)
        {
            var team = new Team(id, database);
            return TeamV1.FromInternal(team);
        }

        public override string GetTypeName()
        {
            return "teams";
        }
    }
}
