#nullable disable
using System.Text.Json;
using System.Text.RegularExpressions;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using CricketClubMiddle.Stats;
using Microsoft.AspNetCore.Mvc;
using Match = CricketClubMiddle.Match;

namespace CricketClub.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScorecardsController : ControllerBase
    {
        private readonly IDao database;

        public ScorecardsController(IDao database)
        {
            this.database = database;
        }

        [HttpGet("{id}")]
        [HttpPost("{id}")]
        public async Task<IActionResult> HandleRequest()
        {
            return await ProcessRequest();
        }

        public override void ProcessRequest(IHandlerContext context)
        {
            var matchId = ExtractMatchIdFromUrl(context.Request.Url.ToString());
            if (matchId == null)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Match ID not specified in URL");
                context.Response.StatusCode = 400;
                return;
            }

            switch (context.Request.HttpMethod)
            {
                case "GET":
                    GetScorecard(context, matchId.Value);
                    break;
                case "POST":
                    SaveScorecard(context, matchId.Value);
                    break;
                default:
                    context.Response.StatusCode = 405;
                    break;
            }
        }

        private int? ExtractMatchIdFromUrl(string url)
        {
            var matchCollection = Regex.Matches(url, "/scorecards/([0-9]+)");
            if (matchCollection.Count == 1)
            {
                return int.Parse(matchCollection[0].Groups[1].Value);
            }
            return null;
        }

        private void GetScorecard(IHandlerContext context, int matchId)
        {
            var match = new Match(matchId, database);
            var scorecard = MatchScorecardV1.GetExternalScorecard(match);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.Write(JsonSerializer.Serialize(scorecard));
        }

        private void SaveScorecard(IHandlerContext context, int matchId)
        {
            var stringReader = new StreamReader(context.Request.InputStream);
            string postData = stringReader.ReadToEnd();
            var unsavedScorecard = JsonSerializer.Deserialize<MatchScorecardV1>(postData);
            
            var match = new Match(matchId, database);

            if (unsavedScorecard.ourInnings.batting.entries.Any())
            {
                var internalBattingCard =
                    unsavedScorecard.ourInnings.batting.ToInternalBattingCard(match, ThemOrUs.Us);
                internalBattingCard.Save(BattingOrBowling.Batting);
            }

            if (unsavedScorecard.theirInnings.batting.entries.Any())
            {
                var internalOppoBattingCard =
                    unsavedScorecard.theirInnings.batting.ToInternalBattingCard(match, ThemOrUs.Them);
                internalOppoBattingCard.Save(BattingOrBowling.Bowling);
            }

            var internalExtras =
                unsavedScorecard.ourInnings.batting.ToInternalExtras(match.ID, ThemOrUs.Them);
            internalExtras.Save();

            var internalOppoExtras =
                unsavedScorecard.theirInnings.batting.ToInternalExtras(match.ID, ThemOrUs.Us);
            internalOppoExtras.Save();

            match.OurInningsLength = unsavedScorecard.ourInnings.inningsLength;
            match.TheirInningsLength = unsavedScorecard.theirInnings.inningsLength;
            match.Abandoned = unsavedScorecard.matchConditions.abandoned;
            match.WasDeclaration = unsavedScorecard.matchConditions.declaration;
            match.Overs = unsavedScorecard.matchConditions.overs;
            match.Captain = new Player(unsavedScorecard.matchConditions.captainId);
            match.WicketKeeper = new Player(unsavedScorecard.matchConditions.wicketKeeperId);
            match.WonToss = unsavedScorecard.matchConditions.weWonTheToss;
            match.TossWinnerBatted = unsavedScorecard.matchConditions.tossWinnerBatted;
            match.Save();

            if (unsavedScorecard.ourInnings.bowling.entries.Any())
            {
                var theirBowlingStats =
                    unsavedScorecard.ourInnings.bowling.ToInternal(match, ThemOrUs.Them);
                theirBowlingStats.Save();
            }

            if (unsavedScorecard.theirInnings.bowling.entries.Any())
            {
                var ourBowlingStats =
                    unsavedScorecard.theirInnings.bowling.ToInternal(match, ThemOrUs.Us);
                ourBowlingStats.Save();
            }

            if (unsavedScorecard.ourInnings.fow.entries.Any())
            {
                var ourFowData = unsavedScorecard.ourInnings.fow.ToInternal(match, ThemOrUs.Us);
                ourFowData.Save();
            }

            var savedScorecard = new MatchScorecardV1(match.GetOurBattingScoreCard(),
                match.GetThierBowlingStats(), new FoWStats(match.ID, ThemOrUs.Us),
                match.GetTheirBattingScoreCard(), match.GetOurBowlingStats(),
                new FoWStats(match.ID, ThemOrUs.Them), new Extras(match.ID, ThemOrUs.Them),
                new Extras(match.ID, ThemOrUs.Us), match);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.Write(JsonSerializer.Serialize(savedScorecard));
        }
    }
}
