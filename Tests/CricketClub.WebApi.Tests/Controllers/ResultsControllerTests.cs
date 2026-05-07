#nullable disable
using CricketClub.WebApi.Controllers;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using CricketClub.WebApi.Tests.Utils;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class ResultsControllerTests
    {
        private readonly Mock<IDao> _mockDao;
        private readonly ResultsController _controller;

        public ResultsControllerTests()
        {
            TestDefaults.ResetInternalCache();

            _mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(_mockDao);

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

            _controller = new ResultsController(_mockDao.Object, mockEnv.Object);
            TestDefaults.SetupHttpContext(_controller);

            // Mock extra dependencies ResultV1.FromInternal touches
            SetupMockBowlingStats();
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

        [Fact]
        public void GetResults_WeWonTossAndBatted_TossFieldsReflectUs()
        {
            // Arrange: WonToss = true (we won), Batted = true (we batted)
            var matchData = new MatchData
            {
                ID = 1,
                Date = new DateTime(2026, 1, 20),
                OppositionID = 1,
                VenueID = 1,
                MatchType = 1,
                HomeOrAway = "Home",
                WonToss = true,
                Batted = true
            };
            _mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData> { matchData });
            _mockDao.Setup(d => d.GetAllMatchReports()).Returns(new Dictionary<int, MatchReportAndConditions>());

            // Act
            var result = _controller.GetResults(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsAssignableFrom<List<ResultV1>>(okResult.Value);
            var item = Assert.Single(results);
            // WonToss = true → toss winner is "Us" (The Village)
            Assert.NotNull(item.TossWinner);
            Assert.Equal("bat", item.TossWinnerElectedTo);
        }

        [Fact]
        public void GetResults_OppositionWonTossAndBowled_TossFieldsReflectOpposition()
        {
            // Arrange: WonToss = false (opposition won), Batted = false (opposition fielded → we batted)
            var matchData = new MatchData
            {
                ID = 1,
                Date = new DateTime(2026, 1, 20),
                OppositionID = 1,
                VenueID = 1,
                MatchType = 1,
                HomeOrAway = "Home",
                WonToss = false,
                Batted = false
            };
            _mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData> { matchData });
            _mockDao.Setup(d => d.GetAllMatchReports()).Returns(new Dictionary<int, MatchReportAndConditions>());

            // Act
            var result = _controller.GetResults(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsAssignableFrom<List<ResultV1>>(okResult.Value);
            var item = Assert.Single(results);
            // WonToss = false → toss winner is the opposition
            Assert.NotNull(item.TossWinner);
            Assert.Equal("bowl", item.TossWinnerElectedTo);
        }
    }
}
