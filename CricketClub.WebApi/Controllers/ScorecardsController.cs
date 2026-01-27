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

            if (unsavedScorecard.OurInnings?.Batting?.Entries?.Any() == true)
            {
                var internalBattingCard = unsavedScorecard.OurInnings.Batting.ToInternalBattingCard(match, ThemOrUs.Us);
                internalBattingCard.Save(BattingOrBowling.Batting);
            }

            if (unsavedScorecard.TheirInnings?.Batting?.Entries?.Any() == true)
            {
                var internalOppoBattingCard = unsavedScorecard.TheirInnings.Batting.ToInternalBattingCard(match, ThemOrUs.Them);
                internalOppoBattingCard.Save(BattingOrBowling.Bowling);
            }

            if (unsavedScorecard.OurInnings?.Batting != null)
            {
                var internalExtras = unsavedScorecard.OurInnings.Batting.ToInternalExtras(match.ID, ThemOrUs.Them);
                internalExtras.Save();
            }

            if (unsavedScorecard.TheirInnings?.Batting != null)
            {
                var internalOppoExtras = unsavedScorecard.TheirInnings.Batting.ToInternalExtras(match.ID, ThemOrUs.Us);
                internalOppoExtras.Save();
            }

            if (unsavedScorecard.OurInnings != null)
            {
                match.OurInningsLength = unsavedScorecard.OurInnings.InningsLength;
            }

            if (unsavedScorecard.TheirInnings != null)
            {
                match.TheirInningsLength = unsavedScorecard.TheirInnings.InningsLength;
            }

            match.Abandoned = unsavedScorecard.MatchConditions.abandoned;
            match.WasDeclaration = unsavedScorecard.MatchConditions.declaration;
            match.Overs = unsavedScorecard.MatchConditions.overs;
            match.Captain = new Player(unsavedScorecard.MatchConditions.captainId, database);
            match.WicketKeeper = new Player(unsavedScorecard.MatchConditions.wicketKeeperId, database);
            match.WonToss = unsavedScorecard.MatchConditions.weWonTheToss;
            match.TossWinnerBatted = unsavedScorecard.MatchConditions.tossWinnerBatted;
            match.Save();

            if (unsavedScorecard.OurInnings?.Bowling?.Entries?.Any() == true)
            {
                var theirBowlingStats = unsavedScorecard.OurInnings.Bowling.ToInternal(match, ThemOrUs.Them);
                theirBowlingStats.Save();
            }

            if (unsavedScorecard.TheirInnings?.Bowling?.Entries?.Any() == true)
            {
                var ourBowlingStats = unsavedScorecard.TheirInnings.Bowling.ToInternal(match, ThemOrUs.Us);
                ourBowlingStats.Save();
            }

            if (unsavedScorecard.OurInnings?.Fow?.Entries?.Any() == true)
            {
                var ourFowData = unsavedScorecard.OurInnings.Fow.ToInternal(match, ThemOrUs.Us);
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
