namespace CricketClubMiddle
{
    /// <summary>
    /// Batting scorecard line for an opposition batter in a ball-by-ball innings.
    /// </summary>
    public class OppositionBatterScorecardLine
    {
        public string BatsmanName { get; set; }
        public int Score { get; set; }
        public int BallsFaced { get; set; }
        public int Fours { get; set; }
        public int Sixes { get; set; }
        public decimal StrikeRate { get; set; }
        public CricketClubDomain.OppositionWicket Wicket { get; set; }
    }

    /// <summary>
    /// Bowling figures for one of OUR players bowling in the opposition's ball-by-ball innings.
    /// </summary>
    public class OppositionBowlerDetails
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

