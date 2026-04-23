using System;
using System.Collections.Generic;
using System.Linq;
using CricketClubDAL;
using CricketClubDomain;

namespace CricketClubMiddle
{
    /// <summary>
    /// Computes and persists batting-friendliness statistics for all venues.
    /// Logic lives here in C# rather than in SQL stored procedures.
    /// Called from Match.Save() to keep the cache current after each result is entered.
    ///
    /// DifficultyScore 0–100:
    ///   0  = minefield  (batsmen really struggle — very low average runs per innings)
    ///   100 = road      (batsmen make loads of runs — very high average runs per innings)
    ///
    /// Normalisation ceiling: 300 runs per innings. Any venue averaging ≥300 scores 100.
    /// </summary>
    public static class VenueStatsRecalculator
    {
        private const double NormalisationCeiling = 300.0;

        /// <summary>
        /// Recalculates stats for a single venue and persists to venue_stats_cache.
        /// Called on every Match.Save() — only touches one row in the cache table.
        /// </summary>
        public static void RecalculateForVenue(int venueId, IDao dao)
        {
            var summaries = dao.GetAllMatchScoreSummaries()
                .Where(s => s.VenueId == venueId)
                .ToList();

            var stats = ComputeForVenue(venueId, summaries);
            dao.UpsertVenueStatsCache(stats);

            // Invalidate the in-process cache so the next read picks up the fresh value
            InternalCache.GetInstance().Remove("venueStatsCache");
        }

        /// <summary>
        /// Full rebuild of the cache for every venue.
        /// Use for admin/initial-seed scenarios; prefer RecalculateForVenue for incremental updates.
        /// </summary>
        public static void RecalculateAll(IDao dao)
        {
            var allSummaries = dao.GetAllMatchScoreSummaries();

            var byVenue = allSummaries.GroupBy(s => s.VenueId);
            foreach (var group in byVenue)
            {
                var stats = ComputeForVenue(group.Key, group.ToList());
                dao.UpsertVenueStatsCache(stats);
            }

            InternalCache.GetInstance().Remove("venueStatsCache");
        }

        /// <summary>
        /// Pure computation — no DB access. Testable in isolation.
        /// A match counts as "played" only when it has a scorecard (at least one side scored).
        /// Abandoned matches are excluded from the average.
        /// </summary>
        public static VenueStatsCacheData ComputeForVenue(int venueId, IList<MatchScoreSummaryData> matches)
        {
            var completed = matches
                .Where(m => !m.Abandoned && (m.OurScore > 0 || m.TheirScore > 0))
                .ToList();

            var totalOurRuns   = completed.Sum(m => m.OurScore);
            var totalTheirRuns = completed.Sum(m => m.TheirScore);

            // Count each innings separately (both teams bat in a match)
            var completedInningsCount = completed.Count(m => m.OurScore > 0) +
                                        completed.Count(m => m.TheirScore > 0);

            var difficultyScore = 0.0;
            if (completedInningsCount > 0)
            {
                var avgRunsPerInnings = (double)(totalOurRuns + totalTheirRuns) / completedInningsCount;
                difficultyScore = Math.Min(avgRunsPerInnings / NormalisationCeiling * 100.0, 100.0);
            }

            return new VenueStatsCacheData
            {
                VenueId               = venueId,
                MatchesPlayed         = completed.Count,
                TotalOurInningsRuns   = totalOurRuns,
                TotalTheirInningsRuns = totalTheirRuns,
                CompletedInningsCount = completedInningsCount,
                DifficultyScore       = difficultyScore,
                LastUpdated           = DateTime.UtcNow
            };
        }
    }
}

