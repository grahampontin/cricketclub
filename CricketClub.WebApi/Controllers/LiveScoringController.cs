#nullable disable
using System.Text.Json;
using System.Text.RegularExpressions;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using CricketClubMiddle.Stats;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Match = CricketClubMiddle.Match;

namespace CricketClub.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LiveScoringController : ControllerBase
    {
        private readonly IDao database;
        private static readonly ILog Log = LogManager.GetLogger(typeof(LiveScoringController));

        public LiveScoringController(IDao database)
        {
            this.database = database;
        }

        [HttpGet("matches")]
        [HttpGet("{matchId}/scorecard")]
        [HttpGet("{matchId}")]
        [HttpPost("{matchId}/start")]
        [HttpPost("{matchId}/over")]
        [HttpPost("{matchId}/opposition-score")]
        [HttpPost("{matchId}/end-innings")]
        [HttpDelete("{matchId}/last-over")]
        [HttpDelete("{matchId}/reset")]
        [HttpPost("{matchId}/force-end")]
        public IActionResult HandleRequest()
        {
            return ProcessRequest();
        }

        public override void ProcessRequest(IHandlerContext context)
        {
            try
            {
                var path = context.Request.Url.AbsolutePath.ToLower();
                var method = context.Request.HttpMethod.ToUpper();

                if (Regex.IsMatch(path, @"^/api/livescoring/matches/?$", RegexOptions.IgnoreCase) && method == "GET")
                {
                    HandleListMatches(context);
                }
                else if (Regex.IsMatch(path, @"^/api/livescoring/\d+/scorecard/?$", RegexOptions.IgnoreCase) && method == "GET")
                {
                    var matchId = ExtractMatchId(path, @"^/api/livescoring/(\d+)/scorecard");
                    HandleLiveScorecard(context, matchId);
                }
                else if (Regex.IsMatch(path, @"^/api/livescoring/\d+/?$", RegexOptions.IgnoreCase) && method == "GET")
                {
                    var matchId = ExtractMatchId(path, @"^/api/livescoring/(\d+)");
                    HandleMatchState(context, matchId);
                }
                else if (Regex.IsMatch(path, @"^/api/livescoring/\d+/start/?$", RegexOptions.IgnoreCase) && method == "POST")
                {
                    var matchId = ExtractMatchId(path, @"^/api/livescoring/(\d+)/start");
                    HandleStartMatch(context, matchId);
                }
                else if (Regex.IsMatch(path, @"^/api/livescoring/\d+/over/?$", RegexOptions.IgnoreCase) && method == "POST")
                {
                    var matchId = ExtractMatchId(path, @"^/api/livescoring/(\d+)/over");
                    HandleSubmitOver(context, matchId);
                }
                else if (Regex.IsMatch(path, @"^/api/livescoring/\d+/opposition-score/?$", RegexOptions.IgnoreCase) && method == "POST")
                {
                    var matchId = ExtractMatchId(path, @"^/api/livescoring/(\d+)/opposition-score");
                    HandleUpdateOppositionScore(context, matchId);
                }
                else if (Regex.IsMatch(path, @"^/api/livescoring/\d+/end-innings/?$", RegexOptions.IgnoreCase) && method == "POST")
                {
                    var matchId = ExtractMatchId(path, @"^/api/livescoring/(\d+)/end-innings");
                    HandleEndInnings(context, matchId);
                }
                else if (Regex.IsMatch(path, @"^/api/livescoring/\d+/last-over/?$", RegexOptions.IgnoreCase) && method == "DELETE")
                {
                    var matchId = ExtractMatchId(path, @"^/api/livescoring/(\d+)/last-over");
                    HandleDeleteLastOver(context, matchId);
                }
                else if (Regex.IsMatch(path, @"^/api/livescoring/\d+/reset/?$", RegexOptions.IgnoreCase) && method == "DELETE")
                {
                    var matchId = ExtractMatchId(path, @"^/api/livescoring/(\d+)/reset");
                    HandleResetMatch(context, matchId);
                }
                else if (Regex.IsMatch(path, @"^/api/livescoring/\d+/force-end/?$", RegexOptions.IgnoreCase) && method == "POST")
                {
                    var matchId = ExtractMatchId(path, @"^/api/livescoring/(\d+)/force-end");
                    HandleForceEndMatch(context, matchId);
                }
                else
                {
                    context.Response.StatusCode = 404;
                    context.Response.ContentType = "text/plain";
                    context.Response.Write("Not Found");
                }
            }
            catch (ArgumentException ex)
            {
                Log.Error("Bad request error in LiveScoringController", ex);
                context.Response.StatusCode = 400;
                context.Response.ContentType = "text/plain";
                context.Response.Write(ex.Message);
                context.Response.End();
            }
            catch (Exception ex)
            {
                Log.Error("Error in LiveScoringController", ex);
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain";
                context.Response.Write(ex.Message + Environment.NewLine + ex.StackTrace);
                context.Response.End();
            }
        }

        private int ExtractMatchId(string path, string pattern)
        {
            var match = Regex.Match(path, pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                throw new ArgumentException("Invalid match ID in URL");
            }
            return int.Parse(match.Groups[1].Value);
        }

        private void HandleListMatches(IHandlerContext context)
        {
            var seasonParam = context.Request.QueryString["season"];
            
            if (seasonParam != null && int.TryParse(seasonParam, out var season))
            {
                var matchDescriptors = Match.GetAll(new DateTime(season, 1, 1), new DateTime(season, 12, 31), null, null, database)
                    .OrderBy(m => m.MatchDate).Select(MatchV1.FromInternal).ToList();
                WriteJsonResponse(context, matchDescriptors);
            }
            else
            {
                var matchDescriptors = Match.GetInProgressGames()
                    .Union(Match.GetFixtures().Where(m =>
                        m.MatchDate < DateTime.Today.AddDays(14) &&
                        !m.GetCurrentBallByBallState().IsMatchComplete()))
                    .Select(m => new BallByBallMatchDescriptor(m))
                    .Distinct(BallByBallMatchDescriptor.MatchIdComparer).ToList();
                WriteJsonResponse(context, matchDescriptors);
            }
        }

        private void HandleMatchState(IHandlerContext context, int matchId)
        {
            var match = new Match(matchId, database);
            ReturnCurrentMatchState(context, match);
        }

        private void HandleLiveScorecard(IHandlerContext context, int matchId)
        {
            var match = new Match(matchId, database);
            var liveScorecard = FromLiveScorecard(match);
            WriteJsonResponse(context, liveScorecard);
        }

        private void HandleStartMatch(IHandlerContext context, int matchId)
        {
            var match = new Match(matchId, database);
            if (match.GetIsBallByBallInProgress())
            {
                throw new ArgumentException("Coverage for match vs " + match.Opposition.Name + " has already been started");
            }

            var matchConditions = DeserializeRequestBody<BallByBallMatchConditions>(context);
            match.StartBallByBallCoverage(matchConditions);
            ReturnCurrentMatchState(context, match);
        }

        private void HandleSubmitOver(IHandlerContext context, int matchId)
        {
            var match = new Match(matchId, database);
            var stateFromClient = DeserializeRequestBody<MatchState>(context);
            match.UpdateCurrentBallByBallState(stateFromClient);
            ReturnCurrentMatchState(context, match);
        }

        private void HandleUpdateOppositionScore(IHandlerContext context, int matchId)
        {
            var match = new Match(matchId, database);
            var incoming = DeserializeRequestBody<OppositionInningsDetails>(context);
            match.UpdateOppositionScore(incoming);
            ReturnCurrentMatchState(context, match);
        }

        private void HandleEndInnings(IHandlerContext context, int matchId)
        {
            var match = new Match(matchId, database);
            var inningsEndDetails = DeserializeRequestBody<InningsEndDetails>(context);
            match.EndInnings(inningsEndDetails);
            ReturnCurrentMatchState(context, match);
        }

        private void HandleDeleteLastOver(IHandlerContext context, int matchId)
        {
            var match = new Match(matchId, database);
            match.DeleteLastBallByBallOver();
            ReturnCurrentMatchState(context, match);
        }

        private void HandleResetMatch(IHandlerContext context, int matchId)
        {
            var match = new Match(matchId, database);
            match.ResetBallByBallData();
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 204;
        }

        private void HandleForceEndMatch(IHandlerContext context, int matchId)
        {
            var match = new Match(matchId, database);
            var nextInnings = EndInnings(match, match.GetCurrentBallByBallState().GetInningsStatus().OurInningsStatus == InningsStatus.InProgress
                ? "Batting"
                : "Bowling");
            match = new Match(matchId, database);
            switch (nextInnings)
            {
                case NextInnings.Batting:
                    EndInnings(match, "Batting");
                    break;
                case NextInnings.Bowling:
                    EndInnings(match, "Bowling");
                    break;
                case NextInnings.GameOver:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            context.Response.StatusCode = 204;
        }

        private static NextInnings EndInnings(Match match, string inningsType)
        {
            return match.EndInnings(new InningsEndDetails()
            {
                Commentary = "",
                InningsType = inningsType,
                WasDeclared = false
            });
        }

        private LiveScorecardV1 FromLiveScorecard(Match match)
        {
            var matchReportAndConditions = match.GetMatchReport();
            var external = new LiveScorecardV1
            {
                MatchData = MatchV1.FromInternal(match),
                InPlayData = match.GetLiveScorecard(),
                FinalScorecard = MatchScorecardV1.GetExternalScorecard(match),
                MatchReport = new MatchReportV1(matchReportAndConditions.Conditions,
                    matchReportAndConditions.Report, matchReportAndConditions.ReportImage),
                Result = ResultV1.FromInternal(match)
            };
            return external;
        }

        private void ReturnCurrentMatchState(IHandlerContext context, Match match)
        {
            BallByBallMatch ballByBallMatch = match.GetCurrentBallByBallState();
            MatchState matchState = ballByBallMatch.GetMatchState();
            var matchStateV1 = MatchStateMapper.MapToMatchStateV1(matchState);
            matchStateV1.LiveScorecard = FromLiveScorecard(match);
            WriteJsonResponse(context, matchStateV1);
        }

        private T DeserializeRequestBody<T>(IHandlerContext context)
        {
            var stringReader = new StreamReader(context.Request.InputStream);
            string body = stringReader.ReadToEnd();
            return JsonSerializer.Deserialize<T>(body);
        }

        private void WriteJsonResponse(IHandlerContext context, object data)
        {
            string json = JsonSerializer.Serialize(data);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.Write(json);
        }
    }
}
