using CricketClubDomain;

namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Venue batting-friendliness statistics derived from venue_stats_cache.
    /// Score 0 = minefield (batsmen struggle); Score 100 = road (batsmen make loads of runs).
    /// </summary>
    public class VenueStatsV1
    {
        /// <summary>Completed matches played at this venue.</summary>
        public int MatchesPlayed { get; set; }

        /// <summary>Average runs per innings across all completed innings at this venue.</summary>
        public double AverageRunsPerInnings { get; set; }

        /// <summary>
        /// Batting-friendliness score normalised 0–100.
        /// 0 = minefield, 100 = road.
        /// Null if fewer than 3 completed matches (insufficient data).
        /// </summary>
        public double? DifficultyScore { get; set; }

        /// <summary>
        /// Human-readable label for the difficulty score:
        /// "minefield" (&lt;=20), "difficult" (&lt;=40), "balanced" (&lt;=60), "batting-friendly" (&lt;=80), "road" (&gt;80).
        /// "unknown" when insufficient data.
        /// </summary>
        public string DifficultyLabel { get; set; } = "unknown";

        public static VenueStatsV1 FromCache(VenueStatsCacheData? cache)
        {
            if (cache == null || cache.MatchesPlayed < 3)
            {
                return new VenueStatsV1
                {
                    MatchesPlayed       = cache?.MatchesPlayed ?? 0,
                    AverageRunsPerInnings = cache?.AverageRunsPerInnings ?? 0.0,
                    DifficultyScore     = null,
                    DifficultyLabel     = "unknown"
                };
            }

            return new VenueStatsV1
            {
                MatchesPlayed         = cache.MatchesPlayed,
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

