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
    public class ResultsController : ControllerBase
    {
        private readonly IDao database;

        public ResultsController(IDao database)
        {
            this.database = database;
        }

        [HttpGet]
        public IActionResult HandleRequest()
        {
            return ProcessRequest();
        }

        public override void ProcessRequest(IHandlerContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    GetResults(context);
                    break;
                default:
                    context.Response.StatusCode = 405;
                    break;
            }
        }

        private void GetResults(IHandlerContext context)
        {
            var queryString = context.Request.QueryString;
            var seasonParam = queryString["season"];

            int season = DateTime.Now.Year;
            if (!string.IsNullOrEmpty(seasonParam) && int.TryParse(seasonParam, out var parsedSeason))
            {
                season = parsedSeason;
            }

            var startDate = new DateTime(season, 1, 1);
            var endDate = new DateTime(season, 12, 31);
            var matches = Match.GetResults(database);

            var filteredMatches = matches
                .Where(m => m.MatchDate >= startDate && m.MatchDate <= endDate)
                .ToList();

            var results = filteredMatches.Select(ResultV1.FromInternal).ToList();

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.Write(JsonSerializer.Serialize(results));
        }
    }
}
