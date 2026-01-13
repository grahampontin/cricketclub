using System;
using System.Linq;
using CricketClubDAL;
using NUnit.Framework;

namespace CricketClub.Tests
{
    public class TeamIntegrationTests : IntegrationTestSupport
    {
        private readonly Dao _dao = new Dao();

        [Test]
        public void CanCreateQueryAndUpdateTeam()
        {
            // Create
            var teamName = "Test_" + Guid.NewGuid().ToString().Substring(0, 8);
            var teamId = _dao.CreateNewTeam(teamName);
            Assert.True(teamId > 0);

            // Query
            var team = _dao.GetTeamData(teamId);
            Assert.NotNull(team);
            Assert.AreEqual(teamName, team.Name);
            Assert.AreEqual(teamId, team.ID);

            // Update
            var newName = teamName + "_Upd";
            team.Name = newName;
            _dao.UpdateTeam(team);

            // Query again
            var updatedTeam = _dao.GetTeamData(teamId);
            Assert.NotNull(updatedTeam);
            Assert.AreEqual(newName, updatedTeam.Name);
        }

        [Test]
        public void CanGetAllTeams()
        {
            // Create a team to ensure we have at least one
            var teamName = "GetAll_" + Guid.NewGuid().ToString().Substring(0, 8);
            var teamId = _dao.CreateNewTeam(teamName);
            Assert.True(teamId > 0);

            // Get all teams
            var allTeams = _dao.GetAllTeamData();
            Assert.NotNull(allTeams);
            Assert.IsTrue(allTeams.Any());
            Assert.IsTrue(allTeams.Any(t => t.ID == teamId && t.Name == teamName));
        }

        [Test]
        public void CreateNewTeamReturnsSameIdIfTeamAlreadyExists()
        {
            // Create first time
            var teamName = "Dup_" + Guid.NewGuid().ToString().Substring(0, 8);
            var firstId = _dao.CreateNewTeam(teamName);
            Assert.True(firstId > 0);

            // Try to create again with same name
            var secondId = _dao.CreateNewTeam(teamName);
            Assert.AreEqual(firstId, secondId);
        }
    }
}
