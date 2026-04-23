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
        /// <summary>
        /// True when we batted first (i.e. toss winner batted = us, or toss winner fielded = them).
        /// Derived from matches.won_toss and matches.batted: WeBattedFirst when won_toss == batted.
        /// </summary>
        public bool WeBattedFirst { get; set; }
        /// <summary>
        /// Wickets we lost in our batting innings (dismissals from batting_scorecards,
        /// excludes NotOut/DidNotBat/RetiredHurt and the extras row).
        /// </summary>
        public int OurWickets { get; set; }
        /// <summary>
        /// Wickets the opposition lost in their batting innings (dismissals from bowling_scorecards,
        /// excludes NotOut/DidNotBat/RetiredHurt and the extras row).
        /// </summary>
        public int TheirWickets { get; set; }
    }
}
