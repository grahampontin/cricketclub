namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Represents the number of catches dropped by a player in a single match.
    /// </summary>
    public class MatchDropV1
    {
        /// <summary>The player who dropped the catch(es).</summary>
        public int PlayerId { get; set; }

        /// <summary>Number of catches dropped by this player in the match.</summary>
        public int Drops { get; set; }
    }
}

