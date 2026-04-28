using System;
using System.Collections.Generic;
using CricketClubDomain;
using CricketClubMiddle;
using NUnit.Framework;

namespace CricketClub.Tests
{
    /// <summary>
    /// Unit tests for VenueStatsRecalculator.ComputeForVenue.
    /// Covers the batting-friendliness (difficulty) score calculation.
    /// </summary>
    [TestFixture]
    public class VenueStatsRecalculatorTests
    {
        // ── ComputeForVenue ───────────────────────────────────────────────────────

        [Test]
        public void ComputeForVenue_NoMatches_ReturnsZeroScore()
        {
            var result = VenueStatsRecalculator.ComputeForVenue(1, new List<MatchScoreSummaryData>());

            Assert.AreEqual(1, result.VenueId);
            Assert.AreEqual(0, result.MatchesPlayed);
            Assert.AreEqual(0.0, result.DifficultyScore, 1e-9);
        }

        [Test]
        public void ComputeForVenue_AbandonedMatchesExcluded()
        {
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 5, abandoned: true, ourScore: 0, theirScore: 0)
            };

            var result = VenueStatsRecalculator.ComputeForVenue(5, matches);

            Assert.AreEqual(0, result.MatchesPlayed);
            Assert.AreEqual(0.0, result.DifficultyScore, 1e-9);
        }

        [Test]
        public void ComputeForVenue_ZeroWickets_ReturnsZeroScore()
        {
            // When no wickets are recorded the formula cannot compute rpw → score = 0
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 1, abandoned: false, ourScore: 200, theirScore: 180, ourWickets: 0, theirWickets: 0)
            };

            var result = VenueStatsRecalculator.ComputeForVenue(1, matches);

            Assert.AreEqual(1, result.MatchesPlayed);
            Assert.AreEqual(0.0, result.DifficultyScore, 1e-9);
        }

        [Test]
        public void ComputeForVenue_RpwBelowFloor_ScoreClampsToZero()
        {
            // rpw = 120 / 24 = 5.0 — below the normalisation floor of 13 → score = 0 (minefield)
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 1, abandoned: false, ourScore: 60, theirScore: 60,
                      ourWickets: 12, theirWickets: 12)
            };

            var result = VenueStatsRecalculator.ComputeForVenue(1, matches);

            Assert.AreEqual(0.0, result.DifficultyScore, 1e-9,
                "score must be clamped to 0 when rpw is below normalisation floor");
        }

        [Test]
        public void ComputeForVenue_BalancedVenue_ScoreNearFifty()
        {
            // rpw = 245 / 10 = 24.5 → score = (24.5 - 13) / 23 × 100 = 50.0 exactly
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 2, abandoned: false, ourScore: 125, theirScore: 120,
                      ourWickets: 5, theirWickets: 5)
            };

            var result = VenueStatsRecalculator.ComputeForVenue(2, matches);

            const double expectedRpw   = 245.0 / 10.0;
            const double expectedScore = (expectedRpw - 13.0) / 23.0 * 100.0;
            Assert.AreEqual(expectedScore, result.DifficultyScore, 1e-6,
                "balanced venue (rpw = 24.5) should score exactly 50");
        }

        [Test]
        public void ComputeForVenue_RpwAboveCeiling_ScoreClampsToHundred()
        {
            // rpw = 400 / 8 = 50.0 — above the normalisation ceiling of 36 → score = 100 (road)
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 3, abandoned: false, ourScore: 200, theirScore: 200,
                      ourWickets: 4, theirWickets: 4)
            };

            var result = VenueStatsRecalculator.ComputeForVenue(3, matches);

            Assert.AreEqual(100.0, result.DifficultyScore, 1e-9,
                "score must be clamped to 100 when rpw exceeds normalisation ceiling");
        }

        [Test]
        public void ComputeForVenue_InningsCountedSeparately()
        {
            // Only our innings has a score; their innings was not recorded (theirScore=0, theirWickets=0).
            // CompletedInningsCount should be 1; rpw uses only the wickets from our innings.
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 4, abandoned: false, ourScore: 160, theirScore: 0,
                      ourWickets: 8, theirWickets: 0)
            };

            var result = VenueStatsRecalculator.ComputeForVenue(4, matches);

            Assert.AreEqual(1, result.CompletedInningsCount, "Only our innings should be counted");

            const double rpw      = 160.0 / 8.0;   // = 20.0
            const double expected = (rpw - 13.0) / 23.0 * 100.0;
            Assert.AreEqual(expected, result.DifficultyScore, 1e-6);
        }

        [Test]
        public void ComputeForVenue_TotalsAccumulatedCorrectly()
        {
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 6, abandoned: false, ourScore: 100, theirScore: 150,
                      ourWickets: 7, theirWickets: 6),
                Match(venueId: 6, abandoned: false, ourScore: 120, theirScore: 130,
                      ourWickets: 8, theirWickets: 7),
            };

            var result = VenueStatsRecalculator.ComputeForVenue(6, matches);

            Assert.AreEqual(2,   result.MatchesPlayed);
            Assert.AreEqual(220, result.TotalOurInningsRuns);
            Assert.AreEqual(280, result.TotalTheirInningsRuns);
            Assert.AreEqual(15,  result.TotalOurWickets);
            Assert.AreEqual(13,  result.TotalTheirWickets);
            Assert.AreEqual(4,   result.CompletedInningsCount);

            // Verify DifficultyScore matches formula: rpw = (220+280)/(15+13) = 500/28
            const double rpw      = 500.0 / 28.0;
            const double expected = (rpw - 13.0) / 23.0 * 100.0;
            Assert.AreEqual(expected, result.DifficultyScore, 1e-6);
        }

        [Test]
        public void ComputeForVenue_MultipleMatchesAccumulated()
        {
            // 3 matches × (ourScore=100, theirScore=95, ourWickets=6, theirWickets=5)
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 7, abandoned: false, ourScore: 100, theirScore: 95, ourWickets: 6, theirWickets: 5),
                Match(venueId: 7, abandoned: false, ourScore: 100, theirScore: 95, ourWickets: 6, theirWickets: 5),
                Match(venueId: 7, abandoned: false, ourScore: 100, theirScore: 95, ourWickets: 6, theirWickets: 5),
            };

            var result = VenueStatsRecalculator.ComputeForVenue(7, matches);

            Assert.AreEqual(3, result.MatchesPlayed);

            // rpw = (300 + 285) / (18 + 15) = 585/33
            double rpw      = (300.0 + 285.0) / (18.0 + 15.0);
            double expected = Math.Max(0.0, Math.Min(100.0, (rpw - 13.0) / 23.0 * 100.0));
            Assert.AreEqual(expected, result.DifficultyScore, 1e-6);
        }

        // ── helpers ──────────────────────────────────────────────────────────────

        private static MatchScoreSummaryData Match(
            int venueId, bool abandoned, int ourScore, int theirScore,
            int ourWickets = 0, int theirWickets = 0) =>
            new MatchScoreSummaryData
            {
                VenueId      = venueId,
                Abandoned    = abandoned,
                OurScore     = ourScore,
                TheirScore   = theirScore,
                OurWickets   = ourWickets,
                TheirWickets = theirWickets,
                MatchDate    = DateTime.Today
            };
    }
}

