using System;
using CricketClubDAL;
using CricketClubDomain;
using NUnit.Framework;

namespace CricketClub.Tests
{
    public class DaoIntegrationTests
    {
    
        private readonly Dao _dao = new Dao();

        [Test]
        public void CanCreateQueryAndUpdateVenue()
        {
            // Create
            string name = "Test Venue " + Guid.NewGuid();
            string mapUrl = "http://test.com/map";
            string description = "Test Description";
            decimal lat = 51.5074m;
            decimal lng = -0.1278m;

            int venueId = _dao.CreateNewVenue(name, mapUrl, description, lat, lng);
            Assert.True(venueId > 0);

            // Query
            VenueData venue = _dao.GetVenueData(venueId);
            Assert.AreEqual(name, venue.Name);
            Assert.AreEqual(mapUrl, venue.MapUrl);
            Assert.AreEqual(description, venue.Description);
            Assert.AreEqual(lat, venue.Coordinates.Item1);
            Assert.AreEqual(lng, venue.Coordinates.Item2);

            // Update
            string newName = name + " Updated";
            string newMapUrl = mapUrl + "/updated";
            string newDescription = description + " Updated";
            decimal newLat = lat + 1;
            decimal newLng = lng + 1;

            venue.Name = newName;
            venue.MapUrl = newMapUrl;
            venue.Description = newDescription;
            venue.Coordinates = new Tuple<decimal?, decimal?>(newLat, newLng);

            _dao.UpdateVenue(venue);

            // Query again
            VenueData updatedVenue = _dao.GetVenueData(venueId);
            Assert.AreEqual(newName, updatedVenue.Name);
            Assert.AreEqual(newMapUrl, updatedVenue.MapUrl);
            Assert.AreEqual(newDescription, updatedVenue.Description);
            Assert.AreEqual(newLat, updatedVenue.Coordinates.Item1);
            Assert.AreEqual(newLng, updatedVenue.Coordinates.Item2);
        }
    }
}