using System;
namespace CricketClubDomain
{
    /// <summary>
    /// Pre-computed win/loss/draw statistics for an opposition team.
    /// Maintained by TeamStatsRecalculator; read by the API for fast difficulty-rating calculation.
    /// </summary>
    public class TeamStatsCacheData
    {
        public int TeamId { get; set; }
        /// <summary>Completed matches (excludes abandoned and matches with no scorecard yet).</summary>
        public int Played { get; set; }
        public int Won { get; set; }
        public int Lost { get; set; }
        /// <summary>Drawn or tied matches.</summary>
        public int Drawn { get; set; }
        public int Abandoned { get; set; }
        public DateTime LastUpdated { get; set; }
        /// <summary>Win percentage over completed matches as a fraction (0–1). Returns 0 if no matches played.</summary>
        public double WinPercentage => Played > 0 ? (double)Won / Played : 0.0;
    }
}
