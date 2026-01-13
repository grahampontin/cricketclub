using System;
using System.Collections.Generic;
using System.Linq;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using Moq;
using NUnit.Framework;

namespace CricketClub.Tests
{
    /// <summary>
    /// Unit tests demonstrating the testability of Match class using the IDao interface with Moq.
    /// These tests show how to mock the DAO for unit testing without requiring a database.
    /// </summary>
    [TestFixture]
    public class MatchUnitTests
    {
        [Test]
        public void Match_CanBeConstructedWithMockDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            mockDao.Setup(dao => dao.GetMatchData(1))
                .Returns(new MatchData
                {
                    ID = 1,
                    OppositionID = 2,
                    VenueID = 1,
                    Date = DateTime.Now.Date,
                    MatchType = 1,
                    HomeOrAway = "H",
                    WonToss = true,
                    Batted = true,
                    Overs = 50
                });
            
            // Act - Create a match with the mock DAO
            var match = new Match(1, mockDao.Object);
            
            // Assert - Verify the match was created and the DAO was called
            Assert.IsNotNull(match);
            Assert.AreEqual(1, match.ID);
            Assert.AreEqual(2, match.OppositionID);
            Assert.AreEqual(1, match.VenueID);
            Assert.AreEqual(50, match.Overs);
            Assert.IsTrue(match.WonToss);
            Assert.IsTrue(match.TossWinnerBatted);
            mockDao.Verify(dao => dao.GetMatchData(1), Times.Once);
        }

        [Test]
        public void Match_GetNextMatch_CanBeCalledWithMockDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            mockDao.Setup(dao => dao.GetNextMatch(It.IsAny<DateTime>()))
                .Returns(5);
            mockDao.Setup(dao => dao.GetMatchData(5))
                .Returns(new MatchData
                {
                    ID = 5,
                    OppositionID = 1,
                    VenueID = 1,
                    Date = DateTime.Now.AddDays(7),
                    MatchType = 1,
                    HomeOrAway = "H"
                });
            
            // Act
            var nextMatch = Match.GetNextMatch(mockDao.Object);
            
            // Assert
            Assert.IsNotNull(nextMatch);
            Assert.AreEqual(5, nextMatch.ID);
            mockDao.Verify(dao => dao.GetNextMatch(It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public void Match_GetLastMatch_CanBeCalledWithMockDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            mockDao.Setup(dao => dao.GetPreviousMatch(It.IsAny<DateTime>()))
                .Returns(3);
            mockDao.Setup(dao => dao.GetMatchData(3))
                .Returns(new MatchData
                {
                    ID = 3,
                    OppositionID = 1,
                    VenueID = 1,
                    Date = DateTime.Now.AddDays(-7),
                    MatchType = 1,
                    HomeOrAway = "A"
                });
            
            // Act
            var lastMatch = Match.GetLastMatch(mockDao.Object);
            
            // Assert
            Assert.IsNotNull(lastMatch);
            Assert.AreEqual(3, lastMatch.ID);
            mockDao.Verify(dao => dao.GetPreviousMatch(It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public void Match_Save_UsesInjectedDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            var matchData = new MatchData
            {
                ID = 1,
                OppositionID = 2,
                VenueID = 1,
                Date = DateTime.Now.Date,
                MatchType = 1,
                HomeOrAway = "H",
                WonToss = true,
                Batted = true,
                Overs = 50
            };
            mockDao.Setup(dao => dao.GetMatchData(1))
                .Returns(matchData);
            mockDao.Setup(dao => dao.UpdateMatch(It.IsAny<MatchData>()));
            
            // Act
            var match = new Match(1, mockDao.Object);
            match.Overs = 40;
            match.Save();
            
            // Assert
            mockDao.Verify(dao => dao.UpdateMatch(It.Is<MatchData>(m => m.Overs == 40)), Times.Once);
        }

        [Test]
        public void Match_GetMatchReport_UsesInjectedDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            var matchData = new MatchData
            {
                ID = 1,
                OppositionID = 2,
                VenueID = 1,
                Date = DateTime.Now.Date,
                MatchType = 1,
                HomeOrAway = "H"
            };
            mockDao.Setup(dao => dao.GetMatchData(1))
                .Returns(matchData);
            mockDao.Setup(dao => dao.GetMatchReport(1))
                .Returns(new MatchReportAndConditions
                {
                    Conditions = "Test conditions",
                    Report = "Test report"
                });
            
            // Act
            var match = new Match(1, mockDao.Object);
            var report = match.GetMatchReport();
            
            // Assert
            Assert.IsNotNull(report);
            Assert.AreEqual("Test conditions", report.Conditions);
            Assert.AreEqual("Test report", report.Report);
            mockDao.Verify(dao => dao.GetMatchReport(1), Times.Once);
        }
    }
}
