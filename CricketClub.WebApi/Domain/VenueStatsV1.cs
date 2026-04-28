using CricketClubDomain;

namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Venue batting-friendliness statistics derived from venue_stats_cache.
    /// Metric: average runs per wicket (batting average at the venue).
    ///   Score 0 = minefield (batsmen dismissed cheaply); Score 100 = road (batsmen dominant).
    /// </summary>
    public class VenueStatsV1
    {
        /// <summary>Completed matches played at this venue.</summary>
        public int MatchesPlayed { get; set; }

        /// <summary>Completed matches where we scored more than the opposition.</summary>
        public int Won { get; set; }

        /// <summary>Completed matches where the opposition scored more than us.</summary>
        public int Lost { get; set; }

        /// <summary>Completed matches with equal scores (draws, ties, no result in play).</summary>
        public int NoResult { get; set; }

        /// <summary>Win percentage over completed matches as a fraction (0–1). 0 when no matches played.</summary>
        public double WinPercentage { get; set; }

        /// <summary>
        /// Average runs per wicket (batting average) across all completed innings at this venue.
        /// This is the primary difficulty metric: low = batsmen struggle, high = batsmen dominate.
        /// </summary>
        public double AverageRunsPerWicket { get; set; }

        /// <summary>Average runs per innings — supplementary context alongside runs-per-wicket.</summary>
        public double AverageRunsPerInnings { get; set; }

        /// <summary>
        /// Batting-friendliness score 0–100. Null when fewer than 3 completed matches (insufficient data).
        /// Formula: clamp((runsPerWicket - 13) / 23 × 100, 0, 100).
        /// Calibrated against historical club data: balanced ≈ rpw 24.5 → score 50.
        /// </summary>
        public double? DifficultyScore { get; set; }

        /// <summary>
        /// Human-readable pitch rating:
        /// "minefield" (≤20) | "difficult" (≤40) | "balanced" (≤60) | "batting-friendly" (≤80) | "road" (&gt;80).
        /// "unknown" when fewer than 3 completed matches.
        /// </summary>
        public string DifficultyLabel { get; set; } = "unknown";

        public static VenueStatsV1 FromCache(VenueStatsCacheData? cache)
        {
            if (cache == null || cache.MatchesPlayed < 3)
            {
                return new VenueStatsV1
                {
                    MatchesPlayed         = cache?.MatchesPlayed ?? 0,
                    Won                   = cache?.Won ?? 0,
                    Lost                  = cache?.Lost ?? 0,
                    NoResult              = cache?.NoResult ?? 0,
                    WinPercentage         = cache?.WinPercentage ?? 0.0,
                    AverageRunsPerWicket  = cache?.AverageRunsPerWicket ?? 0.0,
                    AverageRunsPerInnings = cache?.AverageRunsPerInnings ?? 0.0,
                    DifficultyScore       = null,
                    DifficultyLabel       = "unknown"
                };
            }

            return new VenueStatsV1
            {
                MatchesPlayed         = cache.MatchesPlayed,
                Won                   = cache.Won,
                Lost                  = cache.Lost,
                NoResult              = cache.NoResult,
                WinPercentage         = cache.WinPercentage,
                AverageRunsPerWicket  = cache.AverageRunsPerWicket,
                AverageRunsPerInnings = cache.AverageRunsPerInnings,
                DifficultyScore       = cache.DifficultyScore,
                DifficultyLabel       = BuildLabel(cache.DifficultyScore)
            };
        }

        public static string BuildLabel(double score) => score switch
        {
            <= 20 => "minefield",
            <= 40 => "difficult",
            <= 60 => "balanced",
            <= 80 => "batting-friendly",
            _     => "road"
        };
    }
}

