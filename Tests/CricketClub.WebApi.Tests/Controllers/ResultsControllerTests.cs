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
    public class ResultsControllerTests
    {
        private readonly Mock<IDao> _mockDao;
        private readonly ResultsController _controller;

        public ResultsControllerTests()
        {
            _mockDao = new Mock<IDao>();
            _controller = new ResultsController(_mockDao.Object);
            
            // Mock venue/team/stats data that Match objects need
            SetupMockVenueAndTeamData();
            SetupMockBowlingStats();
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

        private void SetupMockBowlingStats()
        {
            // ResultV1.FromInternal calls match.GetOurBowlingStats().BowlingStatsData.Sum(...)
            // That constructs BowlingStats(matchId, ThemOrUs.Us, dao) which calls dao.GetBowlingStats.
            // Provide at least one row so Sum() works.
            _mockDao
                .Setup(d => d.GetBowlingStats(It.IsAny<int>(), It.IsAny<ThemOrUs>()))
                .Returns((int matchId, ThemOrUs who) =>
                    new List<BowlingStatsEntryData>
                    {
                        new BowlingStatsEntryData
                        {
                            MatchID = matchId,
                            PlayerID = 1,
                            Overs = 10m,
                            Maidens = 0,
                            Runs = 40,
                            Wickets = 2
                        }
                    });
        }

        [Fact]
        public void GetResults_WithoutSeason_ReturnsResultsForCurrentYear()
        {
            // Arrange - use past date in 2026 (today is 2026-01-27)
            var matchData = new MatchData 
            { 
                ID = 1, 
                Date = new DateTime(2026, 1, 15), // Past result in current year
                OppositionID = 1,
                VenueID = 1,
                MatchType = 1,
                HomeOrAway = "Home"
            };
            _mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData> { matchData });
            _mockDao.Setup(d => d.GetAllMatchReports()).Returns(new Dictionary<int, MatchReportAndConditions>());

            // Act
            var result = _controller.GetResults(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsAssignableFrom<List<ResultV1>>(okResult.Value);
            Assert.Single(results);
        }

        [Fact]
        public void GetResults_WithSeasonFilter_ReturnsFilteredResults()
        {
            // Arrange - use past dates in different years
            var matches = new List<MatchData>
            {
                new MatchData { ID = 1, Date = new DateTime(2025, 6, 15), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" },
                new MatchData { ID = 2, Date = new DateTime(2024, 6, 15), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" }
            };
            _mockDao.Setup(d => d.GetAllMatches()).Returns(matches);
            _mockDao.Setup(d => d.GetAllMatchReports()).Returns(new Dictionary<int, MatchReportAndConditions>());

            // Act
            var result = _controller.GetResults(2025);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsAssignableFrom<List<ResultV1>>(okResult.Value);
            Assert.Single(results);
        }

        [Fact]
        public void GetResults_WithNoMatchesInSeason_ReturnsEmptyList()
        {
            // Arrange
            _mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData>());
            _mockDao.Setup(d => d.GetAllMatchReports()).Returns(new Dictionary<int, MatchReportAndConditions>());

            // Act
            var result = _controller.GetResults(2025);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsAssignableFrom<List<ResultV1>>(okResult.Value);
            Assert.Empty(results);
        }

        [Fact]
        public void GetResults_WithMatchReports_IncludesReportData()
        {
            // Arrange - use past date
            var matchData = new MatchData 
            { 
                ID = 1, 
                Date = new DateTime(2026, 1, 20), // Recent past match
                OppositionID = 1,
                VenueID = 1,
                MatchType = 1,
                HomeOrAway = "Home"
            };
            var reportData = new MatchReportAndConditions("Sunny", "Test report", null);
            
            _mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData> { matchData });
            _mockDao.Setup(d => d.GetAllMatchReports()).Returns(new Dictionary<int, MatchReportAndConditions> 
            { 
                { 1, reportData } 
            });

            // Act
            var result = _controller.GetResults(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsAssignableFrom<List<ResultV1>>(okResult.Value);
            Assert.Single(results);
        }
    }
}
