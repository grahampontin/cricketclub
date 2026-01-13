using System.Collections.Generic;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using Moq;
using NUnit.Framework;

namespace CricketClub.Tests
{
    /// <summary>
    /// Unit tests demonstrating the testability of Team class using the IDao interface with Moq.
    /// These tests show how to mock the DAO for unit testing without requiring a database.
    /// </summary>
    [TestFixture]
    public class TeamUnitTests
    {
        [Test]
        public void Team_CanBeConstructedWithMockDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            mockDao.Setup(dao => dao.GetTeamData(1))
                .Returns(new TeamData
                {
                    ID = 1,
                    Name = "Test Team"
                });
            
            // Act - Create a team with the mock DAO
            var team = new Team(1, mockDao.Object);
            
            // Assert - Verify the team was created and the DAO was called
            Assert.IsNotNull(team);
            Assert.AreEqual(1, team.ID);
            Assert.AreEqual("Test Team", team.Name);
            mockDao.Verify(dao => dao.GetTeamData(1), Times.Once);
        }

        [Test]
        public void Team_GetAll_CanBeCalledWithMockDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            mockDao.Setup(dao => dao.GetAllTeamData())
                .Returns(new List<TeamData>
                {
                    new TeamData { ID = 1, Name = "Team 1" },
                    new TeamData { ID = 2, Name = "Team 2" }
                });
            
            // Act
            var teams = Team.GetAll(mockDao.Object);
            
            // Assert
            Assert.IsNotNull(teams);
            Assert.AreEqual(2, teams.Count);
            Assert.AreEqual("Team 1", teams[0].Name);
            Assert.AreEqual("Team 2", teams[1].Name);
            mockDao.Verify(dao => dao.GetAllTeamData(), Times.Once);
        }

        [Test]
        public void Team_CreateNewTeam_CanBeCalledWithMockDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            mockDao.Setup(dao => dao.CreateNewTeam("New Team"))
                .Returns(99);
            mockDao.Setup(dao => dao.GetTeamData(99))
                .Returns(new TeamData
                {
                    ID = 99,
                    Name = "New Team"
                });
            
            // Act
            var newTeam = Team.CreateNewTeam("New Team", mockDao.Object);
            
            // Assert
            Assert.IsNotNull(newTeam);
            Assert.AreEqual(99, newTeam.ID);
            Assert.AreEqual("New Team", newTeam.Name);
            mockDao.Verify(dao => dao.CreateNewTeam("New Team"), Times.Once);
        }

        [Test]
        public void Team_Save_UsesInjectedDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            var teamData = new TeamData { ID = 1, Name = "Test Team" };
            mockDao.Setup(dao => dao.GetTeamData(1))
                .Returns(teamData);
            mockDao.Setup(dao => dao.UpdateTeam(It.IsAny<TeamData>()));
            
            // Act
            var team = new Team(1, mockDao.Object);
            team.Name = "Updated Team";
            team.Save();
            
            // Assert
            mockDao.Verify(dao => dao.UpdateTeam(It.Is<TeamData>(t => t.Name == "Updated Team")), Times.Once);
        }
    }
}
