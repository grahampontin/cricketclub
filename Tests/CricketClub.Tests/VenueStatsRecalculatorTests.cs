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
        public void ComputeForVenue_LowScoringVenue_LowDifficultyScore()
        {
            // Average 50 runs per innings → 50/300 * 100 ≈ 16.67 (minefield territory)
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 1, abandoned: false, ourScore: 50, theirScore: 50),
                Match(venueId: 1, abandoned: false, ourScore: 50, theirScore: 50),
            };

            var result = VenueStatsRecalculator.ComputeForVenue(1, matches);

            Assert.AreEqual(2, result.MatchesPlayed);
            Assert.AreEqual(4, result.CompletedInningsCount);
            Assert.AreEqual(50.0 / 300.0 * 100.0, result.DifficultyScore, 1e-6);
        }

        [Test]
        public void ComputeForVenue_HighScoringVenue_HighDifficultyScore()
        {
            // Average 250 runs per innings → 250/300 * 100 ≈ 83.33 (road territory)
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 2, abandoned: false, ourScore: 250, theirScore: 250),
            };

            var result = VenueStatsRecalculator.ComputeForVenue(2, matches);

            Assert.AreEqual(1, result.MatchesPlayed);
            Assert.AreEqual(250.0 / 300.0 * 100.0, result.DifficultyScore, 1e-6);
        }

        [Test]
        public void ComputeForVenue_ScoreCappedAt100()
        {
            // Average 400 runs per innings — above ceiling of 300, should cap at 100
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 3, abandoned: false, ourScore: 400, theirScore: 400),
            };

            var result = VenueStatsRecalculator.ComputeForVenue(3, matches);

            Assert.AreEqual(100.0, result.DifficultyScore, 1e-9);
        }

        [Test]
        public void ComputeForVenue_InningsCountedSeparately()
        {
            // One match: our innings 200, their innings 0 (not recorded).
            // Only 1 innings contributed, avg = 200, score = 200/300*100.
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 4, abandoned: false, ourScore: 200, theirScore: 0),
            };

            var result = VenueStatsRecalculator.ComputeForVenue(4, matches);

            Assert.AreEqual(1, result.CompletedInningsCount, "Only our innings counted");
            Assert.AreEqual(200.0 / 300.0 * 100.0, result.DifficultyScore, 1e-6);
        }

        [Test]
        public void ComputeForVenue_TotalsAccumulatedCorrectly()
        {
            var matches = new List<MatchScoreSummaryData>
            {
                Match(venueId: 6, abandoned: false, ourScore: 100, theirScore: 150),
                Match(venueId: 6, abandoned: false, ourScore: 120, theirScore: 130),
            };

            var result = VenueStatsRecalculator.ComputeForVenue(6, matches);

            Assert.AreEqual(2, result.MatchesPlayed);
            Assert.AreEqual(220, result.TotalOurInningsRuns);
            Assert.AreEqual(280, result.TotalTheirInningsRuns);
            Assert.AreEqual(4,   result.CompletedInningsCount);
        }

        // ── helpers ──────────────────────────────────────────────────────────────

        private static MatchScoreSummaryData Match(int venueId, bool abandoned, int ourScore, int theirScore) =>
            new MatchScoreSummaryData
            {
                VenueId    = venueId,
                Abandoned  = abandoned,
                OurScore   = ourScore,
                TheirScore = theirScore,
                MatchDate  = DateTime.Today
            };
    }
}

