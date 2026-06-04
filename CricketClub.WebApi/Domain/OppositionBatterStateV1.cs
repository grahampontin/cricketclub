namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// State of a single opposition batter during ball-by-ball scoring.
    /// The batter is identified by name (no player ID) because opposition players have no records.
    /// </summary>
    public class OppositionBatterStateV1
    {
        public string BatsmanName { get; set; }
        public int Position { get; set; }
        public string State { get; set; }
        public int CurrentScore { get; set; }
        public int BallsFaced { get; set; }
        public int Fours { get; set; }
        public int Sixes { get; set; }
        public decimal StrikeRate { get; set; }
    }
}

