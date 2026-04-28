using CricketClub.WebApi.Domain;
using CricketClubDomain;
using Xunit;

namespace CricketClub.WebApi.Tests.Domain
{
    /// <summary>
    /// Unit tests for VenueStatsV1 DTO mapping and label logic.
    /// </summary>
    public class VenueStatsV1Tests
    {
        // ── BuildLabel ────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0.0,   "minefield")]
        [InlineData(20.0,  "minefield")]
        [InlineData(20.1,  "difficult")]
        [InlineData(40.0,  "difficult")]
        [InlineData(40.1,  "balanced")]
        [InlineData(60.0,  "balanced")]
        [InlineData(60.1,  "batting-friendly")]
        [InlineData(80.0,  "batting-friendly")]
        [InlineData(80.1,  "road")]
        [InlineData(100.0, "road")]
        public void BuildLabel_ReturnsCorrectLabelForScore(double score, string expected)
        {
            Assert.Equal(expected, VenueStatsV1.BuildLabel(score));
        }

        // ── FromCache ─────────────────────────────────────────────────────────────

        [Fact]
        public void FromCache_NullCache_ReturnsUnknownWithZeros()
        {
            var result = VenueStatsV1.FromCache(null);

            Assert.Equal("unknown", result.DifficultyLabel);
            Assert.Null(result.DifficultyScore);
            Assert.Equal(0, result.MatchesPlayed);
            Assert.Equal(0.0, result.AverageRunsPerWicket);
            Assert.Equal(0.0, result.AverageRunsPerInnings);
        }

        [Fact]
        public void FromCache_FewerThanThreeMatches_ReturnsUnknown()
        {
            var cache = new VenueStatsCacheData
            {
                VenueId               = 1,
                MatchesPlayed         = 2,
                TotalOurInningsRuns   = 200,
                TotalTheirInningsRuns = 180,
                TotalOurWickets       = 8,
                TotalTheirWickets     = 7,
                CompletedInningsCount = 4,
                DifficultyScore       = 45.0
            };

            var result = VenueStatsV1.FromCache(cache);

            Assert.Equal("unknown", result.DifficultyLabel);
            Assert.Null(result.DifficultyScore);
            Assert.Equal(2, result.MatchesPlayed);
        }

        [Fact]
        public void FromCache_ThreeOrMoreMatches_ReturnsDifficultyScoreAndLabel()
        {
            // rpw = (300+280)/(12+10) = 580/22 ≈ 26.4 → score = (26.4-13)/23*100 ≈ 58.3 → "balanced"
            var cache = new VenueStatsCacheData
            {
                VenueId               = 5,
                MatchesPlayed         = 3,
                TotalOurInningsRuns   = 300,
                TotalTheirInningsRuns = 280,
                TotalOurWickets       = 12,
                TotalTheirWickets     = 10,
                CompletedInningsCount = 6,
                DifficultyScore       = 58.3
            };

            var result = VenueStatsV1.FromCache(cache);

            Assert.Equal(3,    result.MatchesPlayed);
            Assert.Equal(58.3, result.DifficultyScore);
            Assert.Equal("balanced", result.DifficultyLabel);

            // AverageRunsPerWicket: 580/22 ≈ 26.36
            Assert.Equal(580.0 / 22.0, result.AverageRunsPerWicket, 6);
            // AverageRunsPerInnings: 580/6 ≈ 96.67
            Assert.Equal(580.0 / 6.0,  result.AverageRunsPerInnings, 6);
        }
    }
}

