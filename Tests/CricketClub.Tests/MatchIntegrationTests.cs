using System;
using System.Collections.Generic;
using System.Linq;
using CricketClubDAL;
using CricketClubDomain;
using NUnit.Framework;

namespace CricketClub.Tests
{
    [Category("RequiresDatabase")]
    public class MatchIntegrationTests : IntegrationTestSupport
    {
        private readonly Dao dao = new Dao();
        private readonly List<int> _createdMatchIds = new List<int>();
        private readonly List<int> _createdTeamIds = new List<int>();
        private readonly List<int> _createdVenueIds = new List<int>();

        [TearDown]
        public void TearDown()
        {
            foreach (var matchId in _createdMatchIds)
            {
                try { dao.DeleteMatch(matchId); } catch { /* ignore cleanup errors */ }
            }
            _createdMatchIds.Clear();

            foreach (var venueId in _createdVenueIds)
            {
                try { dao.DeleteVenue(venueId); } catch { /* ignore cleanup errors */ }
            }
            _createdVenueIds.Clear();

            foreach (var teamId in _createdTeamIds)
            {
                try { dao.DeleteTeam(teamId); } catch { /* ignore cleanup errors */ }
            }
            _createdTeamIds.Clear();
        }

        [Test]
        public void CanCreateQueryAndUpdateMatch()
        {
            // Setup - create prerequisite data
            var opponentName = "Opp_" + Guid.NewGuid().ToString().Substring(0, 8);
            var opponentId = dao.CreateNewTeam(opponentName);
            _createdTeamIds.Add(opponentId);
            
            var venueName = "Ven_" + Guid.NewGuid().ToString().Substring(0, 8);
            var venueId = dao.CreateNewVenue(venueName, "http://test.com", "Test venue", null, null);
            _createdVenueIds.Add(venueId);

            // Create match
            var matchDate = DateTime.Now.Date;
            var matchTypeId = 1; // assuming this exists
            var matchId = dao.CreateNewMatch(opponentId, matchDate, venueId, matchTypeId, HomeOrAway.Home);
            _createdMatchIds.Add(matchId);
            Assert.True(matchId > 0);

            // Query
            var match = dao.GetMatchData(matchId);
            Assert.NotNull(match);
            Assert.AreEqual(matchId, match.ID);
            Assert.AreEqual(opponentId, match.OppositionID);
            Assert.AreEqual(venueId, match.VenueID);
            Assert.AreEqual(matchTypeId, match.MatchType);

            // Update
            match.WonToss = true;
            match.Batted = true;
            match.Overs = 50;
            match.CaptainID = 1;
            match.WicketKeeperID = 2;

            dao.UpdateMatch(match);

            // Query again
            var updatedMatch = dao.GetMatchData(matchId);
            Assert.NotNull(updatedMatch);
            Assert.AreEqual(true, updatedMatch.WonToss);
            Assert.AreEqual(true, updatedMatch.Batted);
            Assert.AreEqual(50, updatedMatch.Overs);
            Assert.AreEqual(1, updatedMatch.CaptainID);
            Assert.AreEqual(2, updatedMatch.WicketKeeperID);
        }

        [Test]
        public void CanGetAllMatches()
        {
            // Create a match to ensure we have at least one
            var opponentName = "OppAll_" + Guid.NewGuid().ToString().Substring(0, 8);
            var opponentId = dao.CreateNewTeam(opponentName);
            _createdTeamIds.Add(opponentId);
            
            var venueName = "VenAll_" + Guid.NewGuid().ToString().Substring(0, 8);
            var venueId = dao.CreateNewVenue(venueName, "http://test.com", "Test venue", null, null);
            _createdVenueIds.Add(venueId);

            var matchDate = DateTime.Now.Date;
            var matchTypeId = 1;
            var matchId = dao.CreateNewMatch(opponentId, matchDate, venueId, matchTypeId, HomeOrAway.Away);
            _createdMatchIds.Add(matchId);
            Assert.True(matchId > 0);

            // Get all matches
            var allMatches = dao.GetAllMatches();
            Assert.NotNull(allMatches);
            Assert.IsTrue(allMatches.Any());
            Assert.IsTrue(allMatches.Any(m => m.ID == matchId));
        }

        [Test]
        public void GetMatchDataHandlesAllBooleanFields()
        {
            // Setup
            var opponentId = dao.CreateNewTeam("Bool_" + Guid.NewGuid().ToString().Substring(0, 8));
            _createdTeamIds.Add(opponentId);
            var venueId = dao.CreateNewVenue("BoolV_" + Guid.NewGuid().ToString().Substring(0, 8), "http://test.com", "Test", null, null);
            _createdVenueIds.Add(venueId);
            var matchId = dao.CreateNewMatch(opponentId, DateTime.Now.Date, venueId, 1, HomeOrAway.Home);
            _createdMatchIds.Add(matchId);

            // Set various boolean fields
            var match = dao.GetMatchData(matchId);
            match.WonToss = true;
            match.Batted = false;
            match.Abandoned = false;
            match.WasDeclarationGame = true;
            match.TheyDeclared = true;
            match.WeDeclared = false;
            dao.UpdateMatch(match);

            // Verify
            var retrieved = dao.GetMatchData(matchId);
            Assert.AreEqual(true, retrieved.WonToss);
            Assert.AreEqual(false, retrieved.Batted);
            Assert.AreEqual(false, retrieved.Abandoned);
            Assert.AreEqual(true, retrieved.WasDeclarationGame);
            Assert.AreEqual(true, retrieved.TheyDeclared);
            Assert.AreEqual(false, retrieved.WeDeclared);
        }
    }
}
