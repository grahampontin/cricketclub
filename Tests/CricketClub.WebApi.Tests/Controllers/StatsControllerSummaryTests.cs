#nullable disable
using CricketClub.WebApi.Controllers;
using CricketClub.WebApi.Stats;
using CricketClub.WebApi.Tests.Utils;
using CricketClubDAL;
using CricketClubDomain;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using MatchType = CricketClubDomain.MatchType;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class StatsControllerSummaryTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly StatsController controller;

        public StatsControllerSummaryTests()
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            // Minimal stats data — batting/bowling/fielding all empty so the player has no recorded career.
            mockDao.Setup(d => d.GetPlayerBattingStatsData(It.IsAny<int>())).Returns(new List<BattingCardLineData>());
            mockDao.Setup(d => d.GetPlayerBowlingStatsData(It.IsAny<int>())).Returns(new List<BowlingStatsEntryData>());
            mockDao.Setup(d => d.GetPlayerFieldingStatsData(It.IsAny<int>())).Returns(new List<BattingCardLineData>());

            var env = TestDefaults.MockEnvironment();
            controller = new StatsController(env.Object, mockDao.Object);
            TestDefaults.SetupHttpContext(controller);
        }

        [Fact]
        public void GetPlayerSummary_ReturnsOk_WithCorrectPlayerId()
        {
            // Arrange
            mockDao.Setup(d => d.GetPlayerData(42)).Returns(new PlayerData
            {
                ID = 42,
                Name = "A Tester",
                FirstName = "Alice",
                Surname = "Tester"
            });

            // Act
            var result = controller.GetPlayerSummary(42);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var summary = Assert.IsType<PlayerSummaryV1>(ok.Value);
            Assert.Equal(42, summary.PlayerId);
            Assert.Equal("Alice", summary.FirstName);
            Assert.Equal("Tester", summary.Surname);
        }

        [Fact]
        public void GetPlayerSummary_NoBattingRecords_ReturnsNullHighScoreAndAverage()
        {
            // Arrange
            mockDao.Setup(d => d.GetPlayerData(1)).Returns(new PlayerData { ID = 1, Name = "New Player" });

            // Act
            var result = controller.GetPlayerSummary(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var summary = Assert.IsType<PlayerSummaryV1>(ok.Value);
            Assert.Null(summary.HighScore);
            Assert.Null(summary.BattingAverage);
        }

        [Fact]
        public void GetPlayerSummary_NoBowlingRecords_ReturnsNullBestBowling()
        {
            // Arrange
            mockDao.Setup(d => d.GetPlayerData(2)).Returns(new PlayerData { ID = 2, Name = "Batter Only" });

            // Act
            var result = controller.GetPlayerSummary(2);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var summary = Assert.IsType<PlayerSummaryV1>(ok.Value);
            Assert.Null(summary.BestBowling);
        }

        [Fact]
        public void GetPlayerSummary_NoDebutDate_ReturnsNullDebutYear()
        {
            // Arrange — PlayerData with default DateTime => Year == 1
            mockDao.Setup(d => d.GetPlayerData(3)).Returns(new PlayerData { ID = 3, Name = "No Debut" });

            // Act
            var result = controller.GetPlayerSummary(3);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var summary = Assert.IsType<PlayerSummaryV1>(ok.Value);
            Assert.Null(summary.DebutYear);
        }

        [Fact]
        public void GetPlayerSummary_WithBattingRecords_PopulatesHighScoreAndAverage()
        {
            // Arrange — one not-out innings of 94 runs
            mockDao.Setup(d => d.GetPlayerData(10)).Returns(new PlayerData
            {
                ID = 10,
                Name = "Good Batter",
                FirstName = "Good",
                Surname = "Batter"
            });
            mockDao.Setup(d => d.GetPlayerBattingStatsData(10)).Returns(new List<BattingCardLineData>
            {
                new BattingCardLineData
                {
                    PlayerID = 10,
                    MatchID = 100,
                    Score = 94,
                    ModeOfDismissal = (int)ModesOfDismissal.NotOut,
                    MatchDate = new DateTime(2020, 6, 1),
                    MatchTypeID = (int)MatchType.Friendly
                }
            });

            // Act
            var result = controller.GetPlayerSummary(10);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var summary = Assert.IsType<PlayerSummaryV1>(ok.Value);
            Assert.Equal("94*", summary.HighScore);
            Assert.NotNull(summary.BattingAverage);
            Assert.Equal(94, summary.CareerRuns);
        }

        [Fact]
        public void GetPlayerSummary_WithWickets_PopulatesBestBowling()
        {
            // Arrange — one bowling entry with 3 wickets for 22 runs
            mockDao.Setup(d => d.GetPlayerData(11)).Returns(new PlayerData { ID = 11, Name = "Bowler" });
            mockDao.Setup(d => d.GetPlayerBowlingStatsData(11)).Returns(new List<BowlingStatsEntryData>
            {
                new BowlingStatsEntryData
                {
                    PlayerID = 11,
                    MatchID = 200,
                    Wickets = 3,
                    Runs = 22,
                    Overs = 8,
                    Maidens = 1,
                    MatchDate = new DateTime(2021, 7, 10),
                    MatchTypeID = (int)MatchType.Friendly
                }
            });

            // Act
            var result = controller.GetPlayerSummary(11);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var summary = Assert.IsType<PlayerSummaryV1>(ok.Value);
            Assert.NotNull(summary.BestBowling);
            Assert.Equal(3, summary.CareerWickets);
        }
    }
}


