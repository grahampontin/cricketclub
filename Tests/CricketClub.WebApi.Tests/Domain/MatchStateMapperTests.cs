using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Tests.Utils;
using CricketClubDomain;
using CricketClubMiddle;
using CricketClubMiddle.Stats;
using Xunit;

namespace CricketClub.WebApi.Tests.Domain
{
    public class MatchStateMapperTests
    {
        public MatchStateMapperTests()
        {
            TestDefaults.ResetInternalCache();
        }

        // ...existing tests...

        [Fact]
        public void MapToInPlayScorecardV1_WhenScorecardHasWaitingPlayers_YetToBatIsMappedPreservingOrder()
        {
            // Sorting by position is done in Match.GetLiveScorecard(); the mapper just maps in the order it receives.
            var scorecard = new LiveScorecard
            {
                YetToBat = new List<PlayerState>
                {
                    new PlayerState { PlayerId = 42, PlayerName = "A. Smith", Position = 3, State = PlayerState.Waiting },
                    new PlayerState { PlayerId = 43, PlayerName = "B. Jones", Position = 4, State = PlayerState.Waiting }
                }
            };

            var result = MatchStateMapper.MapToInPlayScorecardV1(scorecard);

            Assert.NotNull(result.YetToBat);
            Assert.Equal(2, result.YetToBat.Count);
            Assert.Equal(42, result.YetToBat[0].PlayerId);
            Assert.Equal("A. Smith", result.YetToBat[0].PlayerName);
            Assert.Equal(43, result.YetToBat[1].PlayerId);
            Assert.Equal("B. Jones", result.YetToBat[1].PlayerName);
        }

        [Fact]
        public void MapToInPlayScorecardV1_WhenYetToBatIsNull_YetToBatPropertyIsNull()
        {
            var scorecard = new LiveScorecard { YetToBat = null };

            var result = MatchStateMapper.MapToInPlayScorecardV1(scorecard);

            Assert.Null(result.YetToBat);
        }

        [Fact]
        public void MapToInPlayScorecardV1_WhenAllPlayersBatted_YetToBatIsEmpty()
        {
            var scorecard = new LiveScorecard
            {
                YetToBat = new List<PlayerState>()
            };

            var result = MatchStateMapper.MapToInPlayScorecardV1(scorecard);

            Assert.NotNull(result.YetToBat);
            Assert.Empty(result.YetToBat);
        }

        [Fact]
        public void MapToInPlayScorecardV1_NullScorecard_ReturnsNull()
        {
            var result = MatchStateMapper.MapToInPlayScorecardV1(null);

            Assert.Null(result);
        }

        // ── Opposition ball-by-ball innings tests ─────────────────────────────────

        /// <summary>
        /// When GetLiveScorecard() produces a scorecard with null OnStrikeBatsman / OtherBatsman
        /// (because our innings is Completed and it's now their ball-by-ball innings), the mapper
        /// must return null for those fields rather than throwing or producing a default.
        /// </summary>
        [Fact]
        public void MapToInPlayScorecardV1_WhenOurInningsCompleteAndTheirsBallByBall_OurLiveBattingFieldsAreNull()
        {
            // This simulates the output of Match.GetLiveScorecard() when VCC has completed
            // their innings and the opposition is now batting ball-by-ball.
            var scorecard = new LiveScorecard
            {
                OurInningsStatus = "Completed",
                TheirInningsStatus = "InProgress",
                TheirInningsIsBallByBall = true,
                // VCC live batting fields are not set (null) when innings is complete
                OnStrikeBatsman = null,
                OtherBatsman = null,
                BowlerOneDetails = null,
                BowlerTwoDetails = null,
                CurrentPartnership = null,
                PreviousPartnership = null,
                // VCC historical batting data is still populated
                Score = 145,
                Wickets = 9,
                OurLastCompletedOver = 20,
                // Opposition ball-by-ball data
                TheirScore = 60,
                TheirWickets = 3,
                TheirLastCompletedOver = 8,
                TheirRunRate = 7.5m
            };

            var result = MatchStateMapper.MapToInPlayScorecardV1(scorecard);

            // Live batting fields should be null — we are not currently batting
            Assert.Null(result.OnStrikeBatsman);
            Assert.Null(result.OtherBatsman);
            Assert.Null(result.BowlerOneDetails);
            Assert.Null(result.BowlerTwoDetails);
            Assert.Null(result.CurrentPartnership);
            Assert.Null(result.PreviousPartnership);

            // Historical batting data must still be present
            Assert.Equal(145, result.Score);
            Assert.Equal(9, result.Wickets);
            Assert.Equal(20, result.OurLastCompletedOver);

            // Opposition ball-by-ball data must be correctly surfaced
            Assert.True(result.TheirInningsIsBallByBall);
            Assert.Equal(60, result.TheirScore);
            Assert.Equal(3, result.TheirWickets);
            Assert.Equal(8, result.TheirLastCompletedOver);
            Assert.Equal(7.5m, result.TheirRunRate);
        }

