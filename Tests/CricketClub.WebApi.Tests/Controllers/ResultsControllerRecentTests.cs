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
    public class ResultsControllerRecentTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly ResultsController controller;

        public ResultsControllerRecentTests()
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            // Match graph dependencies touched by ResultV1
            mockDao.Setup(d => d.GetVenueData(It.IsAny<int>())).Returns((int id) => new VenueData { ID = id, Name = "V" + id, Coordinates = new Tuple<decimal?, decimal?>(51.5m, -0.1m) });
            mockDao.Setup(d => d.GetTeamData(It.IsAny<int>())).Returns((int id) => new TeamData { ID = id, Name = id == 0 ? "Us" : "Opp" });

            mockDao.Setup(d => d.GetBattingCard(It.IsAny<int>(), It.IsAny<ThemOrUs>())).Returns(new List<BattingCardLineData>());
            mockDao.Setup(d => d.GetBowlingStats(It.IsAny<int>(), It.IsAny<ThemOrUs>())).Returns(new List<BowlingStatsEntryData>());

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

            controller = new ResultsController(mockDao.Object, mockEnv.Object);
            TestDefaults.SetupHttpContext(controller);
        }

        [Fact]
        public void GetRecentResults_BadCount_ReturnsBadRequest()
        {
            var result = controller.GetRecentResults(0);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void GetRecentResults_ReturnsMostRecentAcrossSeasons()
        {
            // Arrange: Match.GetResults(dao) calls dao.GetAllMatches() and filters <= DateTime.Today
            var allMatches = new List<MatchData>
            {
                new MatchData { ID = 1, Date = new DateTime(2024, 5, 1), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "H" },
                new MatchData { ID = 2, Date = new DateTime(2023, 6, 1), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "H" },
                new MatchData { ID = 3, Date = new DateTime(2025, 1, 1), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "H" }
            };

            mockDao.Setup(d => d.GetAllMatches()).Returns(allMatches);
            mockDao.Setup(d => d.GetAllMatchReports()).Returns(new Dictionary<int, MatchReportAndConditions>());
            mockDao.Setup(d => d.GetAllMatchScoreSummaries()).Returns(
                allMatches.Select(m => new MatchScoreSummaryData { MatchId = m.ID, OppositionId = m.OppositionID, VenueId = m.VenueID, MatchDate = m.Date }).ToList());

            // Act
            var action = controller.GetRecentResults(2);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action);
            var results = Assert.IsAssignableFrom<List<ResultV1>>(ok.Value);
            Assert.Equal(2, results.Count);
            Assert.Equal(3, results[0].MatchId);
            Assert.Equal(1, results[1].MatchId);
        }
    }
}
