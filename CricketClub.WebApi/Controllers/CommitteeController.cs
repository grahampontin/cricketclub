#nullable disable
using System.Collections.Specialized;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class CommitteeController : EntityControllerBase<CommitteePostV1>
    {
        public CommitteeController(IDao database) : base(database)
        {
        }

        [HttpGet]
        [HttpGet("{id}")]
        [HttpPost]
        [HttpPut]
        [HttpDelete("{id}")]
        public IActionResult HandleRequest()
        {
            return ProcessRequest();
        }

        protected override CommitteePostV1 UpdateEntity(CommitteePostV1 entity)
        {
            database.UpdateCommittee(CommitteePostV1.ToInternal(entity));
            return entity;
        }

        protected override void DeleteEntity(int id)
        {
            database.DeleteCommittee(id);
        }

        protected override CommitteePostV1 CreateEntity(CommitteePostV1 deserializeRequestBody)
        {
            var createdId = database.CreateNewCommittee(CommitteePostV1.ToInternal(deserializeRequestBody));
            return CommitteePostV1.ToExternal(database.GetCommitteeData(createdId));
        }

        protected override List<CommitteePostV1> GetAllEntities(NameValueCollection requestQueryString)
        {
            var season = requestQueryString["season"] ?? requestQueryString["year"];
            var allEntities = database.GetAllCommitteeData().Select(CommitteePostV1.ToExternal).ToList();
            if (season != null && int.TryParse(season, out var seasonAsInt))
            {
                allEntities = allEntities.Where(a => a.Year == seasonAsInt).ToList();
            }

            return allEntities;
        }

        protected override CommitteePostV1 GetEntity(int id)
        {
            var committeeData = database.GetCommitteeData(id);
            return committeeData == null ? null : CommitteePostV1.ToExternal(committeeData);
        }

        public override string GetTypeName()
        {
            return "committee";
        }
    }
}
