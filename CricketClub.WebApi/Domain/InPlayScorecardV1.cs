using System.Collections.Generic;

namespace CricketClub.WebApi.Domain
{
    public class InPlayScorecardV1
    {
        public BatsmanInningsDetailsV1 OnStrikeBatsman { get; set; }
        public BatsmanInningsDetailsV1 OtherBatsman { get; set; }
        public BatsmanInningsDetailsV1 LastBatsmanOut { get; set; }
        public string Opposition { get; set; }
        public int OurLastCompletedOver { get; set; }
        public int OversRemaining { get; set; }
        public bool DeclarationGame { get; set; }
        public int Score { get; set; }
        public int Wickets { get; set; }
        public decimal RunRate { get; set; }
        public PartnershipV1 CurrentPartnership { get; set; }
        public PartnershipV1 PreviousPartnership { get; set; }
        public FallOfWicketV1 LastManOut { get; set; }
        public List<FallOfWicketV1> FallOfWickets { get; set; }
        public List<OverSummaryV1> CompletedOvers { get; set; }
        public BowlerInningsDetailsV1 BowlerOneDetails { get; set; }
        public BowlerInningsDetailsV1 BowlerTwoDetails { get; set; }
        public LiveBattingCardV1 LiveBattingCard { get; set; }
        public int Overs { get; set; }
        public bool TossWinnerBatted { get; set; }
        public bool WonToss { get; set; }
        public string OurInningsStatus { get; set; }
        public string TheirInningsStatus { get; set; }
        public int TheirScore { get; set; }
        public int TheirWickets { get; set; }
        public int TheirOver { get; set; }
        public decimal TheirRunRate { get; set; }
        public bool IsFirstInnings { get; set; }
        public List<OppositionInningsDetailsV1> TheirCompletedOvers { get; set; }
        public bool IsMatchComplete { get; set; }
        public string ResultText { get; set; }
        public string OurInningsCommentary { get; set; }
        public string TheirInningsCommentary { get; set; }
        public List<BowlerInningsDetailsV1> LiveBowlingCard { get; set; }
        public List<PartnershipV1> Partnerships { get; set; }

        /// <summary>
        /// VCC players who have not yet come to the crease, in batting order.
        /// </summary>
        public List<YetToBatEntryV1> YetToBat { get; set; }

        // ── Opposition ball-by-ball innings (populated when TheirInningsIsBallByBall = true) ──
        /// <summary>True when their innings is being scored ball-by-ball rather than per-over summary.</summary>
        public bool TheirInningsIsBallByBall { get; set; }
        /// <summary>The opposition batter currently on strike — full scorecard line.</summary>
        public OppositionBatterScorecardLineV1 TheirOnStrikeBatsman { get; set; }
        /// <summary>The other opposition batter at the crease — full scorecard line.</summary>
        public OppositionBatterScorecardLineV1 TheirOtherBatsman { get; set; }
        /// <summary>The last opposition batter to be dismissed.</summary>
        public OppositionBatterScorecardLineV1 TheirLastBatsmanOut { get; set; }
        /// <summary>Opposition batters yet to bat (ball-by-ball mode).</summary>
        public List<OppositionBatterStateV1> TheirYetToBat { get; set; }
        /// <summary>Live batting card for all opposition batters who have faced a ball.</summary>
        public List<OppositionBatterScorecardLineV1> TheirLiveBattingCard { get; set; }
        /// <summary>Bowling figures for each of OUR players who has bowled in the opposition innings.</summary>
        public List<OppositionBowlerDetailsV1> TheirLiveBowlingCard { get; set; }
        /// <summary>Over number of the last completed opposition over (ball-by-ball mode).</summary>
        public int TheirLastCompletedOver { get; set; }
        /// <summary>The VCC bowler who bowled the most recent completed opposition over.</summary>
        public OppositionBowlerDetailsV1 TheirBowlerOneDetails { get; set; }
        /// <summary>The previous distinct VCC bowler in the opposition innings.</summary>
        public OppositionBowlerDetailsV1 TheirBowlerTwoDetails { get; set; }
        /// <summary>Per-over cumulative summaries for the opposition ball-by-ball innings.</summary>
        public List<OppositionOverSummaryV1> TheirBallByBallCompletedOvers { get; set; }
        /// <summary>Current batting partnership in the opposition innings.</summary>
        public OppositionPartnershipV1 TheirCurrentPartnership { get; set; }
        /// <summary>Previous batting partnership in the opposition innings.</summary>
        public OppositionPartnershipV1 TheirPreviousPartnership { get; set; }
        /// <summary>All batting partnerships in the opposition innings.</summary>
        public List<OppositionPartnershipV1> TheirPartnerships { get; set; }
        /// <summary>Fall of wickets for the opposition innings.</summary>
        public List<OppositionFallOfWicketV1> TheirFallOfWickets { get; set; }
    }

    /// <summary>Batting scorecard line for an opposition batter (ball-by-ball mode).</summary>
    public class OppositionBatterScorecardLineV1
    {
        public string BatsmanName { get; set; }
        public int Score { get; set; }
        public int BallsFaced { get; set; }
        public int Fours { get; set; }
        public int Sixes { get; set; }
        public decimal StrikeRate { get; set; }
        public OppositionWicketV1 Wicket { get; set; }
    }

    /// <summary>Bowling figures for one of OUR players in the opposition innings (ball-by-ball mode).</summary>
    public class OppositionBowlerDetailsV1
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int Overs { get; set; }
        public int Maidens { get; set; }
        public int Runs { get; set; }
        public int Wickets { get; set; }
        public int Wides { get; set; }
        public int NoBalls { get; set; }
        public decimal Economy { get; set; }
    }
}
