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
    public class LiveScoringControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly LiveScoringController controller;

        public LiveScoringControllerTests()
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            controller = new LiveScoringController(mockDao.Object, TestDefaults.MockEnvironment().Object);
            TestDefaults.SetupHttpContext(controller);
        }

        [Fact]
        public void GetMatches_WithSeason_ReturnsMatches()
        {
            // Arrange
            mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData>
            {
                new MatchData { ID = 1, Date = new DateTime(2026, 6, 1), OppositionID = 1, VenueID = 1, MatchType = 1, HomeOrAway = "Home" }
            });

            // Act
            var result = controller.GetMatches(2026);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsAssignableFrom<List<LiveScoringMatchSummaryV1>>(ok.Value);
            Assert.Single(payload);

            var first = payload[0];
            Assert.Equal(LiveScoringMatchSummaryKindV1.Match, first.Kind);
            Assert.NotNull(first.Match);
            Assert.Null(first.BallByBall);
        }

        [Fact]
        public void GetMatches_Default_WithNoGamesOrFixtures_ReturnsEmpty()
        {
            // Arrange – nothing in progress, no upcoming fixtures
            mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData>());
            // GetInProgressMatchIds already returns empty via TestDefaults

            // Act
            var result = controller.GetMatches(null);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsAssignableFrom<List<LiveScoringMatchSummaryV1>>(ok.Value);
            Assert.Empty(payload);
        }

        [Fact]
        public void GetMatches_Default_OrphanedInProgressMatchId_DoesNotThrow()
        {
            // Arrange – GetInProgressMatchIds returns an ID that has no corresponding match data
            // (simulates a stale ballbyball_team row whose Matches record was deleted).
            // GetMatchData is deliberately NOT set up for ID 999 so Moq returns null (the default).
            mockDao.Setup(d => d.GetInProgressMatchIds()).Returns(new List<int> { 999 });
            mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData>());

            // Act – should not throw NullReferenceException
            var result = controller.GetMatches(null);

            // Assert – the orphaned ID is silently skipped; result is empty
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsAssignableFrom<List<LiveScoringMatchSummaryV1>>(ok.Value);
            Assert.Empty(payload);
        }

        [Fact]
        public void GetMatches_Default_WithActiveInProgressGame_ReturnsIt()
        {
            // Arrange – one match is genuinely in progress (ball-by-ball coverage started, innings InProgress)
            const int matchId = 77;
            mockDao.Setup(d => d.GetInProgressMatchIds()).Returns(new List<int> { matchId });
            mockDao.Setup(d => d.GetMatchData(matchId)).Returns(new MatchData
            {
                ID = matchId, OppositionID = 1, VenueID = 1, Date = DateTime.Today,
                MatchType = 1, HomeOrAway = "H", Overs = 20, WonToss = true, Batted = true
            });
            mockDao.Setup(d => d.GetInningsStatus(matchId)).Returns(new BallByBallInningsStatus
            {
                MatchId = matchId,
                OurInningsStatus = InningsStatus.InProgress,
                TheirInningsStatus = InningsStatus.NotStarted
            });
            mockDao.Setup(d => d.GetAllBallsForMatch(matchId)).Returns(new List<Over>());
            mockDao.Setup(d => d.GetPlayerStates(matchId)).Returns(new List<PlayerState>());
            mockDao.Setup(d => d.GetOppositionInnings(matchId))
                   .Returns(new OppositionInnings(new List<OppositionInningsDetails>()));
            // No upcoming fixtures
            mockDao.Setup(d => d.GetAllMatches()).Returns(new List<MatchData>());

            // Act
            var result = controller.GetMatches(null);

            // Assert – the in-progress match appears in the list
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsAssignableFrom<List<LiveScoringMatchSummaryV1>>(ok.Value);
            Assert.Single(payload);
            Assert.Equal(LiveScoringMatchSummaryKindV1.BallByBall, payload[0].Kind);
            Assert.Equal(matchId, payload[0].BallByBall?.MatchId);
        }

        // ── AbandonMatch endpoint ──────────────────────────────────────────────────

        [Fact]
        public void AbandonMatch_WhenNoCoverageInProgress_ReturnsBadRequest()
        {
            // Arrange
            const int matchId = 42;
            mockDao.Setup(d => d.GetMatchData(matchId)).Returns(new MatchData
            {
                ID = matchId, OppositionID = 1, VenueID = 1, Date = new DateTime(2026, 6, 1),
                MatchType = 1, HomeOrAway = "H", Overs = 40
            });
            mockDao.Setup(d => d.IsBallByBallCoverageInProgress(matchId)).Returns(false);

            // Act
            var result = controller.AbandonMatch(matchId, new AbandonMatchV1 { Reason = "rain" });

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void AbandonMatch_WhenCoverageInProgress_ReturnsNoContent()
        {
            // Arrange
            const int matchId = 43;
            mockDao.Setup(d => d.GetMatchData(matchId)).Returns(new MatchData
            {
                ID = matchId, OppositionID = 1, VenueID = 1, Date = new DateTime(2026, 6, 1),
                MatchType = 1, HomeOrAway = "H", Overs = 40, WonToss = true, Batted = true
            });
            // GetIsBallByBallInProgress() now uses batch GetInProgressMatchIds(), not the per-match method.
            mockDao.Setup(d => d.GetInProgressMatchIds()).Returns(new List<int> { matchId });
            mockDao.Setup(d => d.IsBallByBallCoverageInProgress(matchId)).Returns(true);
            mockDao.Setup(d => d.UpdateMatch(It.IsAny<MatchData>()));
            mockDao.Setup(d => d.GetInningsStatus(matchId)).Returns(new BallByBallInningsStatus
            {
                MatchId = matchId,
                OurInningsStatus = InningsStatus.InProgress,
                TheirInningsStatus = InningsStatus.NotStarted
            });
            mockDao.Setup(d => d.UpdateInningsStatus(It.IsAny<BallByBallInningsStatus>()));

            // GetCurrentBallByBallState internals
            mockDao.Setup(d => d.GetAllBallsForMatch(matchId)).Returns(new List<Over>());
            mockDao.Setup(d => d.GetPlayerStates(matchId)).Returns(new List<PlayerState>());
            mockDao.Setup(d => d.GetOppositionInnings(matchId)).Returns(new OppositionInnings(new List<OppositionInningsDetails>()));

            // Scorecard existence checks (all empty → will write from B2B, but no overs so LiveBattingCard == null)
            mockDao.Setup(d => d.GetBattingCard(matchId, It.IsAny<ThemOrUs>()))
                   .Returns(new List<BattingCardLineData>());
            mockDao.Setup(d => d.GetFoWData(matchId, It.IsAny<ThemOrUs>()))
                   .Returns(new List<FoWDataLine>());
            mockDao.Setup(d => d.GetBowlingStats(matchId, It.IsAny<ThemOrUs>()))
                   .Returns(new List<BowlingStatsEntryData>());

            // Act
            var result = controller.AbandonMatch(matchId, new AbandonMatchV1 { Reason = "rain" });

            // Assert: 204 No Content
            Assert.IsType<NoContentResult>(result);

            // Verify the match was marked abandoned and innings status was updated
            mockDao.Verify(d => d.UpdateMatch(It.Is<MatchData>(m => m.Abandoned)), Times.Once);
            mockDao.Verify(d => d.UpdateInningsStatus(It.Is<BallByBallInningsStatus>(
                s => s.OurInningsStatus == InningsStatus.Completed)), Times.Once);
        }

        [Fact]
        public void AbandonMatch_WhenBothInningsInProgress_ClosesOnlyInProgressOnes()
        {
            // Arrange – unusual state where both innings are shown as InProgress
            const int matchId = 44;
            mockDao.Setup(d => d.GetMatchData(matchId)).Returns(new MatchData
            {
                ID = matchId, OppositionID = 1, VenueID = 1, Date = new DateTime(2026, 6, 1),
                MatchType = 1, HomeOrAway = "H", Overs = 40
            });
            // GetIsBallByBallInProgress() now uses batch GetInProgressMatchIds(), not the per-match method.
            mockDao.Setup(d => d.GetInProgressMatchIds()).Returns(new List<int> { matchId });
            mockDao.Setup(d => d.IsBallByBallCoverageInProgress(matchId)).Returns(true);
            mockDao.Setup(d => d.UpdateMatch(It.IsAny<MatchData>()));
            mockDao.Setup(d => d.GetInningsStatus(matchId)).Returns(new BallByBallInningsStatus
            {
                MatchId = matchId,
                OurInningsStatus = InningsStatus.Completed,
                TheirInningsStatus = InningsStatus.InProgress
            });
            mockDao.Setup(d => d.UpdateInningsStatus(It.IsAny<BallByBallInningsStatus>()));
            mockDao.Setup(d => d.GetAllBallsForMatch(matchId)).Returns(new List<Over>());
            mockDao.Setup(d => d.GetPlayerStates(matchId)).Returns(new List<PlayerState>());
            mockDao.Setup(d => d.GetOppositionInnings(matchId)).Returns(new OppositionInnings(new List<OppositionInningsDetails>()));
            mockDao.Setup(d => d.GetBattingCard(matchId, It.IsAny<ThemOrUs>()))
                   .Returns(new List<BattingCardLineData>());
            mockDao.Setup(d => d.GetFoWData(matchId, It.IsAny<ThemOrUs>()))
                   .Returns(new List<FoWDataLine>());
            mockDao.Setup(d => d.GetBowlingStats(matchId, It.IsAny<ThemOrUs>()))
                   .Returns(new List<BowlingStatsEntryData>());

            // Act
            var result = controller.AbandonMatch(matchId, new AbandonMatchV1 { Reason = "bad light" });

            // Assert: 204 and only their innings was closed
            Assert.IsType<NoContentResult>(result);
            mockDao.Verify(d => d.UpdateInningsStatus(It.Is<BallByBallInningsStatus>(
                s => s.TheirInningsStatus == InningsStatus.Completed
                  && s.OurInningsStatus == InningsStatus.Completed)), Times.Once);
        }
    }
}
