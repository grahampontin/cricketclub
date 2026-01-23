#nullable disable
using System.Text.Json;
using System.Text.RegularExpressions;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using Microsoft.AspNetCore.Mvc;
using Match = CricketClubMiddle.Match;

namespace CricketClub.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchReportsController : ControllerBase
    {
        private readonly IDao database;

        public MatchReportsController(IDao database)
        {
            this.database = database;
        }

        [HttpGet]
        [HttpGet("{id}")]
        [HttpPost("{id}")]
        public IActionResult HandleRequest()
        {
            return ProcessRequest();
        }

        public override void ProcessRequest(IHandlerContext context)
        {
            var matchId = ExtractMatchIdFromUrl(context.Request.Url.ToString());

            switch (context.Request.HttpMethod)
            {
                case "GET":
                    if (matchId == null)
                    {
                        GetAllMatchReports(context);
                    }
                    else
                    {
                        GetMatchReport(context, matchId.Value);
                    }
                    break;
                case "POST":
                    if (matchId == null)
                    {
                        context.Response.ContentType = "text/plain";
                        context.Response.Write("Match ID not specified in URL");
                        context.Response.StatusCode = 400;
                        return;
                    }
                    SaveMatchReport(context, matchId.Value);
                    break;
                default:
                    context.Response.StatusCode = 405;
                    break;
            }
        }

        private int? ExtractMatchIdFromUrl(string url)
        {
            var matchCollection = Regex.Matches(url, "/matchreports/([0-9]+)");
            if (matchCollection.Count == 1)
            {
                return int.Parse(matchCollection[0].Groups[1].Value);
            }
            return null;
        }

        private void GetMatchReport(IHandlerContext context, int matchId)
        {
            var match = new Match(matchId, database);
            var savedReport = match.GetMatchReport();
            var matchReport = new MatchReportV1(savedReport.Conditions, savedReport.Report,
                savedReport.ReportImage);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.Write(JsonSerializer.Serialize(matchReport));
        }

        private void GetAllMatchReports(IHandlerContext context)
        {
            var queryString = context.Request.QueryString;
            var limitParam = queryString["limit"] ?? queryString["count"];
            var orderParam = queryString["order"] ?? queryString["orderBy"];

            int? limit = null;
            bool descending = true;

            if (!string.IsNullOrEmpty(limitParam) && int.TryParse(limitParam, out var parsedLimit))
            {
                limit = parsedLimit;
            }

            if (!string.IsNullOrEmpty(orderParam))
            {
                descending = orderParam.ToLower() == "desc" || orderParam.ToLower() == "descending";
            }

            var matches = Match.GetResults(database);
            
            var orderedMatches = descending 
                ? matches.OrderByDescending(m => m.MatchDate) 
                : matches.OrderBy(m => m.MatchDate);

            var matchReports = orderedMatches
                .Select(m => new { Match = m, Report = m.GetMatchReport() })
                .Where(mr => mr.Report != MatchReportAndConditions.None && !string.IsNullOrEmpty(mr.Report.Report))
                .Select(mr => MatchReportListItemV1.FromInternal(mr.Match, mr.Report));

            if (limit.HasValue)
            {
                matchReports = matchReports.Take(limit.Value);
            }

            var result = matchReports.ToList();

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.Write(JsonSerializer.Serialize(result));
        }

        private void SaveMatchReport(IHandlerContext context, int matchId)
        {
            var stringReader = new StreamReader(context.Request.InputStream);
            string postData = stringReader.ReadToEnd();
            var report = JsonSerializer.Deserialize<MatchReportV1>(postData);
            
            var match = new Match(matchId, database);
            match.CreateOrUpdateMatchReport(report.Conditions, report.Report, report.Base64EncodedImage);
            
            var updatedReport = match.GetMatchReport();
            var updatedMatchReport = new MatchReportV1(updatedReport.Conditions, updatedReport.Report,
                updatedReport.ReportImage);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.Write(JsonSerializer.Serialize(updatedMatchReport));
        }
    }
}
