#nullable disable
using System.Collections.Specialized;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class AwardsController : EntityControllerBase<AwardV1>
    {
        public AwardsController(IDao database) : base(database)
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

        protected override AwardV1 UpdateEntity(AwardV1 entity)
        {
            database.UpdateAward(AwardV1.ToInternal(entity));
            return entity;
        }

        protected override void DeleteEntity(int id)
        {
            database.DeleteAward(id);
        }

        protected override AwardV1 CreateEntity(AwardV1 deserializeRequestBody)
        {
            var createdId = Utils.ParseEnumOrThrow<Award, int>(deserializeRequestBody.Award,
                parsed => database.CreateNewAward(
                    parsed,
                    deserializeRequestBody.Year,
                    deserializeRequestBody.PlayerId,
                    deserializeRequestBody.Data
                ));
            
            return AwardV1.FromInternal(database.GetAwardData(createdId));
        }

        protected override List<AwardV1> GetAllEntities(NameValueCollection requestQueryString)
        {
            var season = requestQueryString["season"];
            var allEntities = database.GetAllAwardsData().Select(AwardV1.FromInternal).ToList();
            if (season != null && int.TryParse(season, out var seasonAsInt))
            {
                allEntities = allEntities.Where(a => a.Year == seasonAsInt).ToList();
            }

            return allEntities;
        }

        protected override AwardV1 GetEntity(int id)
        {
            var awardData = database.GetAwardData(id);
            return awardData == null ? null : AwardV1.FromInternal(awardData);
        }

        public override string GetTypeName()
        {
            return "awards";
        }
    }
}
