#nullable disable
using CricketClub.WebApi.Controllers;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using CricketClub.WebApi.Tests.Utils;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class LiveScoringControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly LiveScoringController controller;

        public LiveScoringControllerTests()
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            controller = new LiveScoringController(mockDao.Object, TestDefaults.MockEnvironment().Object);
            TestDefaults.SetupHttpContext(controller);
        }

        [Fact]
        public void GetMatches_WithSeason_ReturnsMatches()
        {
            // Arrange
            mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData>
            {
                new MatchData { ID = 1, Date = new DateTime(2026, 6, 1), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" }
            });

            // Act
            var result = controller.GetMatches(2026);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsAssignableFrom<List<LiveScoringMatchSummaryV1>>(ok.Value);
            Assert.Single(payload);

            var first = payload[0];
            Assert.Equal(LiveScoringMatchSummaryKindV1.Match, first.Kind);
            Assert.NotNull(first.Match);
            Assert.Null(first.BallByBall);
        }
    }
}
