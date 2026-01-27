using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using CricketClubMiddle.Stats;
using Microsoft.AspNetCore.Mvc;
using Match = CricketClubMiddle.Match;

namespace CricketClub.WebApi.Controllers
{
    /// <summary>
    /// Provides read/write access to match scorecards.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ScorecardsController : ControllerBase
    {
        private readonly IDao database;

        public ScorecardsController(IDao database)
        {
            this.database = database;
        }

        /// <summary>
        /// Get the full scorecard for a match.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MatchScorecardV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetScorecard([FromRoute] int id)
        {
            var match = new Match(id, database);
            // If match doesn't exist, underlying match data will be null and many properties will throw.
            // Best-effort: treat "missing" match data as 404.
            if (match.ID == 0)
            {
                return NotFound();
            }

            var scorecard = MatchScorecardV1.GetExternalScorecard(match);
            return Ok(scorecard);
        }

        /// <summary>
        /// Save the scorecard for a match.
        /// </summary>
        [HttpPost("{id:int}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(MatchScorecardV1), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult SaveScorecard([FromRoute] int id, [FromBody] MatchScorecardV1 unsavedScorecard)
        {
            if (unsavedScorecard == null)
            {
                return BadRequest("Request body was empty or invalid JSON");
            }

            var match = new Match(id, database);
            if (match.ID == 0)
            {
                return NotFound();
            }

            if (unsavedScorecard.OurInnings?.batting?.entries?.Any() == true)
            {
                var internalBattingCard = unsavedScorecard.OurInnings.batting.ToInternalBattingCard(match, ThemOrUs.Us);
                internalBattingCard.Save(BattingOrBowling.Batting);
            }

            if (unsavedScorecard.TheirInnings?.batting?.entries?.Any() == true)
            {
                var internalOppoBattingCard = unsavedScorecard.TheirInnings.batting.ToInternalBattingCard(match, ThemOrUs.Them);
                internalOppoBattingCard.Save(BattingOrBowling.Bowling);
            }

            if (unsavedScorecard.OurInnings?.batting != null)
            {
                var internalExtras = unsavedScorecard.OurInnings.batting.ToInternalExtras(match.ID, ThemOrUs.Them);
                internalExtras.Save();
            }

            if (unsavedScorecard.TheirInnings?.batting != null)
            {
                var internalOppoExtras = unsavedScorecard.TheirInnings.batting.ToInternalExtras(match.ID, ThemOrUs.Us);
                internalOppoExtras.Save();
            }

            if (unsavedScorecard.OurInnings != null)
            {
                match.OurInningsLength = unsavedScorecard.OurInnings.inningsLength;
            }

            if (unsavedScorecard.TheirInnings != null)
            {
                match.TheirInningsLength = unsavedScorecard.TheirInnings.inningsLength;
            }

            match.Abandoned = unsavedScorecard.MatchConditions.abandoned;
            match.WasDeclaration = unsavedScorecard.MatchConditions.declaration;
            match.Overs = unsavedScorecard.MatchConditions.overs;
            match.Captain = new Player(unsavedScorecard.MatchConditions.captainId, database);
            match.WicketKeeper = new Player(unsavedScorecard.MatchConditions.wicketKeeperId, database);
            match.WonToss = unsavedScorecard.MatchConditions.weWonTheToss;
            match.TossWinnerBatted = unsavedScorecard.MatchConditions.tossWinnerBatted;
            match.Save();

            if (unsavedScorecard.OurInnings?.bowling?.entries?.Any() == true)
            {
                var theirBowlingStats = unsavedScorecard.OurInnings.bowling.ToInternal(match, ThemOrUs.Them);
                theirBowlingStats.Save();
            }

            if (unsavedScorecard.TheirInnings?.bowling?.entries?.Any() == true)
            {
                var ourBowlingStats = unsavedScorecard.TheirInnings.bowling.ToInternal(match, ThemOrUs.Us);
                ourBowlingStats.Save();
            }

            if (unsavedScorecard.OurInnings?.fow?.entries?.Any() == true)
            {
                var ourFowData = unsavedScorecard.OurInnings.fow.ToInternal(match, ThemOrUs.Us);
                ourFowData.Save();
            }

            var savedScorecard = new MatchScorecardV1(
                match.GetOurBattingScoreCard(),
                match.GetThierBowlingStats(),
                new FoWStats(match.ID, ThemOrUs.Us, database),
                match.GetTheirBattingScoreCard(),
                match.GetOurBowlingStats(),
                new FoWStats(match.ID, ThemOrUs.Them, database),
                new Extras(match.ID, ThemOrUs.Them, database),
                new Extras(match.ID, ThemOrUs.Us, database),
                match);

            return Ok(savedScorecard);
        }
    }
}