        /// <summary>
        /// When the opposition innings has a full scorecard (batters, bowlers, partnerships, FoW, over summaries),
        /// all new "Their" fields must be correctly mapped through to V1.
        /// </summary>
        [Fact]
        public void MapToInPlayScorecardV1_WhenTheirInningsHasFullData_AllOppositionFieldsMapped()
        {
            var onStrike = new OppositionBatterScorecardLine { BatsmanName = "Smith", Score = 34, BallsFaced = 28, Fours = 3, Sixes = 1, StrikeRate = 121.4m };
            var other   = new OppositionBatterScorecardLine { BatsmanName = "Jones", Score = 12, BallsFaced = 18, Fours = 1, Sixes = 0, StrikeRate = 66.7m };
            var lastOut = new OppositionBatterScorecardLine { BatsmanName = "Brown", Score = 8 };

            var partnership = new OppositionPartnership("Smith", "Jones");

            var scorecard = new LiveScorecard
            {
                TheirInningsIsBallByBall = true,
                TheirOnStrikeBatsman = onStrike,
                TheirOtherBatsman = other,
                TheirLastBatsmanOut = lastOut,
                TheirCurrentPartnership = partnership,
                TheirPreviousPartnership = null,
                TheirPartnerships = new List<OppositionPartnership> { partnership },
                TheirFallOfWickets = new List<OppositionFallOfWicket>(),
                TheirLiveBowlingCard = new List<OppositionBowlerDetails>
                {
                    new OppositionBowlerDetails { PlayerId = 5, PlayerName = "VCC Bowler", Overs = 4, Runs = 22, Wickets = 1, Economy = 5.5m }
                },
                TheirBowlerOneDetails = new OppositionBowlerDetails { PlayerId = 5, PlayerName = "VCC Bowler", Overs = 4, Runs = 22, Wickets = 1 },
                TheirBallByBallCompletedOvers = new List<CricketClubMiddle.Stats.OppositionOverSummary>()
            };

            var result = MatchStateMapper.MapToInPlayScorecardV1(scorecard);

            Assert.True(result.TheirInningsIsBallByBall);

            // On-strike and other batters
            Assert.NotNull(result.TheirOnStrikeBatsman);
            Assert.Equal("Smith", result.TheirOnStrikeBatsman.BatsmanName);
            Assert.Equal(34, result.TheirOnStrikeBatsman.Score);
            Assert.NotNull(result.TheirOtherBatsman);
            Assert.Equal("Jones", result.TheirOtherBatsman.BatsmanName);

            // Last batsman out
            Assert.NotNull(result.TheirLastBatsmanOut);
            Assert.Equal("Brown", result.TheirLastBatsmanOut.BatsmanName);

            // Current partnership
            Assert.NotNull(result.TheirCurrentPartnership);
            Assert.Equal("Smith", result.TheirCurrentPartnership.BatsmanOneName);
            Assert.Equal("Jones", result.TheirCurrentPartnership.BatsmanTwoName);

            // All partnerships list
            Assert.Single(result.TheirPartnerships);

            // Fall of wickets
            Assert.NotNull(result.TheirFallOfWickets);
            Assert.Empty(result.TheirFallOfWickets);

            // Bowling card
            Assert.Single(result.TheirLiveBowlingCard);
            Assert.Equal(5, result.TheirLiveBowlingCard[0].PlayerId);

            // Bowler one details
            Assert.NotNull(result.TheirBowlerOneDetails);
            Assert.Equal(5, result.TheirBowlerOneDetails.PlayerId);

            // Over summaries
            Assert.NotNull(result.TheirBallByBallCompletedOvers);
            Assert.Empty(result.TheirBallByBallCompletedOvers);
        }

