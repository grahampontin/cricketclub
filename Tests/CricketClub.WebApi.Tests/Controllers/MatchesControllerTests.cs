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
    public class MatchesControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly MatchesController controller;

        public MatchesControllerTests()
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            controller = new MatchesController(mockDao.Object);
        }

        [Fact]
        public void GetAllMatches_NoSeason_ReturnsAll()
        {
            // Arrange
            mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData>
            {
                new MatchData { ID = 1, Date = DateTime.Today, OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" },
                new MatchData { ID = 2, Date = DateTime.Today.AddDays(1), OppositionID = 2, VenueID = 2, MatchType = 1, HomeOrAway = "Away" }
            });

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
            mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData>
            {
                new MatchData { ID = 1, Date = new DateTime(2025, 6, 1), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" },
                new MatchData { ID = 2, Date = new DateTime(2026, 6, 1), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" }
            });

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
            mockDao.Setup(d => d.GetMatchData(5)).Returns(new MatchData { ID = 5, Date = DateTime.Today, OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" });

            // Act
            var result = controller.GetMatch(5);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var match = Assert.IsType<MatchV1>(ok.Value);
            Assert.Equal(5, match.Id);
        }
    }
}
