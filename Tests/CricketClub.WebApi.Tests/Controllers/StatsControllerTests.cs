#nullable disable
using CricketClub.WebApi.Controllers;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class StatsControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly Mock<IWebHostEnvironment> mockEnvironment;
        private readonly StatsController controller;

        public StatsControllerTests()
        {
            mockDao = new Mock<IDao>();
            mockEnvironment = new Mock<IWebHostEnvironment>();
            controller = new StatsController(mockEnvironment.Object, mockDao.Object);
        }

        [Fact]
        public void GetLeadingPlayers_ReturnsOk_WithFourCategories()
        {
            // Arrange
            var playerData1 = new PlayerData { ID = 1, FirstName = "John", Surname = "Doe", IsActive = true };
            var playerData2 = new PlayerData { ID = 2, FirstName = "Jane", Surname = "Smith", IsActive = true };

            var battingStats1 = new List<BattingCardLineData>
            {
                new BattingCardLineData { PlayerID = 1, Score = 100, MatchID = 1, MatchDate = DateTime.Now, ModeOfDismissal = 0 },
                new BattingCardLineData { PlayerID = 1, Score = 50, MatchID = 2, MatchDate = DateTime.Now.AddDays(1), ModeOfDismissal = 0 }
            };

            var battingStats2 = new List<BattingCardLineData>
            {
                new BattingCardLineData { PlayerID = 2, Score = 75, MatchID = 1, MatchDate = DateTime.Now, ModeOfDismissal = 0 }
            };

            var bowlingStats1 = new List<BowlingStatsEntryData>
            {
                new BowlingStatsEntryData { PlayerID = 1, Wickets = 5, MatchID = 1, MatchDate = DateTime.Now }
            };

            var bowlingStats2 = new List<BowlingStatsEntryData>
            {
                new BowlingStatsEntryData { PlayerID = 2, Wickets = 3, MatchID = 1, MatchDate = DateTime.Now }
            };

            var fieldingStats = new Dictionary<int, List<BattingCardLineData>>();

            mockDao.Setup(d => d.GetAllPlayers()).Returns(new List<PlayerData> { playerData1, playerData2 });
            mockDao.Setup(d => d.GetAllBattingStatsData()).Returns(battingStats1.Concat(battingStats2).ToList().ToLookup(b => b.PlayerID));
            mockDao.Setup(d => d.GetAllBowlingStatsData()).Returns(bowlingStats1.Concat(bowlingStats2).ToList().ToLookup(b => b.PlayerID));
            mockDao.Setup(d => d.GetAllFieldingStatsData()).Returns(fieldingStats);

            // Act
            var result = controller.GetLeadingPlayers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var categories = Assert.IsAssignableFrom<List<LeadingPlayerCategoryV1>>(okResult.Value);
            Assert.Equal(4, categories.Count);
            Assert.Contains(categories, c => c.Category == "Most Runs");
            Assert.Contains(categories, c => c.Category == "Most Wickets");
            Assert.Contains(categories, c => c.Category == "Most Catches");
            Assert.Contains(categories, c => c.Category == "Most Appearances");
        }

        [Fact]
        public void GetLeadingPlayers_MostRunsCategory_ContainsCorrectPlayer()
        {
            // Arrange
            var playerData1 = new PlayerData { ID = 1, FirstName = "John", Surname = "Doe", IsActive = true };
            var playerData2 = new PlayerData { ID = 2, FirstName = "Jane", Surname = "Smith", IsActive = true };

            var battingStats1 = new List<BattingCardLineData>
            {
                new BattingCardLineData { PlayerID = 1, Score = 100, MatchID = 1, MatchDate = DateTime.Now, ModeOfDismissal = 0 },
                new BattingCardLineData { PlayerID = 1, Score = 50, MatchID = 2, MatchDate = DateTime.Now.AddDays(1), ModeOfDismissal = 0 }
            };

            var battingStats2 = new List<BattingCardLineData>
            {
                new BattingCardLineData { PlayerID = 2, Score = 75, MatchID = 1, MatchDate = DateTime.Now, ModeOfDismissal = 0 }
            };

            var bowlingStats = new List<BowlingStatsEntryData>();
            var fieldingStats = new Dictionary<int, List<BattingCardLineData>>();

            mockDao.Setup(d => d.GetAllPlayers()).Returns(new List<PlayerData> { playerData1, playerData2 });
            mockDao.Setup(d => d.GetAllBattingStatsData()).Returns(battingStats1.Concat(battingStats2).ToList().ToLookup(b => b.PlayerID));
            mockDao.Setup(d => d.GetAllBowlingStatsData()).Returns(bowlingStats.ToLookup(b => b.PlayerID));
            mockDao.Setup(d => d.GetAllFieldingStatsData()).Returns(fieldingStats);

            // Act
            var result = controller.GetLeadingPlayers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var categories = Assert.IsAssignableFrom<List<LeadingPlayerCategoryV1>>(okResult.Value);
            var mostRunsCategory = categories.FirstOrDefault(c => c.Category == "Most Runs");
            Assert.NotNull(mostRunsCategory);
            Assert.Single(mostRunsCategory.Players);
            Assert.Equal(1, mostRunsCategory.Players[0].PlayerId);
            Assert.Equal("John Doe", mostRunsCategory.Players[0].PlayerName);
            Assert.Equal(150, mostRunsCategory.Players[0].Value);
        }
    }
}
