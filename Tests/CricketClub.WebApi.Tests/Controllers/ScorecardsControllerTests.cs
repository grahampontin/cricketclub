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
    public class ScorecardsControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly ScorecardsController controller;

        public ScorecardsControllerTests()
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            controller = new ScorecardsController(mockDao.Object);

            // Match graph dependencies
            mockDao.Setup(d => d.GetPlayerData(It.IsAny<int>())).Returns((int id) => new PlayerData { ID = id, Name = "P" + id, IsActive = true });

            // Scorecard-related dependencies
            mockDao.Setup(d => d.GetBattingCard(It.IsAny<int>(), It.IsAny<ThemOrUs>())).Returns(new List<BattingCardLineData>());
            mockDao.Setup(d => d.GetBowlingStats(It.IsAny<int>(), It.IsAny<ThemOrUs>())).Returns(new List<BowlingStatsEntryData>());
            mockDao.Setup(d => d.GetFoWData(It.IsAny<int>(), It.IsAny<ThemOrUs>())).Returns(new List<FoWDataLine>());
            mockDao.Setup(d => d.GetExtras(It.IsAny<int>(), It.IsAny<ThemOrUs>())).Returns(new ExtrasData());
        }

        [Fact]
        public void GetScorecard_ReturnsScorecard()
        {
            // Arrange
            mockDao.Setup(d => d.GetMatchData(1)).Returns(new MatchData
            {
                ID = 1,
                Date = DateTime.Today,
                OppositionID = 1,
                VenueID = 1,
                MatchType = 1,
                HomeOrAway = "H",
                CaptainID = 1,
                WicketKeeperID = 2,
                Overs = 40,
                WonToss = true,
                Batted = true
            });

            // Act
            var result = controller.GetScorecard(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var scorecard = Assert.IsType<MatchScorecardV1>(ok.Value);
            Assert.NotNull(scorecard.MatchConditions);
            Assert.Equal(1, scorecard.MatchConditions.captainId);
        }

        [Fact]
        public void SaveScorecard_NullBody_ReturnsBadRequest()
        {
            // Act
            var result = controller.SaveScorecard(1, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
