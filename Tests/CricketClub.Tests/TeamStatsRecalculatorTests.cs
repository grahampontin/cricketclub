using System;
using System.Collections.Generic;
using CricketClubDomain;
using CricketClubMiddle;
using NUnit.Framework;

namespace CricketClub.Tests
{
    /// <summary>
    /// Unit tests for TeamStatsRecalculator.ComputeForTeam and MatchDifficultyContribution,
    /// covering the batting-order-aware difficulty score introduced in migration 003.
    /// </summary>
    [TestFixture]
    public class TeamStatsRecalculatorTests
    {
        // ── MatchDifficultyContribution — batting first ──────────────────────────

        [Test]
        public void MatchDifficulty_WeBatFirst_WeWin_IsNegative()
        {
            // We bat first, score 200; they score 50 all out.
            // Contribution = -(200-50)/(200+50) = -150/250 = -0.6
            var m = Match(ourScore: 200, theirScore: 50, weBattedFirst: true, ourWickets: 10, theirWickets: 10);
            Assert.AreEqual(-0.6, TeamStatsRecalculator.MatchDifficultyContribution(m), 1e-9);
        }

        [Test]
        public void MatchDifficulty_WeBatFirst_TheyWin_RunMargin_IsPositive()
        {
            // We bat first, score 100; they fail to chase — WAIT, score 150 > 100, so they exceeded.
            // We bat first, score 100; they score 80 — NO that's a loss for them.
            // Scenario: we bat first 100, they bat first ... hmm.
            // WeBattedFirst + TheirScore < OurScore → we won.
            // WeBattedFirst + TheirScore > OurScore → they chased → wicket win for them.
            // For a RUN win for them: TheyBattedFirst + they win.
            // So for a batting-first RUN loss (they bat first, outscore us):
            // WeBattedFirst=false, TheirScore=200, OurScore=80 → run win for them
            var m = Match(ourScore: 80, theirScore: 200, weBattedFirst: false, ourWickets: 10, theirWickets: 3);
            var contribution = TeamStatsRecalculator.MatchDifficultyContribution(m);
            // (200-80)/(200+80) = 120/280 ≈ 0.429
            Assert.AreEqual(120.0 / 280.0, contribution, 1e-9);
            Assert.IsTrue(contribution > 0, "A loss should give a positive (harder) contribution");
        }

        [Test]
        public void MatchDifficulty_WeBatFirst_NormalisesForGameScale()
        {
            // Same 50-run margin, different game scales.
            // Low-scoring:  150 vs 100 → -(150-100)/250 = -0.2
            // High-scoring: 350 vs 300 → -(350-300)/650 ≈ -0.077
            var low  = Match(ourScore: 150, theirScore: 100, weBattedFirst: true,  ourWickets: 10, theirWickets: 10);
            var high = Match(ourScore: 350, theirScore: 300, weBattedFirst: true,  ourWickets: 10, theirWickets: 10);
            Assert.IsTrue(
                TeamStatsRecalculator.MatchDifficultyContribution(low) <
                TeamStatsRecalculator.MatchDifficultyContribution(high),
                "A 50-run win in a low-scoring game should produce a more negative (easier) contribution");
        }

        // ── MatchDifficultyContribution — batting second (the key fix) ───────────

        [Test]
        public void MatchDifficulty_WeBatSecond_TheyWin_By10Wickets_IsMax()
        {
            // Classic case: we bat first, score 100.
            // They chase, win 101 for 0 — a 10-wicket thrashing.
            // Old formula would give (101-100)/201 ≈ 0.005  ← WRONG (looks like a close match)
            // New formula: WeBattedFirst=true, OurScore<TheirScore, TheirWickets=0
            //   → (10 - 0) / 10 = 1.0  ← correctly signals a crushing defeat
            var m = Match(ourScore: 100, theirScore: 101, weBattedFirst: true, ourWickets: 10, theirWickets: 0);
            Assert.AreEqual(1.0, TeamStatsRecalculator.MatchDifficultyContribution(m), 1e-9,
                "A 10-wicket chase should score maximum difficulty (1.0), not near-zero");
        }

        [Test]
        public void MatchDifficulty_WeBatSecond_TheyWin_By1Wicket_IsSmall()
        {
            // They bat first 150, we chase and are bowled out for 151 with 9 wickets down — wait.
            // Scenario: we bat first 150; they chase 151 for 9 — they win by 1 wicket.
            // WeBattedFirst=true, OurScore=150, TheirScore=151, TheirWickets=9
            // → (10 - 9) / 10 = 0.1  (tight win for them = slightly hard for us)
            var m = Match(ourScore: 150, theirScore: 151, weBattedFirst: true, ourWickets: 10, theirWickets: 9);
            Assert.AreEqual(0.1, TeamStatsRecalculator.MatchDifficultyContribution(m), 1e-9,
                "A 1-wicket win should score 0.1 (slightly hard), not near-zero");
        }

        [Test]
        public void MatchDifficulty_WeChase_WeWin_By10Wickets_IsMin()
        {
            // They bat first 100, we win chasing with 101 for 0 — a 10-wicket thrashing of them.
            // WeBattedFirst=false, OurScore=101, TheirScore=100, OurWickets=0
            // Old formula: (100-101)/201 ≈ -0.005  ← WRONG (looks nearly even)
            // New formula: -(10 - 0) / 10 = -1.0  ← maximum ease
            var m = Match(ourScore: 101, theirScore: 100, weBattedFirst: false, ourWickets: 0, theirWickets: 10);
            Assert.AreEqual(-1.0, TeamStatsRecalculator.MatchDifficultyContribution(m), 1e-9,
                "A 10-wicket win for us should score minimum difficulty (-1.0), not near-zero");
        }

