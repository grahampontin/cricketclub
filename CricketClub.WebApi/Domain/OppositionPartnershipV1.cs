namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// A batting partnership in the opposition's ball-by-ball innings.
    /// Mirrors <see cref="PartnershipV1"/> but identifies batters by name rather than player ID.
    /// </summary>
    public class OppositionPartnershipV1
    {
        public string BatsmanOneName { get; set; }
        public string BatsmanTwoName { get; set; }
        public int Score { get; set; }
        public int BallCount { get; set; }
        public int BatsmanOneScore { get; set; }
        public int BatsmanTwoScore { get; set; }
        public int Fours { get; set; }
        public int Sixes { get; set; }
        public decimal RunRate { get; set; }
        public string OversAsString { get; set; }
    }
}

