namespace CricketClubDomain
{
    /// <summary>
    /// Represents a single dropped catch event in a match.
    /// One row per drop — query COUNT(*) GROUP BY player_id to get a player's total drops in a match.
    /// </summary>
    public class MatchDropData
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int PlayerId { get; set; }
    }
}

