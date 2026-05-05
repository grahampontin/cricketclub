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
            var match = new CricketClubMiddle.Match(1, mockDao.Object);
            
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
            var nextMatch = CricketClubMiddle.Match.GetNextMatch(mockDao.Object);
            
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
            var lastMatch = CricketClubMiddle.Match.GetLastMatch(mockDao.Object);
            
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
            mockDao.Setup(dao => dao.GetAllMatchScoreSummaries())
                .Returns(new List<CricketClubDomain.MatchScoreSummaryData>());
            mockDao.Setup(dao => dao.UpsertTeamStatsCache(It.IsAny<CricketClubDomain.TeamStatsCacheData>()));
            
            // Act
            var match = new CricketClubMiddle.Match(1, mockDao.Object);
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
                .Returns(new MatchReportAndConditions(
                    "Test conditions",
                    "Test report",
                    "")
                );
            
            // Act
            var match = new CricketClubMiddle.Match(1, mockDao.Object);
            var report = match.GetMatchReport();
            
            // Assert
            Assert.IsNotNull(report);
            Assert.AreEqual("Test conditions", report.Conditions);
            Assert.AreEqual("Test report", report.Report);
            mockDao.Verify(dao => dao.GetMatchReport(1), Times.Once);
        }

        // ── AbandonMatch unit tests ───────────────────────────────────────────────
        // Helper: sets up the minimum DAO stubs needed for Match.Save() to succeed
        private static Mock<IDao> CreateMinimalMatchDao(int matchId, MatchData matchData)
        {
            var mockDao = new Mock<IDao>();
            mockDao.Setup(d => d.GetMatchData(matchId)).Returns(matchData);
            mockDao.Setup(d => d.UpdateMatch(It.IsAny<MatchData>()));
            mockDao.Setup(d => d.GetAllMatchScoreSummaries()).Returns(new List<MatchScoreSummaryData>());
            mockDao.Setup(d => d.GetAllTeamStatsCache()).Returns(new Dictionary<int, TeamStatsCacheData>());
            mockDao.Setup(d => d.UpsertTeamStatsCache(It.IsAny<TeamStatsCacheData>()));
            mockDao.Setup(d => d.GetAllVenueStatsCache()).Returns(new Dictionary<int, VenueStatsCacheData>());
            mockDao.Setup(d => d.UpsertVenueStatsCache(It.IsAny<VenueStatsCacheData>()));
            mockDao.Setup(d => d.GetMatchesByVenue(It.IsAny<int>())).Returns(new List<MatchData>());
            mockDao.Setup(d => d.GetMatchesByTeam(It.IsAny<int>())).Returns(new List<MatchData>());
            mockDao.Setup(d => d.GetTeamData(It.IsAny<int>())).Returns((int id) => new TeamData { ID = id, Name = $"Team {id}" });
            mockDao.Setup(d => d.GetVenueData(It.IsAny<int>())).Returns((int id) => new VenueData
            {
                ID = id, Name = $"Venue {id}",
                Coordinates = new Tuple<decimal?, decimal?>(51.5m, -0.1m)
            });
            // B2B state: no overs played so GetLiveScorecard returns LiveBattingCard == null
            mockDao.Setup(d => d.GetAllBallsForMatch(matchId)).Returns(new List<Over>());
            mockDao.Setup(d => d.GetPlayerStates(matchId)).Returns(new List<PlayerState>());
            mockDao.Setup(d => d.GetOppositionInnings(matchId)).Returns(new OppositionInnings(new List<OppositionInningsDetails>()));
            return mockDao;
        }

        [Test]
        public void AbandonMatch_SetsAbandonedFlagAndPersists()
        {
            // Arrange
            const int matchId = 100;
            var matchData = new MatchData
            {
                ID = matchId, OppositionID = 2, VenueID = 1,
                Date = DateTime.Today, MatchType = 1, HomeOrAway = "H", Overs = 40,
                WonToss = true, Batted = true
            };
            var mockDao = CreateMinimalMatchDao(matchId, matchData);
            mockDao.Setup(d => d.GetInningsStatus(matchId)).Returns(new BallByBallInningsStatus
            {
                MatchId = matchId,
                OurInningsStatus = InningsStatus.InProgress,
                TheirInningsStatus = InningsStatus.NotStarted
            });
            mockDao.Setup(d => d.UpdateInningsStatus(It.IsAny<BallByBallInningsStatus>()));

            InternalCache.GetInstance().Clear();

            // Act
            var match = new CricketClubMiddle.Match(matchId, mockDao.Object);
            match.AbandonMatch("rain");

            // Assert: UpdateMatch called with Abandoned = true
            mockDao.Verify(d => d.UpdateMatch(It.Is<MatchData>(m => m.Abandoned)), Times.Once);
        }

        [Test]
        public void AbandonMatch_ClosesInProgressInnings_LeavesNotStartedUntouched()
        {
            // Arrange
            const int matchId = 101;
            var matchData = new MatchData
            {
                ID = matchId, OppositionID = 2, VenueID = 1,
                Date = DateTime.Today, MatchType = 1, HomeOrAway = "H", Overs = 40,
                WonToss = true, Batted = true
            };
            var mockDao = CreateMinimalMatchDao(matchId, matchData);
            mockDao.Setup(d => d.GetInningsStatus(matchId)).Returns(new BallByBallInningsStatus
            {
                MatchId = matchId,
                OurInningsStatus = InningsStatus.InProgress,
                TheirInningsStatus = InningsStatus.NotStarted
            });
            BallByBallInningsStatus capturedStatus = null;
            mockDao.Setup(d => d.UpdateInningsStatus(It.IsAny<BallByBallInningsStatus>()))
                   .Callback<BallByBallInningsStatus>(s => capturedStatus = s);

            InternalCache.GetInstance().Clear();

            // Act
            var match = new CricketClubMiddle.Match(matchId, mockDao.Object);
            match.AbandonMatch("rain");

            // Assert
            Assert.IsNotNull(capturedStatus);
            Assert.AreEqual(InningsStatus.Completed, capturedStatus.OurInningsStatus);
            // Their innings was NotStarted — must NOT be changed
            Assert.AreEqual(InningsStatus.NotStarted, capturedStatus.TheirInningsStatus);
        }

        [Test]
        public void AbandonMatch_WhenNoInningsInProgress_DoesNotCallUpdateInningsStatus()
        {
            // Arrange: both already Completed (unusual but let's handle it gracefully)
            const int matchId = 102;
            var matchData = new MatchData
            {
                ID = matchId, OppositionID = 2, VenueID = 1,
                Date = DateTime.Today, MatchType = 1, HomeOrAway = "H", Overs = 40
            };
            var mockDao = CreateMinimalMatchDao(matchId, matchData);
            mockDao.Setup(d => d.GetInningsStatus(matchId)).Returns(new BallByBallInningsStatus
            {
                MatchId = matchId,
                OurInningsStatus = InningsStatus.Completed,
                TheirInningsStatus = InningsStatus.Completed
            });

            InternalCache.GetInstance().Clear();

            // Act
            var match = new CricketClubMiddle.Match(matchId, mockDao.Object);
            match.AbandonMatch("rain");

            // Assert: status update not required when nothing was InProgress
            mockDao.Verify(d => d.UpdateInningsStatus(It.IsAny<BallByBallInningsStatus>()), Times.Never);
        }

        [Test]
        public void AbandonMatch_WhenNoBallsPlayed_DoesNotWriteScorecardData()
        {
            // Arrange: innings in progress but no overs, so no B2B data to flush
            const int matchId = 103;
            var matchData = new MatchData
            {
                ID = matchId, OppositionID = 2, VenueID = 1,
                Date = DateTime.Today, MatchType = 1, HomeOrAway = "H", Overs = 40,
                WonToss = true, Batted = true
            };
            var mockDao = CreateMinimalMatchDao(matchId, matchData);
            mockDao.Setup(d => d.GetInningsStatus(matchId)).Returns(new BallByBallInningsStatus
            {
                MatchId = matchId,
                OurInningsStatus = InningsStatus.InProgress,
                TheirInningsStatus = InningsStatus.NotStarted
            });
            mockDao.Setup(d => d.UpdateInningsStatus(It.IsAny<BallByBallInningsStatus>()));

            InternalCache.GetInstance().Clear();

            // Act
            var match = new CricketClubMiddle.Match(matchId, mockDao.Object);
            match.AbandonMatch("rain");

            // Assert: no batting/bowling/FoW data written since no overs were bowled
            mockDao.Verify(d => d.UpdateScoreCard(It.IsAny<List<BattingCardLineData>>(), It.IsAny<int>(), It.IsAny<BattingOrBowling>()), Times.Never);
            mockDao.Verify(d => d.UpdateFoWData(It.IsAny<List<FoWDataLine>>(), It.IsAny<ThemOrUs>()), Times.Never);
            mockDao.Verify(d => d.UpdateBowlingStats(It.IsAny<List<BowlingStatsEntryData>>(), It.IsAny<ThemOrUs>()), Times.Never);
        }
    }
}