        [Test]
        public void MatchDifficulty_WeChase_WeWin_By1Wicket_IsSlightlyEasy()
        {
            // They bat first 150, we chase 151 for 9 — we win by 1 wicket.
            // WeBattedFirst=false, OurScore=151, TheirScore=150, OurWickets=9
            // → -(10 - 9) / 10 = -0.1  (tight win for us = slightly easy)
            var m = Match(ourScore: 151, theirScore: 150, weBattedFirst: false, ourWickets: 9, theirWickets: 10);
            Assert.AreEqual(-0.1, TeamStatsRecalculator.MatchDifficultyContribution(m), 1e-9);
        }

        [Test]
        public void MatchDifficulty_Draw_IsZero()
        {
            var m = Match(ourScore: 150, theirScore: 150, weBattedFirst: true, ourWickets: 5, theirWickets: 5);
            Assert.AreEqual(0.0, TeamStatsRecalculator.MatchDifficultyContribution(m));
        }

        // ── ComputeForTeam — DifficultyScore aggregation ─────────────────────────

        [Test]
        public void ComputeForTeam_DifficultyScore_IsZeroWithNoMatches()
        {
            var result = TeamStatsRecalculator.ComputeForTeam(1, new List<MatchScoreSummaryData>());
            Assert.AreEqual(0.0, result.DifficultyScore);
        }

        [Test]
        public void ComputeForTeam_DifficultyScore_IsAverageAcrossMultipleMatches()
        {
            // Match 1: we bat first, big run win → -0.6
            // Match 2: they bat first, 10-wicket win for us → -1.0
            // Average → -0.8
            var matches = new List<MatchScoreSummaryData>
            {
                Match(ourScore: 200, theirScore: 50,  weBattedFirst: true,  ourWickets: 10, theirWickets: 10), // -0.6
                Match(ourScore: 101, theirScore: 100, weBattedFirst: false, ourWickets: 0,  theirWickets: 10)  // -1.0
            };
            var result = TeamStatsRecalculator.ComputeForTeam(5, matches);
            Assert.AreEqual(-0.8, result.DifficultyScore, 1e-9);
        }

        [Test]
        public void ComputeForTeam_DifficultyScore_AbandonedMatchesExcluded()
        {
            var matches = new List<MatchScoreSummaryData>
            {
                Match(ourScore: 50, theirScore: 200, weBattedFirst: true,  ourWickets: 10, theirWickets: 0, abandoned: false),
                Match(ourScore: 99, theirScore: 1,   weBattedFirst: true,  ourWickets: 10, theirWickets: 10, abandoned: true)
            };
            var result = TeamStatsRecalculator.ComputeForTeam(5, matches);
            // Only match 1 contributes: weBattedFirst, OurScore<TheirScore, TheirWickets=0 → +1.0
            Assert.AreEqual(1.0, result.DifficultyScore, 1e-9);
        }

        [Test]
        public void ComputeForTeam_DifficultyScore_ZeroWhenBothScoresZero()
        {
            var matches = new List<MatchScoreSummaryData>
            {
                Match(ourScore: 0, theirScore: 0, weBattedFirst: true, ourWickets: 0, theirWickets: 0)
            };
            var result = TeamStatsRecalculator.ComputeForTeam(5, matches);
            Assert.AreEqual(0, result.Played, "A 0-0 match is not considered completed");
            Assert.AreEqual(0.0, result.DifficultyScore);
        }

        // ── Existing stats still computed correctly ──────────────────────────────

        [Test]
        public void ComputeForTeam_CountsPlayedWonLostDrawnAbandoned()
        {
            var matches = new List<MatchScoreSummaryData>
            {
                Match(ourScore: 150, theirScore: 100, weBattedFirst: true,  ourWickets: 10, theirWickets: 10), // won
                Match(ourScore: 100, theirScore: 150, weBattedFirst: true,  ourWickets: 10, theirWickets: 10), // lost
                Match(ourScore: 120, theirScore: 120, weBattedFirst: true,  ourWickets: 10, theirWickets: 10), // drawn
                Match(ourScore: 0,   theirScore: 0,   weBattedFirst: true,  ourWickets: 0,  theirWickets: 0,  abandoned: true) // abandoned
            };
            var result = TeamStatsRecalculator.ComputeForTeam(7, matches);
            Assert.AreEqual(3, result.Played);
            Assert.AreEqual(1, result.Won);
            Assert.AreEqual(1, result.Lost);
            Assert.AreEqual(1, result.Drawn);
            Assert.AreEqual(1, result.Abandoned);
        }

        // ── helpers ──────────────────────────────────────────────────────────────

        private static MatchScoreSummaryData Match(
            int ourScore, int theirScore,
            bool weBattedFirst,
            int ourWickets, int theirWickets,
            bool abandoned = false) =>
            new MatchScoreSummaryData
            {
                MatchId       = 1,
                OppositionId  = 5,
                OurScore      = ourScore,
                TheirScore    = theirScore,
                WeBattedFirst = weBattedFirst,
                OurWickets    = ourWickets,
                TheirWickets  = theirWickets,
                Abandoned     = abandoned
            };
    }
}

