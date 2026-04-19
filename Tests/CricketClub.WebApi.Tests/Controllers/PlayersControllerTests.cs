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
    public class PlayersControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly PlayersController controller;

        public PlayersControllerTests()
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            controller = new PlayersController(mockDao.Object);

            // PlayerV1.FromInternal creates CricketClubMiddle.Player which may query stats;
            // keep tests focused on controller filtering + DAO calls by returning minimal/empty stats.
            mockDao.Setup(d => d.GetPlayerBattingStatsData(It.IsAny<int>())).Returns(new List<BattingCardLineData>());
            mockDao.Setup(d => d.GetPlayerFieldingStatsData(It.IsAny<int>())).Returns(new List<BattingCardLineData>());
            mockDao.Setup(d => d.GetPlayerBowlingStatsData(It.IsAny<int>())).Returns(new List<BowlingStatsEntryData>());
        }

        [Fact]
        public void GetAllPlayers_Default_ExcludesInactive()
        {
            // Arrange
            mockDao.Setup(d => d.GetAllPlayers()).Returns(new List<PlayerData>
            {
                new PlayerData { ID = 1, Name = "Active", IsActive = true },
                new PlayerData { ID = 2, Name = "Inactive", IsActive = false }
            });

            // Act
            var result = controller.GetAllPlayers(false);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var players = Assert.IsAssignableFrom<List<PlayerV1>>(ok.Value);
            Assert.Single(players);
            Assert.Equal(1, players[0].PlayerId);
        }

        [Fact]
        public void GetAllPlayers_IncludeInactive_ReturnsAll()
        {
            // Arrange
            mockDao.Setup(d => d.GetAllPlayers()).Returns(new List<PlayerData>
            {
                new PlayerData { ID = 1, Name = "Active", IsActive = true },
                new PlayerData { ID = 2, Name = "Inactive", IsActive = false }
            });

            // Act
            var result = controller.GetAllPlayers(true);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var players = Assert.IsAssignableFrom<List<PlayerV1>>(ok.Value);
            Assert.Equal(2, players.Count);
        }

        [Fact]
        public void GetPlayer_ReturnsPlayer()
        {
            // Arrange
            mockDao.Setup(d => d.GetPlayerData(7)).Returns(new PlayerData { ID = 7, Name = "P7", IsActive = true });

            // Act
            var result = controller.GetPlayer(7);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var player = Assert.IsType<PlayerV1>(ok.Value);
            Assert.Equal(7, player.PlayerId);
        }

        [Fact]
        public void CreatePlayer_CallsDaoCreateNewPlayer()
        {
            // Arrange
            mockDao.Setup(d => d.CreateNewPlayer("New Player")).Returns(3);
            // Player.CreateNewPlayer returns new Player(newId, dao) which immediately calls GetPlayerData(newId)
            mockDao.Setup(d => d.GetPlayerData(3)).Returns(new PlayerData { ID = 3, Name = "New Player", IsActive = true });

            // Act
            var result = controller.CreatePlayer(new PlayerV1 { FirstName = "New", Surname = "Player", IsActive = true });

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            var player = Assert.IsType<PlayerV1>(created.Value);
            Assert.Equal(3, player.PlayerId);
            mockDao.Verify(d => d.CreateNewPlayer("New Player"), Times.Once);
            mockDao.Verify(d => d.UpdatePlayer(It.Is<PlayerData>(p => p.ID == 3)), Times.Once);
        }

        [Fact]
        public void UpdatePlayer_CallsDaoUpdatePlayer()
        {
            // Arrange
            mockDao.Setup(d => d.GetPlayerData(3)).Returns(new PlayerData { ID = 3, Name = "Old", IsActive = true });
            mockDao.Setup(d => d.UpdatePlayer(It.IsAny<PlayerData>()));

            // Act
            var result = controller.UpdatePlayer(new PlayerV1 { PlayerId = 3, FirstName = "Updated", Surname = "Player", IsActive = true });

            // Assert
            Assert.IsType<OkObjectResult>(result);
            mockDao.Verify(d => d.UpdatePlayer(It.Is<PlayerData>(p =>
                p.ID == 3 &&
                p.FirstName == "Updated" &&
                p.Surname == "Player" &&
                p.IsActive)), Times.Once);
        }
        [Fact]
        public void CreatePlayer_WithoutNickname_PassesNullNicknameToDao()
        {
            // Verifies the controller correctly maps a missing Nickname to null in PlayerData.
            // NOTE: This does NOT test the SQL layer - the DAO is mocked, so the DBNull.Value fix
            // in Dao.UpdatePlayer is covered by the integration test in PlayersIntegrationTest.
            //
            // Also note: CreatePlayer calls UpdatePlayer by design - CreateNewPlayer inserts a
            // minimal stub row, then UpdatePlayerFields/Save() persists all the remaining fields.

            // Arrange
            mockDao.Setup(d => d.CreateNewPlayer("PLayer One")).Returns(10);
            mockDao.Setup(d => d.GetPlayerData(10)).Returns(new PlayerData
            {
                ID = 10, Name = "PLayer One", FirstName = "PLayer", Surname = "One",
                IsRightHandBat = true, BowlingStyle = "RM", IsActive = true
            });

            // Act
            var result = controller.CreatePlayer(new PlayerV1
            {
                FirstName = "PLayer",
                Surname = "One",
                IsRightHandBat = true,
                BowlingStyle = "RM",
                IsActive = true
                // Nickname intentionally omitted (null)
            });

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.IsType<PlayerV1>(created.Value);
            mockDao.Verify(d => d.UpdatePlayer(It.Is<PlayerData>(p =>
                p.ID == 10 &&
                p.FirstName == "PLayer" &&
                p.Surname == "One" &&
                p.IsRightHandBat == true &&
                p.BowlingStyle == "RM" &&
                p.IsActive == true &&
                p.NickName == null)), Times.Once);
        }
    }
}
