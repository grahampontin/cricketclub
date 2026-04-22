using System;
namespace CricketClubDomain
{
    /// <summary>
    /// Lightweight per-match score summary used by TeamStatsRecalculator.
    /// Avoids loading full batting scorecards for every match.
    /// </summary>
    public class MatchScoreSummaryData
    {
        public int MatchId { get; set; }
        public int OppositionId { get; set; }
        public DateTime MatchDate { get; set; }
        public bool Abandoned { get; set; }
        /// <summary>Sum of all rows in batting_scorecards for this match (includes extras row).</summary>
        public int OurScore { get; set; }
        /// <summary>Sum of all rows in bowling_scorecards for this match (includes extras row).</summary>
        public int TheirScore { get; set; }
    }
}
