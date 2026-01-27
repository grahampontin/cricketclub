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
    public class TeamsControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly TeamsController controller;

        public TeamsControllerTests()
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            controller = new TeamsController(mockDao.Object);
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