        /// <summary>
        /// TheirRunRate should be passed through from the scorecard regardless of mode.
        /// When the opposition innings is in progress using per-over summary mode, the run rate
        /// is computed from TheirOver in GetLiveScorecard(); the mapper just passes it through.
        /// </summary>
        [Fact]
        public void MapToInPlayScorecardV1_TheirRunRate_IsMappedDirectlyFromScorecard()
        {
            var scorecard = new LiveScorecard
            {
                TheirScore = 100,
                TheirOver = 10,
                TheirRunRate = 10.0m,
                TheirInningsIsBallByBall = false
            };

            var result = MatchStateMapper.MapToInPlayScorecardV1(scorecard);

            Assert.Equal(10.0m, result.TheirRunRate);
        }

        /// <summary>
        /// OversRemaining is passed straight through from the scorecard. The calculation
        /// (using ball-by-ball vs per-over data) happens in Match.GetLiveScorecard() before
        /// reaching the mapper.
        /// </summary>
        [Fact]
        public void MapToInPlayScorecardV1_OversRemaining_IsMappedDirectlyFromScorecard()
        {
            var scorecard = new LiveScorecard { OversRemaining = 12 };

            var result = MatchStateMapper.MapToInPlayScorecardV1(scorecard);

            Assert.Equal(12, result.OversRemaining);
        }

        [Theory]
        [InlineData(ModesOfDismissalV1.Bowled, "bowled")]
        [InlineData(ModesOfDismissalV1.Caught, "caught")]
        [InlineData(ModesOfDismissalV1.CaughtAndBowled, "c&b")]
        [InlineData(ModesOfDismissalV1.RunOut, "run out")]
        [InlineData(ModesOfDismissalV1.Stumped, "stumped")]
        [InlineData(ModesOfDismissalV1.LBW, "lbw")]
        [InlineData(ModesOfDismissalV1.HitWicket, "hit wicket")]
        [InlineData(ModesOfDismissalV1.Retired, "retired")]
        [InlineData(ModesOfDismissalV1.RetiredHurt, "retired hurt")]
        [InlineData(ModesOfDismissalV1.NotOut, "not out")]
        public void MapToInternalMatchState_WicketDismissalV1_MapsToCorrectDismissalText(
            ModesOfDismissalV1 v1Mode, string expectedText)
        {
            var update = new MatchStateUpdateV1
            {
                LastCompletedOver = 0,
                Over = new OverV1
                {
                    OverNumber = 1,
                    Balls = new[]
                    {
                        new BallV1
                        {
                            BallNumber = 1,
                            Amount = 0,
                            Batsman = 1,
                            BatsmanName = "Batsman",
                            Bowler = "Bowler",
                            Thing = Ball.Runs,
                            Wicket = new WicketV1
                            {
                                Player = 1,
                                PlayerName = "Batsman",
                                ModeOfDismissal = v1Mode,
                                Bowler = "Bowler",
                                Fielder = ""
                            }
                        }
                    }
                },
                Players = Array.Empty<PlayerStateV1>()
            };

            var result = MatchStateMapper.MapToInternalMatchState(update);

            Assert.NotNull(result.Over);
            Assert.Single(result.Over.Balls);
            var ball = result.Over.Balls[0];
            Assert.NotNull(ball.Wicket);
            Assert.Equal(expectedText, ball.Wicket.ModeOfDismissal);
        }

        [Fact]
        public void MapToInternalMatchState_NullWicket_MapsToNullWicket()
        {
            var update = new MatchStateUpdateV1
            {
                LastCompletedOver = 0,
                Over = new OverV1
                {
                    OverNumber = 1,
                    Balls = new[]
                    {
                        new BallV1
                        {
                            BallNumber = 1,
                            Amount = 4,
                            Batsman = 1,
                            BatsmanName = "Batsman",
                            Bowler = "Bowler",
                            Thing = Ball.Runs,
                            Wicket = null
                        }
                    }
                },
                Players = Array.Empty<PlayerStateV1>()
            };

            var result = MatchStateMapper.MapToInternalMatchState(update);

            Assert.NotNull(result.Over);
            Assert.Single(result.Over.Balls);
            Assert.Null(result.Over.Balls[0].Wicket);
        }
    }
}
