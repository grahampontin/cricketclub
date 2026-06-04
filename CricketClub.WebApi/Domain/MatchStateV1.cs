namespace CricketClub.WebApi.Domain
{
    public class MatchStateV1
    {
        public int LastCompletedOver { get; set; }
        public int OnStrikeBatsmanId { get; set; }
        public OverV1 Over { get; set; }
        public PlayerStateV1[] Players { get; set; }
        public decimal RunRate { get; set; }
        public int Score { get; set; }
        public string[] Bowlers { get; set; }
        public int MatchId { get; set; }
        public string PreviousBowler { get; set; }
        public string PreviousBowlerButOne { get; set; }
        public PartnershipStubV1 Partnership { get; set; }
        public string NextState { get; set; }
        public int OppositionScore { get; set; }
        public int OppositionWickets { get; set; }
        public string OppositionName { get; set; }
        public string OppositionShortName { get; set; }
        public BowlerInningsDetailsV1[] BowlerDetails { get; set; }
        public LiveScorecardV1 LiveScorecard { get; set; }

        // ── Opposition ball-by-ball extras ───────────────────────────────────────
        /// <summary>True when the opposition innings is being scored ball-by-ball.</summary>
        public bool TheirInningsIsBallByBall { get; set; }
        /// <summary>Current batter states for the opposition (ball-by-ball mode only).</summary>
        public OppositionBatterStateV1[] OppositionPlayers { get; set; }
        /// <summary>Name of the opposition batter currently on strike (ball-by-ball mode only).</summary>
        public string OppositionOnStrikeBatsmanName { get; set; }
        /// <summary>Over number of the last completed opposition over (ball-by-ball mode only).</summary>
        public int OppositionLastCompletedOver { get; set; }
    }
}