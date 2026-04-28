using System;
namespace CricketClubDomain
{
    /// <summary>
    /// Pre-computed batting-friendliness statistics for a venue.
    /// The primary metric is average runs per wicket (batting average at the venue):
    ///   low rpw = batsmen struggle and are dismissed cheaply (minefield)
    ///   high rpw = batsmen score freely before being dismissed (road)
    ///
    /// DifficultyScore 0–100 is normalised as: clamp((rpw - 13) / 23 * 100, 0, 100)
    ///   rpw ≤ 13 → 0 (absolute minefield)  rpw = 24.5 → 50 (balanced)  rpw ≥ 36 → 100 (road)
    /// </summary>
    public class VenueStatsCacheData
    {
        public int VenueId { get; set; }
        /// <summary>Completed matches (excluded abandoned and scoreless matches).</summary>
        public int MatchesPlayed { get; set; }
        /// <summary>Total runs scored by our side across all completed innings at this venue.</summary>
        public int TotalOurInningsRuns { get; set; }
        /// <summary>Total runs scored by the opposition across all completed innings at this venue.</summary>
        public int TotalTheirInningsRuns { get; set; }
        /// <summary>Total wickets fallen in our batting innings at this venue.</summary>
        public int TotalOurWickets { get; set; }
        /// <summary>Total wickets fallen in opposition batting innings at this venue.</summary>
        public int TotalTheirWickets { get; set; }
        /// <summary>Number of innings where at least one run was scored (used for average runs per innings).</summary>
        public int CompletedInningsCount { get; set; }
        /// <summary>
        /// Batting-friendliness score 0–100.
        /// 0 = minefield, 100 = road. Based on average runs per wicket across all completed matches.
        /// </summary>
        public double DifficultyScore { get; set; }
        public DateTime LastUpdated { get; set; }

        /// <summary>Average runs per wicket (batting average) at this venue. 0 if no wickets recorded.</summary>
        public double AverageRunsPerWicket
        {
            get
            {
                var totalWickets = TotalOurWickets + TotalTheirWickets;
                return totalWickets > 0
                    ? (double)(TotalOurInningsRuns + TotalTheirInningsRuns) / totalWickets
                    : 0.0;
            }
        }

        /// <summary>Average runs per innings at this venue. Supplementary display metric.</summary>
        public double AverageRunsPerInnings =>
            CompletedInningsCount > 0
                ? (double)(TotalOurInningsRuns + TotalTheirInningsRuns) / CompletedInningsCount
                : 0.0;
    }
}
