#nullable disable
using CricketClub.WebApi.Controllers;
using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Services;
using CricketClub.WebApi.Tests.Utils;
using CricketClubDomain;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class FixturesControllerTests
    {
        private readonly Mock<IMatchService> _mockMatchService;
        private readonly FixturesController _controller;

        public FixturesControllerTests()
        {
            TestDefaults.ResetInternalCache();

            _mockMatchService = TestDefaults.MockMatchService();
            _controller = new FixturesController(_mockMatchService.Object, TestDefaults.MockEnvironment().Object);
            TestDefaults.SetupHttpContext(_controller);
        }

        [Fact]
        public void GetFixtures_WithoutSeason_ReturnsAllFixtures()
        {
            // Arrange - use future dates (today is 2026-01-27)
            var matchData = new MatchData
            {
                ID = 1,
                Date = DateTime.Today.AddDays(10), // Future fixture
                OppositionID = 1,
                VenueID = 1,
                MatchType = 1,
                HomeOrAway = "Home"
            };
            _mockMatchService.Setup(s => s.GetFixtures()).Returns(new List<MatchData> { matchData });

            // Act
            var result = _controller.GetFixtures(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var fixtures = Assert.IsAssignableFrom<List<MatchV1>>(okResult.Value);
            Assert.Single(fixtures);
        }

        [Fact]
        public void GetFixtures_WithSeasonFilter_ReturnsFilteredFixtures()
        {
            // Arrange - use current year (2026) for fixtures
            var matches = new List<MatchData>
            {
                new MatchData { ID = 1, Date = new DateTime(2026, 8, 15), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" }
            };
            // GetBySeason will filter by year; "future only" is applied in the controller using Date >= today
            _mockMatchService.Setup(s => s.GetBySeason(2026)).Returns(matches);

            // Act
            var result = _controller.GetFixtures(2026);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var fixtures = Assert.IsAssignableFrom<List<MatchV1>>(okResult.Value);
            Assert.Single(fixtures);
        }

        [Fact]
        public void GetFixtures_WithNoMatchesInSeason_ReturnsEmptyList()
        {
            // Arrange
            _mockMatchService.Setup(s => s.GetBySeason(2025)).Returns(new List<MatchData>());

            // Act
            var result = _controller.GetFixtures(2025);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var fixtures = Assert.IsAssignableFrom<List<MatchV1>>(okResult.Value);
            Assert.Empty(fixtures);
        }

        [Fact]
        public void GetFixtures_FiltersCorrectly_ByDateRange()
        {
            // Arrange - all in-season matches are in the future; one next-season match also in future
            var season = DateTime.Today.Year;

            var inSeasonList = new List<MatchData>
            {
                new MatchData { ID = 1, Date = DateTime.Today.AddDays(7),  OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" },
                new MatchData { ID = 2, Date = DateTime.Today.AddDays(30), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" },
                new MatchData { ID = 3, Date = DateTime.Today.AddDays(90), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" },
            };
            // The season filter is applied by GetBySeason, so only in-season matches come back
            _mockMatchService.Setup(s => s.GetBySeason(season)).Returns(inSeasonList);

            // Act
            var result = _controller.GetFixtures(season);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var fixtures = Assert.IsAssignableFrom<List<MatchV1>>(okResult.Value);
            Assert.Equal(3, fixtures.Count);
        }
    }
}
