namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Lightweight summary for a single opposition team, suitable for listing pages.
    /// Stats are pre-computed from the team_stats_cache table.
    /// </summary>
    public class TeamSummaryV1
    {
        /// <summary>Unique team identifier.</summary>
        public int Id { get; set; }

        /// <summary>Team display name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Name of this team's home venue, or null if unknown.</summary>
        public string? HomeVenueName { get; set; }

        /// <summary>Traffic-light difficulty rating: "red" (hardest), "amber", "green" (easiest), or "unknown" (fewer than 3 completed matches).</summary>
        public string DifficultyRating { get; set; } = "green";

        /// <summary>
        /// Raw margin-weighted difficulty score used to assign the rating.
        /// Mean of (TheirScore − OurScore) / (OurScore + TheirScore) per completed match,
        /// with batting-second wins measured by wickets in hand instead of run difference.
        /// Range: −1 (we dominated every game) to +1 (they dominated every game).
        /// Null when fewer than 3 completed matches (insufficient data).
        /// Use this for precise sorting; use DifficultyRating for display.
        /// </summary>
        public double? DifficultyScore { get; set; }

        /// <summary>Win percentage as a fraction (0–1). Returns 0 if no matches played.</summary>
        public double WinPercentage { get; set; }

        /// <summary>Total completed matches played against this team.</summary>
        public int Played { get; set; }

        /// <summary>Matches won against this team.</summary>
        public int Won { get; set; }

        /// <summary>Matches lost against this team.</summary>
        public int Lost { get; set; }

        /// <summary>Abandoned / no-result matches against this team.</summary>
        public int NoResult { get; set; }
    }
}

