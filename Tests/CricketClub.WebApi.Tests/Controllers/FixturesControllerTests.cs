#nullable disable
using CricketClub.WebApi.Controllers;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class FixturesControllerTests
    {
        private readonly Mock<IDao> _mockDao;
        private readonly FixturesController _controller;

        public FixturesControllerTests()
        {
            _mockDao = new Mock<IDao>();
            _controller = new FixturesController(_mockDao.Object);

            // Mock venue/team data that Match objects need
            SetupMockVenueAndTeamData();
        }

        private void SetupMockVenueAndTeamData()
        {
            // Mock venue data for VenueID = 1
            var venueData = new VenueData
            {
                ID = 1,
                Name = "Test Ground",
                MapUrl = "http://maps.test.com",
                Description = "Test venue",
                Coordinates = new Tuple<decimal?, decimal?>(51.5m, -0.1m)
            };
            _mockDao.Setup(d => d.GetVenueData(1)).Returns(venueData);

            // Mock team data for OppositionID = 1
            var teamData = new TeamData
            {
                ID = 1,
                Name = "Test Opposition"
            };
            _mockDao.Setup(d => d.GetTeamData(1)).Returns(teamData);

            // Mock team data for Us (ID = 0)
            var usTeamData = new TeamData
            {
                ID = 0,
                Name = "The Village"
            };
            _mockDao.Setup(d => d.GetTeamData(0)).Returns(usTeamData);
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
            _mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData> { matchData });

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
                new MatchData { ID = 1, Date = new DateTime(2026, 8, 15), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" },
                new MatchData { ID = 2, Date = new DateTime(2027, 8, 15), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" }
            };
            _mockDao.Setup(d => d.GetAllMatches()).Returns(matches);

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
            _mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData>());

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
            // Arrange - use 2026 (current year) for testing date range filtering
            var matches = new List<MatchData>
            {
                new MatchData { ID = 1, Date = new DateTime(2026, 2, 15), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" },
                new MatchData { ID = 2, Date = new DateTime(2026, 6, 15), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" },
                new MatchData { ID = 3, Date = new DateTime(2026, 12, 31), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" },
                new MatchData { ID = 4, Date = new DateTime(2027, 1, 1), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" }
            };
            _mockDao.Setup(d => d.GetAllMatches()).Returns(matches);

            // Act
            var result = _controller.GetFixtures(2026);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var fixtures = Assert.IsAssignableFrom<List<MatchV1>>(okResult.Value);
            Assert.Equal(3, fixtures.Count);
        }
    }
}
