#nullable disable
using CricketClub.WebApi.Controllers;
using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Services;
using CricketClub.WebApi.Tests.Utils;
using CricketClubDAL;
using CricketClubDomain;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class MatchesControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly Mock<IMatchService> mockMatchService;
        private readonly MatchesController controller;

        public MatchesControllerTests()
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            mockMatchService = TestDefaults.MockMatchService();

            controller = new MatchesController(mockDao.Object, TestDefaults.MockEnvironment().Object, mockMatchService.Object);
            TestDefaults.SetupHttpContext(controller);
        }

        [Fact]
        public void GetAllMatches_NoSeason_ReturnsAll()
        {
            // Arrange
            var matchDataList = new List<MatchData>
            {
                new MatchData { ID = 1, Date = DateTime.Today, OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" },
                new MatchData { ID = 2, Date = DateTime.Today.AddDays(1), OppositionID = 2, VenueID = 2, MatchType = 1, HomeOrAway = "Away" }
            };
            mockMatchService.Setup(s => s.GetAll()).Returns(matchDataList);

            // Act
            var result = controller.GetAllMatches(null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var matches = Assert.IsAssignableFrom<List<MatchV1>>(ok.Value);
            Assert.Equal(2, matches.Count);
        }

        [Fact]
        public void GetAllMatches_WithSeason_Filters()
        {
            // Arrange
            var matchDataList = new List<MatchData>
            {
                new MatchData { ID = 2, Date = new DateTime(2026, 6, 1), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" }
            };
            mockMatchService.Setup(s => s.GetBySeason(2026)).Returns(matchDataList);

            // Act
            var result = controller.GetAllMatches(2026);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var matches = Assert.IsAssignableFrom<List<MatchV1>>(ok.Value);
            Assert.Single(matches);
            Assert.Equal(2, matches[0].Id);
        }

        [Fact]
        public void GetMatch_ReturnsMatch()
        {
            // Arrange
            mockMatchService.Setup(s => s.GetById(5))
                .Returns(new MatchData { ID = 5, Date = DateTime.Today, OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" });

            // Act
            var result = controller.GetMatch(5);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var match = Assert.IsType<MatchV1>(ok.Value);
            Assert.Equal(5, match.Id);
        }

        private void SetupCleanMatch(int matchId)
        {
            mockDao.Setup(d => d.GetBattingCard(matchId, ThemOrUs.Us)).Returns(Enumerable.Empty<BattingCardLineData>());
            mockDao.Setup(d => d.GetBattingCard(matchId, ThemOrUs.Them)).Returns(Enumerable.Empty<BattingCardLineData>());
            mockDao.Setup(d => d.GetBowlingStats(matchId, ThemOrUs.Us)).Returns(new List<BowlingStatsEntryData>());
            mockDao.Setup(d => d.GetBowlingStats(matchId, ThemOrUs.Them)).Returns(new List<BowlingStatsEntryData>());
            mockDao.Setup(d => d.GetAllBallsForMatch(matchId)).Returns(new List<Over>());
            mockDao.Setup(d => d.GetMatchReport(matchId)).Returns(MatchReportAndConditions.None);
        }

        [Fact]
        public void DeleteMatch_WithNoAssociatedData_Returns204NoContent()
        {
            // Arrange
            SetupCleanMatch(42);

            // Act
            var result = controller.DeleteMatch(42);

            // Assert
            Assert.IsType<NoContentResult>(result);
            mockMatchService.Verify(s => s.Delete(42), Times.Once);
        }

        [Fact]
        public void DeleteMatch_WithScorecardData_Returns409Conflict()
        {
            // Arrange
            SetupCleanMatch(10);
            mockDao.Setup(d => d.GetBattingCard(10, ThemOrUs.Us)).Returns(new List<BattingCardLineData>
            {
                new BattingCardLineData { MatchID = 10, PlayerID = 1, Score = 50 }
            });

            // Act
            var result = controller.DeleteMatch(10);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("scorecard data", conflict.Value.ToString());
            Assert.Contains("10", conflict.Value.ToString());
            mockMatchService.Verify(s => s.Delete(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void DeleteMatch_WithBallByBallData_Returns409Conflict()
        {
            // Arrange
            SetupCleanMatch(20);
            mockDao.Setup(d => d.GetAllBallsForMatch(20)).Returns(new List<Over> { new Over() });

            // Act
            var result = controller.DeleteMatch(20);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("ball by ball data", conflict.Value.ToString());
            Assert.Contains("20", conflict.Value.ToString());
            mockMatchService.Verify(s => s.Delete(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void DeleteMatch_WithMatchReport_Returns409Conflict()
        {
            // Arrange
            SetupCleanMatch(30);
            mockDao.Setup(d => d.GetMatchReport(30)).Returns(new MatchReportAndConditions("Sunny", "Great match", ""));

            // Act
            var result = controller.DeleteMatch(30);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("a match report", conflict.Value.ToString());
            Assert.Contains("30", conflict.Value.ToString());
            mockMatchService.Verify(s => s.Delete(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void DeleteMatch_WithMultipleDataTypes_Returns409ConflictListingAllReasons()
        {
            // Arrange
            SetupCleanMatch(50);
            mockDao.Setup(d => d.GetBowlingStats(50, ThemOrUs.Us)).Returns(new List<BowlingStatsEntryData>
            {
                new BowlingStatsEntryData { MatchID = 50, PlayerID = 1, Wickets = 3 }
            });
            mockDao.Setup(d => d.GetAllBallsForMatch(50)).Returns(new List<Over> { new Over() });
            mockDao.Setup(d => d.GetMatchReport(50)).Returns(new MatchReportAndConditions("Cloudy", "Tough game", ""));

            // Act
            var result = controller.DeleteMatch(50);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            var message = conflict.Value.ToString();
            Assert.Contains("scorecard data", message);
            Assert.Contains("ball by ball data", message);
            Assert.Contains("a match report", message);
            mockMatchService.Verify(s => s.Delete(It.IsAny<int>()), Times.Never);
        }
    }
}
