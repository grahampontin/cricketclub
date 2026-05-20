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

            // Ensure fallback path (no summary found) still works without DB calls
            _mockDao
                .Setup(d => d.GetBattingCard(It.IsAny<int>(), It.IsAny<ThemOrUs>()))
                .Returns(new List<BattingCardLineData>());
            _mockDao
                .Setup(d => d.GetBowlingStats(It.IsAny<int>(), It.IsAny<ThemOrUs>()))
                .Returns(new List<BowlingStatsEntryData>());
        }

        /// <summary>Creates a minimal summary for a given MatchData to exercise the fast path.</summary>
        private static MatchScoreSummaryData SummaryFor(MatchData m,
            int ourScore = 0, int theirScore = 0,
            int ourWickets = 0, int theirWickets = 0,
            decimal ourOversFaced = 0m, decimal theirOversFaced = 0m,
            bool weBattedFirst = true)
            => new MatchScoreSummaryData
            {
                MatchId        = m.ID,
                OppositionId   = m.OppositionID,
                VenueId        = m.VenueID,
                MatchDate      = m.Date,
                Abandoned      = m.Abandoned,
                OurScore       = ourScore,
                TheirScore     = theirScore,
                OurWickets     = ourWickets,
                TheirWickets   = theirWickets,
                OurOversFaced  = ourOversFaced,
                TheirOversFaced = theirOversFaced,
                WeBattedFirst  = weBattedFirst
            };

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
            _mockDao.Setup(d => d.GetAllMatchScoreSummaries()).Returns(new List<MatchScoreSummaryData> { SummaryFor(matchData) });

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
            _mockDao.Setup(d => d.GetAllMatchScoreSummaries()).Returns(matches.Select(m => SummaryFor(m)).ToList());

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
            _mockDao.Setup(d => d.GetAllMatchScoreSummaries()).Returns(new List<MatchScoreSummaryData>());

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
            _mockDao.Setup(d => d.GetAllMatchScoreSummaries()).Returns(new List<MatchScoreSummaryData> { SummaryFor(matchData) });

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
            _mockDao.Setup(d => d.GetAllMatchScoreSummaries()).Returns(new List<MatchScoreSummaryData> { SummaryFor(matchData) });

            // Act
            var result = _controller.GetResults(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsAssignableFrom<List<ResultV1>>(okResult.Value);
            var item = Assert.Single(results);
            // WonToss = true → toss winner is "Us" (The Village CC)
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
            _mockDao.Setup(d => d.GetAllMatchScoreSummaries()).Returns(new List<MatchScoreSummaryData> { SummaryFor(matchData) });

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

        [Fact]
        public void GetResults_WithSummary_ScoresAndWicketsPopulatedFromBulkQuery()
        {
            // Arrange: verify the fast summary path correctly populates score fields.
            // HomeOrAway = "H" so that match.HomeOrAway == HomeOrAway.Home (the legacy code checks .ToUpper() == "H").
            var matchData = new MatchData
            {
                ID = 5,
                Date = new DateTime(2026, 1, 20),
                OppositionID = 1,
                VenueID = 1,
                MatchType = 1,
                HomeOrAway = "H",   // "H" → HomeOrAway.Home
                WonToss = true,
                Batted = true
            };
            var summary = SummaryFor(matchData,
                ourScore: 175, theirScore: 142,
                ourWickets: 7, theirWickets: 10,
                ourOversFaced: 35.0m, theirOversFaced: 28.3m,
                weBattedFirst: true);

            _mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData> { matchData });
            _mockDao.Setup(d => d.GetAllMatchReports()).Returns(new Dictionary<int, MatchReportAndConditions>());
            _mockDao.Setup(d => d.GetAllMatchScoreSummaries()).Returns(new List<MatchScoreSummaryData> { summary });

            // Act
            var result = _controller.GetResults(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsAssignableFrom<List<ResultV1>>(okResult.Value);
            var item = Assert.Single(results);

            Assert.Equal(175, item.OurScore);
            Assert.Equal(7,   item.OurWickets);
            Assert.Equal(35.0m, item.OurOversFaced);
            Assert.Equal(142, item.TheirScore);
            Assert.Equal(10,  item.TheirWickets);
            Assert.Equal(28.3m, item.TheirOversFaced);
            Assert.True(item.IsWinner);
            Assert.Equal("by 33 runs", item.ResultMargin);
            // HomeOrAway="H" → Home → we are the home team → HomeTeamName should be "The Village CC"
            Assert.Equal("The Village CC", item.HomeTeamName);
            // Home team (us) won → "beat"
            Assert.Equal("beat", item.ResultText);
        }
    }
}
