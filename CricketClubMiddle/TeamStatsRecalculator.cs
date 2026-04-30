using System;
using System.Collections.Generic;
using System.Linq;
using CricketClubDAL;
using CricketClubDomain;

namespace CricketClubMiddle
{
    /// <summary>
    /// Computes and persists win/loss/draw statistics for all opposition teams.
    /// Logic lives here in C# rather than in SQL stored procedures.
    /// Called from Match.Save() to keep the cache current after each result is entered.
    /// </summary>
    public static class TeamStatsRecalculator
    {
        /// <summary>
        /// Recalculates stats for a single opposition team and persists to team_stats_cache.
        /// Called on every Match.Save() — only touches one row in the cache table.
        /// </summary>
        public static void RecalculateForTeam(int teamId, IDao dao)
        {
            var allSummaries = dao.GetAllMatchScoreSummaries();
            RecalculateForTeam(teamId, dao, allSummaries);
        }

        /// <summary>
        /// Same as <see cref="RecalculateForTeam(int,IDao)"/> but accepts pre-fetched summaries
        /// so the caller can share a single DB round-trip across multiple recalculations.
        /// </summary>
        public static void RecalculateForTeam(int teamId, IDao dao, IList<MatchScoreSummaryData> allSummaries)
        {
            var summaries = allSummaries
                .Where(s => s.OppositionId == teamId)
                .ToList();

            var stats = ComputeForTeam(teamId, summaries);
            dao.UpsertTeamStatsCache(stats);

            // Invalidate the in-process cache so the next read picks up the fresh value
            InternalCache.GetInstance().Remove("teamStatsCache");
        }

        /// <summary>
        /// Full rebuild of the cache for every opposition team.
        /// Use for admin/initial-seed scenarios; prefer RecalculateForTeam for incremental updates.
        /// </summary>
        public static void RecalculateAll(IDao dao)
        {
            var allSummaries = dao.GetAllMatchScoreSummaries();

            var byTeam = allSummaries.GroupBy(s => s.OppositionId);
            foreach (var group in byTeam)
            {
                var stats = ComputeForTeam(group.Key, group.ToList());
                dao.UpsertTeamStatsCache(stats);
            }

            InternalCache.GetInstance().Remove("teamStatsCache");
        }

        /// <summary>
        /// Pure computation — no DB access.  Testable in isolation.
        /// A match counts as "played" only when it has a scorecard (at least one side scored).
        /// Abandoned matches are tallied separately and excluded from Played.
        /// </summary>
        public static TeamStatsCacheData ComputeForTeam(int teamId, IList<MatchScoreSummaryData> matches)
        {
            var completed = matches
                .Where(m => !m.Abandoned && (m.OurScore > 0 || m.TheirScore > 0))
                .ToList();

            var difficultyScore = 0.0;
            if (completed.Count > 0)
                difficultyScore = completed.Select(MatchDifficultyContribution).Average();

            return new TeamStatsCacheData
            {
                TeamId          = teamId,
                Played          = completed.Count,
                Won             = completed.Count(m => m.OurScore > m.TheirScore),
                Lost            = completed.Count(m => m.OurScore < m.TheirScore),
                Drawn           = completed.Count(m => m.OurScore == m.TheirScore),
                Abandoned       = matches.Count(m => m.Abandoned),
                DifficultyScore = difficultyScore,
                LastUpdated     = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Returns the per-match contribution to DifficultyScore.
        /// Positive = the opposition outperformed us (harder); negative = we outperformed them (easier).
        ///
        /// Cricket has two fundamentally different win types:
        ///
        ///   Batting-first win  → decided by run margin.
        ///     A team that scored 200 and bowled the opposition out for 100 won by 100 runs.
        ///     Normalised: (winner − loser) / (winner + loser)  so a larger margin in a
        ///     lower-scoring game carries more weight than the same margin in a high-scoring one.
        ///
        ///   Batting-second win → decided by wickets in hand.
        ///     A team that chases 100 and reaches 101 for 0 won by 10 wickets — a crushing
        ///     victory — even though the run scores look almost equal.
        ///     Normalised: (10 − wicketsLost) / 10.  Range 0–1 (1 = 10-wicket win).
        ///
        /// From our perspective:
        ///   negative contribution = easy (we outperformed); positive = hard (they outperformed).
        /// </summary>
        public static double MatchDifficultyContribution(MatchScoreSummaryData m)
        {
            if (m.WeBattedFirst)
            {
                if (m.OurScore > m.TheirScore)
                {
                    // We set target, they fell short → run win for us (easier).
                    var total = m.OurScore + m.TheirScore;
                    return total > 0 ? -(double)(m.OurScore - m.TheirScore) / total : 0.0;
                }

                if (m.OurScore < m.TheirScore)
                {
                    // We set target, they chased it → wicket win for them (harder).
                    return (10 - m.TheirWickets) / 10.0;
                }
            }
            else
            {
                if (m.OurScore > m.TheirScore)
                {
                    // They set target, we chased it → wicket win for us (easier).
                    return -(10 - m.OurWickets) / 10.0;
                }

                if (m.OurScore < m.TheirScore)
                {
                    // They set target, we failed → run win for them (harder).
                    var total = m.OurScore + m.TheirScore;
                    return total > 0 ? (double)(m.TheirScore - m.OurScore) / total : 0.0;
                }
            }

            return 0.0; // draw or tie
        }
    }
}

