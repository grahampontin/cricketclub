using System;
using System.Linq;
using CricketClubDAL;
using CricketClubDomain;
using NUnit.Framework;

namespace CricketClub.Tests
{
    public class MatchIntegrationTests : IntegrationTestSupport
    {
        private readonly Dao _dao = new Dao();

        [Test]
        public void CanCreateQueryAndUpdateMatch()
        {
            // Setup - create prerequisite data
            var opponentName = "Opp_" + Guid.NewGuid().ToString().Substring(0, 8);
            var opponentId = _dao.CreateNewTeam(opponentName);
            
            var venueName = "Ven_" + Guid.NewGuid().ToString().Substring(0, 8);
            var venueId = _dao.CreateNewVenue(venueName, "http://test.com", "Test venue", null, null);

            // Create match
            var matchDate = DateTime.Now.Date;
            var matchTypeId = 1; // assuming this exists
            var matchId = _dao.CreateNewMatch(opponentId, matchDate, venueId, matchTypeId, HomeOrAway.Home);
            Assert.True(matchId > 0);

            // Query
            var match = _dao.GetMatchData(matchId);
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

            _dao.UpdateMatch(match);

            // Query again
            var updatedMatch = _dao.GetMatchData(matchId);
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
            var opponentId = _dao.CreateNewTeam(opponentName);
            
            var venueName = "VenAll_" + Guid.NewGuid().ToString().Substring(0, 8);
            var venueId = _dao.CreateNewVenue(venueName, "http://test.com", "Test venue", null, null);

            var matchDate = DateTime.Now.Date;
            var matchTypeId = 1;
            var matchId = _dao.CreateNewMatch(opponentId, matchDate, venueId, matchTypeId, HomeOrAway.Away);
            Assert.True(matchId > 0);

            // Get all matches
            var allMatches = _dao.GetAllMatches();
            Assert.NotNull(allMatches);
            Assert.IsTrue(allMatches.Any());
            Assert.IsTrue(allMatches.Any(m => m.ID == matchId));
        }

        [Test]
        public void GetMatchDataHandlesAllBooleanFields()
        {
            // Setup
            var opponentId = _dao.CreateNewTeam("Bool_" + Guid.NewGuid().ToString().Substring(0, 8));
            var venueId = _dao.CreateNewVenue("BoolV_" + Guid.NewGuid().ToString().Substring(0, 8), "http://test.com", "Test", null, null);
            var matchId = _dao.CreateNewMatch(opponentId, DateTime.Now.Date, venueId, 1, HomeOrAway.Home);

            // Set various boolean fields
            var match = _dao.GetMatchData(matchId);
            match.WonToss = true;
            match.Batted = false;
            match.Abandoned = false;
            match.WasDeclarationGame = true;
            match.TheyDeclared = true;
            match.WeDeclared = false;
            _dao.UpdateMatch(match);

            // Verify
            var retrieved = _dao.GetMatchData(matchId);
            Assert.AreEqual(true, retrieved.WonToss);
            Assert.AreEqual(false, retrieved.Batted);
            Assert.AreEqual(false, retrieved.Abandoned);
            Assert.AreEqual(true, retrieved.WasDeclarationGame);
            Assert.AreEqual(true, retrieved.TheyDeclared);
            Assert.AreEqual(false, retrieved.WeDeclared);
        }
    }
}
