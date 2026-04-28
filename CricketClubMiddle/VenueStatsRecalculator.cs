using System;
using System.Collections.Generic;
using System.Linq;
using CricketClubDAL;
using CricketClubDomain;

namespace CricketClubMiddle
{
    /// <summary>
    /// Computes and persists batting-friendliness statistics for venues.
    ///
    /// METRIC: average runs per wicket (rpw) — the batting average at the venue.
    ///   A low rpw means batsmen are dismissed cheaply (minefield).
    ///   A high rpw means batsmen score freely before being dismissed (road).
    ///   rpw is strictly better than runs-per-innings because it captures both
    ///   how many runs are scored AND how hard it is to survive.
    ///
    /// NORMALISATION (calibrated against historical club data, 37 rated venues):
    ///   score = clamp((rpw - 13) / 23 × 100, 0, 100)
    ///   rpw ≤ 13 → score   0  (absolute minefield — never seen in data, headroom)
    ///   rpw = 17  → score ~17  (e.g. Springfield Park — a known difficult venue)
    ///   rpw = 24.5→ score  50  (balanced, mid-range)
    ///   rpw = 28  → score ~65  (batting-friendly)
    ///   rpw ≥ 36  → score 100  (road — very rare)
    ///
    /// LABELS (applied in VenueStatsV1.BuildLabel):
    ///   0–20  minefield  |  21–40  difficult  |  41–60  balanced
    ///   61–80  batting-friendly  |  81–100  road
    /// </summary>
    public static class VenueStatsRecalculator
    {
        private const double NormalisationFloor   = 13.0;
        private const double NormalisationRange   = 23.0;  // ceiling = 36

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
        /// Pure computation — no DB access. Fully testable in isolation.
        /// Abandoned matches and matches with zero scoring are excluded.
        /// </summary>
        public static VenueStatsCacheData ComputeForVenue(int venueId, IList<MatchScoreSummaryData> matches)
        {
            var completed = matches
                .Where(m => !m.Abandoned && (m.OurScore > 0 || m.TheirScore > 0))
                .ToList();

            var totalOurRuns    = completed.Sum(m => m.OurScore);
            var totalTheirRuns  = completed.Sum(m => m.TheirScore);
            var totalOurWkts    = completed.Sum(m => m.OurWickets);
            var totalTheirWkts  = completed.Sum(m => m.TheirWickets);
            var totalRuns       = totalOurRuns + totalTheirRuns;
            var totalWickets    = totalOurWkts + totalTheirWkts;

            var completedInningsCount = completed.Count(m => m.OurScore   > 0) +
                                        completed.Count(m => m.TheirScore > 0);

            var difficultyScore = 0.0;
            if (totalWickets > 0)
            {
                var rpw = (double)totalRuns / totalWickets;
                difficultyScore = Math.Max(0.0, Math.Min(100.0,
                    (rpw - NormalisationFloor) / NormalisationRange * 100.0));
            }

            return new VenueStatsCacheData
            {
                VenueId               = venueId,
                MatchesPlayed         = completed.Count,
                TotalOurInningsRuns   = totalOurRuns,
                TotalTheirInningsRuns = totalTheirRuns,
                TotalOurWickets       = totalOurWkts,
                TotalTheirWickets     = totalTheirWkts,
                CompletedInningsCount = completedInningsCount,
                DifficultyScore       = difficultyScore,
                LastUpdated           = DateTime.UtcNow
            };
        }
    }
}
