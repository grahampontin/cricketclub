using System;
using System.Collections.Generic;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using CricketClubMiddle.Stats;
using Moq;
using NUnit.Framework;

namespace CricketClub.Tests
{
    /// <summary>
    /// Unit tests for KeeperStats class to verify division by zero handling.
    /// These tests ensure that stats methods return 0 when there are no games rather than throwing divide by zero exceptions.
    /// </summary>
    [TestFixture]
    public class KeeperStatsUnitTests
    {
        private Player _testPlayer;
        private DateTime _fromDate;
        private DateTime _toDate;
        private List<MatchType> _matchTypes;
        private Mock<IDao> _mockDao;

        [SetUp]
        public void Setup()
        {
            // Create a mock DAO to avoid real database calls
            _mockDao = new Mock<IDao>();
            _mockDao.Setup(dao => dao.GetPlayerData(1))
                .Returns(new PlayerData
                {
                    ID = 1,
                    Name = "Test Keeper",
                    FirstName = "Test",
                    Surname = "Keeper",
                    IsActive = true
                });
            
            // Create player with mock DAO to avoid database calls
            _testPlayer = new Player(1, _mockDao.Object);
            _fromDate = DateTime.Now.AddYears(-1);
            _toDate = DateTime.Now;
            _matchTypes = new List<MatchType> { MatchType.Friendly };
        }

        [Test]
        public void GetCatchesPerMatch_WithNoGames_ReturnsZero()
        {
            // Arrange
            var keeperStats = new KeeperStats(_testPlayer, _fromDate, _toDate, _matchTypes, null);

            // Act
            var result = keeperStats.GetCatchesPerMatch();

            // Assert
            Assert.AreEqual(0, result, "GetCatchesPerMatch should return 0 when there are no games");
        }

        [Test]
        public void GetStumpingsPerMatch_WithNoGames_ReturnsZero()
        {
            // Arrange
            var keeperStats = new KeeperStats(_testPlayer, _fromDate, _toDate, _matchTypes, null);

            // Act
            var result = keeperStats.GetStumpingsPerMatch();

            // Assert
            Assert.AreEqual(0, result, "GetStumpingsPerMatch should return 0 when there are no games");
        }

        [Test]
        public void GetAverageByesPerMatch_WithNoGames_ReturnsZero()
        {
            // Arrange
            var keeperStats = new KeeperStats(_testPlayer, _fromDate, _toDate, _matchTypes, null);

            // Act
            var result = keeperStats.GetAverageByesPerMatch();

            // Assert
            Assert.AreEqual(0, result, "GetAverageByesPerMatch should return 0 when there are no games");
        }

        [Test]
        public void GetGames_WithNoMatchingGames_ReturnsZero()
        {
            // Arrange
            var keeperStats = new KeeperStats(_testPlayer, _fromDate, _toDate, _matchTypes, null);

            // Act
            var result = keeperStats.GetGames();

            // Assert
            Assert.AreEqual(0, result, "GetGames should return 0 when there are no matching games");
        }
    }
}
