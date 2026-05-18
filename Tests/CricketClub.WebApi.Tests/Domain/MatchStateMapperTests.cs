using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Tests.Utils;
using CricketClubDomain;
using CricketClubMiddle;
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
