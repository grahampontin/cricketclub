namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// A dismissal that occurred during the opposition's ball-by-ball innings.
    /// OUR players (bowler/fielder) are identified by player ID; the batter is a string name.
    /// </summary>
    public class OppositionWicketV1
    {
        public string BatsmanName { get; set; }
        public int BowlerPlayerId { get; set; }
        public int? FielderPlayerId { get; set; }
        public string ModeOfDismissal { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// A single ball in the opposition's ball-by-ball innings.
    /// </summary>
    public class OppositionBallV1
    {
        public int BallNumber { get; set; }
        public string BatsmanName { get; set; }
        public int BowlerPlayerId { get; set; }
        /// <summary>Ball type using the same constants as BallV1 (empty string = runs, "wd", "nb", "b", "lb", "p").</summary>
        public string Thing { get; set; }
        public int Amount { get; set; }
        public OppositionWicketV1 Wicket { get; set; }
        public decimal? Angle { get; set; }
        public bool IsWide { get; set; }
        public bool IsNoBall { get; set; }
        public bool IsBoundary { get; set; }
        public bool IsSix { get; set; }
    }
}

