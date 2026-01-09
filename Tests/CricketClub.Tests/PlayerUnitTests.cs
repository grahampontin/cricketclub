using System;
using System.Collections.Generic;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using Moq;
using NUnit.Framework;

namespace CricketClub.Tests
{
    /// <summary>
    /// Unit tests demonstrating the testability of Player class using the IDao interface with Moq.
    /// These tests show how to mock the DAO for unit testing without requiring a database.
    /// </summary>
    [TestFixture]
    public class PlayerUnitTests
    {
        [Test]
        public void Player_CanBeConstructedWithMockDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            mockDao.Setup(dao => dao.GetPlayerData(1))
                .Returns(new PlayerData
                {
                    ID = 1,
                    Name = "Test Player",
                    FirstName = "",
                    Surname = "",
                    IsActive = true
                });
            
            // Act - Create a player with the mock DAO
            var player = new Player(1, mockDao.Object);
            
            // Assert - Verify the player was created and the DAO was called
            Assert.IsNotNull(player);
            Assert.AreEqual(1, player.Id);
            Assert.AreEqual("Test Player", player.Name);
            mockDao.Verify(dao => dao.GetPlayerData(1), Times.Once);
        }

        [Test]
        public void Player_GetAll_CanBeCalledWithMockDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            mockDao.Setup(dao => dao.GetAllPlayers())
                .Returns(new List<PlayerData>
                {
                    new PlayerData { ID = 1, Name = "Player 1", FirstName = "", Surname = "" },
                    new PlayerData { ID = 2, Name = "Player 2", FirstName = "", Surname = "" }
                });
            
            // Act
            var players = Player.GetAll(false, mockDao.Object);
            
            // Assert
            Assert.IsNotNull(players);
            Assert.AreEqual(2, players.Count);
            mockDao.Verify(dao => dao.GetAllPlayers(), Times.Once);
        }

        [Test]
        public void Player_CreateNewPlayer_CanBeCalledWithMockDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            mockDao.Setup(dao => dao.CreateNewPlayer("New Player"))
                .Returns(99);
            mockDao.Setup(dao => dao.GetPlayerData(99))
                .Returns(new PlayerData
                {
                    ID = 99,
                    Name = "New Player",
                    FirstName = "",
                    Surname = "",
                    IsActive = true
                });
            
            // Act
            var newPlayer = Player.CreateNewPlayer("New Player", mockDao.Object);
            
            // Assert
            Assert.IsNotNull(newPlayer);
            Assert.AreEqual(99, newPlayer.Id);
            mockDao.Verify(dao => dao.CreateNewPlayer("New Player"), Times.Once);
        }
    }
}
