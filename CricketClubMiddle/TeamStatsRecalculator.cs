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
            var summaries = dao.GetAllMatchScoreSummaries()
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

            return new TeamStatsCacheData
            {
                TeamId      = teamId,
                Played      = completed.Count,
                Won         = completed.Count(m => m.OurScore > m.TheirScore),
                Lost        = completed.Count(m => m.OurScore < m.TheirScore),
                Drawn       = completed.Count(m => m.OurScore == m.TheirScore),
                Abandoned   = matches.Count(m => m.Abandoned),
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}

