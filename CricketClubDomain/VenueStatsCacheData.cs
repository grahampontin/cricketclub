using System;
namespace CricketClubDomain
{
    /// <summary>
    /// Pre-computed batting-friendliness statistics for a venue.
    /// Maintained by VenueStatsRecalculator; read by the API for fast pitch-rating calculation.
    /// A higher DifficultyScore means batsmen score more (a "road"); lower means batsmen struggle (a "minefield").
    /// </summary>
    public class VenueStatsCacheData
    {
        public int VenueId { get; set; }
        /// <summary>Completed matches played at this venue (excludes abandoned and matches with no scorecard yet).</summary>
        public int MatchesPlayed { get; set; }
        /// <summary>Total runs across all our batting innings at this venue.</summary>
        public int TotalOurInningsRuns { get; set; }
        /// <summary>Total runs across all their batting innings at this venue.</summary>
        public int TotalTheirInningsRuns { get; set; }
        /// <summary>Number of innings (both teams combined) with at least one ball recorded.</summary>
        public int CompletedInningsCount { get; set; }
        /// <summary>
        /// Batting-friendliness score normalised to 0–100.
        /// 0 = minefield (very low average runs), 100 = road (very high average runs).
        /// Based on average runs per innings across all completed matches at this venue.
        /// Stored as 0 until at least one completed match exists.
        /// </summary>
        public double DifficultyScore { get; set; }
        public DateTime LastUpdated { get; set; }

        /// <summary>Average runs per innings across all completed innings at this venue. Returns 0 if no innings recorded.</summary>
        public double AverageRunsPerInnings =>
            CompletedInningsCount > 0 ? (double)(TotalOurInningsRuns + TotalTheirInningsRuns) / CompletedInningsCount : 0.0;
    }
}

