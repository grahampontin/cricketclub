using System.Collections.Generic;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle.Stats;

namespace CricketClubMiddle
{
    public class LiveScorecard
    {
        public BatsmanInningsDetails OnStrikeBatsman { get; set; }
        public BatsmanInningsDetails OtherBatsman { get; set; }
        public BatsmanInningsDetails LastBatsmanOut { get; set; }
        public string Opposition { get; set; }
        public int OurLastCompletedOver { get; set; }
        public int OversRemaining { get; set; }
        public bool DeclarationGame { get; set; }
        public int Score { get; set; }
        public int Wickets { get; set; }
        public decimal RunRate { get; set; }
        public Partnership CurrentPartnership { get; set; }
        public Partnership PreviousPartnership { get; set; }
        public FallOfWicket LastManOut { get; set; }

        public List<FallOfWicket> FallOfWickets { get; set; }

        public List<OverSummary> CompletedOvers { get; set; }

        public BowlerInningsDetails BowlerOneDetails { get; set; }
        public BowlerInningsDetails BowlerTwoDetails { get; set; }

        public LiveBattingCard LiveBattingCard { get; set; }
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
        public List<OppositionInningsDetails> TheirCompletedOvers { get; set; }
        public bool IsMatchComplete { get; set; }
        public string ResultText { get; set; }
        public string OurInningsCommentary { get; set; }
        public string TheirInningsCommentary { get; set; }
        public List<BowlerInningsDetails> LiveBowlingCard { get; set; }
        public List<Partnership> Partnerships { get; set; }

        /// <summary>
        /// VCC players who are yet to bat in the current innings, ordered by batting position.
        /// </summary>
        public List<CricketClubDomain.PlayerState> YetToBat { get; set; }

        // ── Opposition ball-by-ball innings extras ───────────────────────────────
        /// <summary>True when the opposition innings is in ball-by-ball mode.</summary>
        public bool TheirInningsIsBallByBall { get; set; }
        /// <summary>Over number of the last completed opposition over (ball-by-ball mode).</summary>
        public int TheirLastCompletedOver { get; set; }
        /// <summary>The opposition batter currently on strike (ball-by-ball mode) — full scorecard line.</summary>
        public OppositionBatterScorecardLine TheirOnStrikeBatsman { get; set; }
        /// <summary>The other opposition batter at the crease (ball-by-ball mode) — full scorecard line.</summary>
        public OppositionBatterScorecardLine TheirOtherBatsman { get; set; }
        /// <summary>The last opposition batter to be dismissed (ball-by-ball mode).</summary>
        public OppositionBatterScorecardLine TheirLastBatsmanOut { get; set; }
        /// <summary>Opposition batters yet to bat (ball-by-ball mode).</summary>
        public List<OppositionBatterState> TheirYetToBat { get; set; }
        /// <summary>Batting scorecard lines for each opposition batter who has faced a ball.</summary>
        public List<OppositionBatterScorecardLine> TheirLiveBattingCard { get; set; }
        /// <summary>Bowling figures for each of OUR players who has bowled in the opposition innings.</summary>
        public List<OppositionBowlerDetails> TheirLiveBowlingCard { get; set; }
        /// <summary>Current bowler details (VCC player) for the opposition innings.</summary>
        public OppositionBowlerDetails TheirBowlerOneDetails { get; set; }
        /// <summary>Previous bowler details (VCC player) for the opposition innings.</summary>
        public OppositionBowlerDetails TheirBowlerTwoDetails { get; set; }
        /// <summary>Per-over cumulative summaries for the opposition ball-by-ball innings.</summary>
        public List<Stats.OppositionOverSummary> TheirBallByBallCompletedOvers { get; set; }
        /// <summary>Current batting partnership in the opposition innings.</summary>
        public Stats.OppositionPartnership TheirCurrentPartnership { get; set; }
        /// <summary>Previous batting partnership in the opposition innings.</summary>
        public Stats.OppositionPartnership TheirPreviousPartnership { get; set; }
        /// <summary>All batting partnerships in the opposition innings.</summary>
        public List<Stats.OppositionPartnership> TheirPartnerships { get; set; }
        /// <summary>Fall of wickets for the opposition innings.</summary>
        public List<Stats.OppositionFallOfWicket> TheirFallOfWickets { get; set; }
    }

    public class LiveBattingCard
    {
        public Dictionary<string, LiveBattingCardEntry> Players;
        public LiveExtras Extras;
    }

    public class LiveBattingCardEntry
    {
        public BatsmanInningsDetails BatsmanInningsDetails;
        public Wicket Wicket;
    }

    public class LiveExtras
    {
        public int Byes;
        public int LegByes;
        public int Wides;
        public int NoBalls;
        public int Penalty;
        public int Total => Byes + LegByes + Wides + NoBalls + Penalty;
        public string DetailString => Byes + "b " + LegByes + "lb " + Wides + "wd " + NoBalls + "nb " + Penalty + "p";
    }
}