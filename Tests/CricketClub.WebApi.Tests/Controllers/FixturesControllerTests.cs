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
    public class FixturesControllerTests
    {
        private readonly Mock<IDao> _mockDao;
        private readonly FixturesController _controller;

        public FixturesControllerTests()
        {
            TestDefaults.ResetInternalCache();

            _mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(_mockDao);

            _controller = new FixturesController(_mockDao.Object, TestDefaults.MockEnvironment().Object);
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
            // Arrange
            // Fixtures are typically defined as matches in the future. Keep all "in-season" matches in the future
            // relative to today, while still verifying that next-season matches are excluded.
            var season = DateTime.Today.Year;

            var inSeasonFuture1 = DateTime.Today.AddDays(7);
            var inSeasonFuture2 = DateTime.Today.AddDays(30);
            var inSeasonFuture3 = DateTime.Today.AddDays(90);

            var inSeasonList = new[] { inSeasonFuture1, inSeasonFuture2, inSeasonFuture3 }
                .Select((d, index) => new MatchData
                {
                    ID = index + 1,
                    Date = new DateTime(season, d.Month, d.Day),
                    OppositionID = 1,
                    VenueID = 1,
                    MatchType = 1,
                    HomeOrAway = "Home"
                })
                .ToList();

            // One match in the next season, also in the future, which should be excluded by the season filter.
            var nextSeasonFuture = DateTime.Today.AddDays(14);
            inSeasonList.Add(new MatchData
            {
                ID = 99,
                Date = new DateTime(season + 1, nextSeasonFuture.Month, nextSeasonFuture.Day),
                OppositionID = 1,
                VenueID = 1,
                MatchType = 1,
                HomeOrAway = "Home"
            });

            _mockDao.Setup(d => d.GetAllMatches()).Returns(inSeasonList);

            // Act
            var result = _controller.GetFixtures(season);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var fixtures = Assert.IsAssignableFrom<List<MatchV1>>(okResult.Value);
            Assert.Equal(3, fixtures.Count);
        }
    }
}
