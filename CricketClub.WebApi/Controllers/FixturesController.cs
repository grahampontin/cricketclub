#nullable disable
using System.Text.Json;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FixturesController : ControllerBase
    {
        private readonly IDao database;

        public FixturesController(IDao database)
        {
            this.database = database;
        }

        [HttpGet]
        public async Task<IActionResult> HandleRequest()
        {
            return await ProcessRequest();
        }

        public override void ProcessRequest(IHandlerContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    GetFixtures(context);
                    break;
                default:
                    context.Response.StatusCode = 405;
                    break;
            }
        }

        private void GetFixtures(IHandlerContext context)
        {
            var queryString = context.Request.QueryString;
            var seasonParam = queryString["season"];

            var matches = Match.GetFixtures(database);

            if (!string.IsNullOrEmpty(seasonParam) && int.TryParse(seasonParam, out var season))
            {
                var startDate = new DateTime(season, 1, 1);
                var endDate = new DateTime(season, 12, 31);
                matches = matches.Where(m => m.MatchDate >= startDate && m.MatchDate <= endDate).ToList();
            }

            var fixtures = matches.Select(MatchV1.FromInternal).ToList();

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.Write(JsonSerializer.Serialize(fixtures));
        }
    }
}
