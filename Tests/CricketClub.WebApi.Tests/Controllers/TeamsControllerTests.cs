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
    public class TeamsControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly TeamsController controller;

        public TeamsControllerTests()
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            // Mock IWebHostEnvironment — ContentRootPath points to a temp dir (no logo files exist, so fallback 0.png is used)
            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

            controller = new TeamsController(mockDao.Object, mockEnv.Object);
            TestDefaults.SetupHttpContext(controller);
        }

        [Fact]
        public void GetAllTeams_ExcludesUsTeam()
        {
            // Arrange
            mockDao.Setup(d => d.GetAllTeamData()).Returns(new[]
            {
                new TeamData { ID = 0, Name = "Us" },
                new TeamData { ID = 1, Name = "Opposition" }
            });

            // Act
            var result = controller.GetAllTeams();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var teams = Assert.IsAssignableFrom<List<TeamV1>>(ok.Value);
            Assert.Single(teams);
            Assert.Equal(1, teams[0].Id);
        }

        [Fact]
        public void GetTeam_ReturnsTeam()
        {
            // Arrange
            // Team objects use InternalCache. Ensure this test is deterministic by explicitly setting the cached value.
            CricketClubMiddle.InternalCache.GetInstance().Insert("team1", new TeamData { ID = 1, Name = "Opp" }, TimeSpan.FromHours(24));
            mockDao.Setup(d => d.GetTeamData(1)).Returns(new TeamData { ID = 1, Name = "Opp" });

            // Act
            var result = controller.GetTeam(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var team = Assert.IsType<TeamV1>(ok.Value);
            Assert.Equal(1, team.Id);
            Assert.Equal("Opp", team.Name);
        }

        [Fact]
        public void CreateTeam_CallsDaoCreateNewTeam()
        {
            // Arrange
            mockDao.Setup(d => d.CreateNewTeam("NewTeam")).Returns(2);
            mockDao.Setup(d => d.GetTeamData(2)).Returns(new TeamData { ID = 2, Name = "NewTeam" });

            // Act
            var result = controller.CreateTeam(new TeamV1 { Name = "NewTeam" });

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            var team = Assert.IsType<TeamV1>(created.Value);
            Assert.Equal(2, team.Id);
            mockDao.Verify(d => d.CreateNewTeam("NewTeam"), Times.Once);
        }

        [Fact]
        public void GetTeamSummaries_ReturnsSummaryForEachOppositionTeam()
        {
            // Arrange
            mockDao.Setup(d => d.GetAllTeamData()).Returns(new[]
            {
                new TeamData { ID = 0, Name = "Us" },
                new TeamData { ID = 5, Name = "Riverside CC", HomeVenueId = 10 },
                new TeamData { ID = 6, Name = "Oakwood CC", HomeVenueId = null }
            });

            mockDao.Setup(d => d.GetAllTeamStatsCache()).Returns(new Dictionary<int, TeamStatsCacheData>
            {
                [5] = new TeamStatsCacheData { TeamId = 5, Played = 10, Won = 7, Lost = 2, Abandoned = 1 },
                [6] = new TeamStatsCacheData { TeamId = 6, Played = 5,  Won = 1, Lost = 4, Abandoned = 0 }
            });

            mockDao.Setup(d => d.GetAllVenueData()).Returns(new List<VenueData>
            {
                new VenueData { ID = 10, Name = "Riverside Ground", Coordinates = new Tuple<decimal?, decimal?>(51m, 0m) }
            });

            // Act
            var result = controller.GetTeamSummaries();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var summaries = Assert.IsAssignableFrom<List<TeamSummaryV1>>(ok.Value);

            // "Us" (ID=0) must be excluded
            Assert.DoesNotContain(summaries, s => s.Id == 0);
            Assert.Equal(2, summaries.Count);

            // Ordered by name: Oakwood CC first, Riverside CC second
            Assert.Equal("Oakwood CC",   summaries[0].Name);
            Assert.Equal("Riverside CC", summaries[1].Name);

            // Stats for Riverside CC
            var riverside = summaries.Single(s => s.Id == 5);
            Assert.Equal(10, riverside.Played);
            Assert.Equal(7,  riverside.Won);
            Assert.Equal(2,  riverside.Lost);
            Assert.Equal(1,  riverside.NoResult);
            Assert.Equal("Riverside Ground", riverside.HomeVenueName);
            // WinPercentage should be fraction (7/10 = 0.7)
            Assert.Equal(0.7, riverside.WinPercentage, 5);

            // Team with no stats gets zeroes
            var oakwood = summaries.Single(s => s.Id == 6);
            Assert.Null(oakwood.HomeVenueName);
        }

        [Fact]
        public void GetTeamSummaries_DifficultyRating_AssignedCorrectly()
        {
            // Arrange — three teams ranked low/mid/high by win rate so all three bands are exercised
            mockDao.Setup(d => d.GetAllTeamData()).Returns(new[]
            {
                new TeamData { ID = 1, Name = "A" },
                new TeamData { ID = 2, Name = "B" },
                new TeamData { ID = 3, Name = "C" }
            });

            mockDao.Setup(d => d.GetAllTeamStatsCache()).Returns(new Dictionary<int, TeamStatsCacheData>
            {
                [1] = new TeamStatsCacheData { TeamId = 1, Played = 10, Won = 1  },  // lowest  → red
                [2] = new TeamStatsCacheData { TeamId = 2, Played = 10, Won = 5  },  // middle  → amber
                [3] = new TeamStatsCacheData { TeamId = 3, Played = 10, Won = 9  }   // highest → green
            });

            mockDao.Setup(d => d.GetAllVenueData()).Returns(new List<VenueData>());

            // Act
            var result = controller.GetTeamSummaries();
            var ok       = Assert.IsType<OkObjectResult>(result);
            var summaries = Assert.IsAssignableFrom<List<TeamSummaryV1>>(ok.Value);

            Assert.Equal("red",   summaries.Single(s => s.Id == 1).DifficultyRating);
            Assert.Equal("amber", summaries.Single(s => s.Id == 2).DifficultyRating);
            Assert.Equal("green", summaries.Single(s => s.Id == 3).DifficultyRating);
        }

        [Fact]
        public void UpdateTeam_CallsDaoUpdateTeam()
        {
            // Arrange
            mockDao.Setup(d => d.GetTeamData(2)).Returns(new TeamData { ID = 2, Name = "Old" });
            mockDao.Setup(d => d.UpdateTeam(It.IsAny<TeamData>()));

            // Act
            var result = controller.UpdateTeam(new TeamV1 { Id = 2, Name = "Updated" });

            // Assert
            Assert.IsType<OkObjectResult>(result);
            mockDao.Verify(d => d.UpdateTeam(It.Is<TeamData>(t => t.ID == 2 && t.Name == "Updated")), Times.Once);
        }
    }
}
