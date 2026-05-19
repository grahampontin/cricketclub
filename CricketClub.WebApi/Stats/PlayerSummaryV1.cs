namespace CricketClub.WebApi.Stats
{
    /// <summary>
    /// Lightweight player summary for the batter mini-profile panel on the live scorecard.
    /// All fields are nullable so the endpoint works for players with incomplete records.
    /// </summary>
    public class PlayerSummaryV1
    {
        public int PlayerId { get; set; }
        public string? FirstName { get; set; }
        public string? Surname { get; set; }
        public string? PlayingRole { get; set; }
        public string? ImageUrl { get; set; }
        public int? Matches { get; set; }
        public int? CareerRuns { get; set; }
        public decimal? BattingAverage { get; set; }

        /// <summary>
        /// Highest score as a string, preserving the not-out suffix (e.g. "94*").
        /// Null if the player has never batted.
        /// </summary>
        public string? HighScore { get; set; }

        public int? CareerWickets { get; set; }

        /// <summary>
        /// Best bowling figures in W/R format (e.g. "5/22").
        /// Null if the player has never taken a wicket.
        /// </summary>
        public string? BestBowling { get; set; }

        /// <summary>
        /// Year of debut. Null if no debut date is recorded.
        /// </summary>
        public int? DebutYear { get; set; }
    }
}

