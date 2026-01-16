using System;
using System.Linq;
using CricketClubDAL;
using NUnit.Framework;

namespace CricketClub.Tests
{
    public class VenueIntegrationTests : IntegrationTestSupport
    {
    
        private readonly Dao dao = new Dao();

        [Test]
        public void CanCreateQueryAndUpdateVenue()
        {
            // Create
            var name = "Test Venue " + Guid.NewGuid();
            var mapUrl = "http://test.com/map";
            var description = "Test Description";
            var lat = 51.5074m;
            var lng = -0.1278m;

            var venueId = dao.CreateNewVenue(name, mapUrl, description, lat, lng);
            Assert.True(venueId > 0);

            // Query
            var venue = dao.GetVenueData(venueId);
            Assert.AreEqual(name, venue.Name);
            Assert.AreEqual(mapUrl, venue.MapUrl);
            Assert.AreEqual(description, venue.Description);
            Assert.AreEqual(lat, venue.Coordinates.Item1);
            Assert.AreEqual(lng, venue.Coordinates.Item2);

            // Update
            var newName = name + " Updated";
            var newMapUrl = mapUrl + "/updated";
            var newDescription = description + " Updated";
            var newLat = lat + 1;
            var newLng = lng + 1;

            venue.Name = newName;
            venue.MapUrl = newMapUrl;
            venue.Description = newDescription;
            venue.Coordinates = new Tuple<decimal?, decimal?>(newLat, newLng);

            dao.UpdateVenue(venue);

            // Query again
            var updatedVenue = dao.GetVenueData(venueId);
            Assert.AreEqual(newName, updatedVenue.Name);
            Assert.AreEqual(newMapUrl, updatedVenue.MapUrl);
            Assert.AreEqual(newDescription, updatedVenue.Description);
            Assert.AreEqual(newLat, updatedVenue.Coordinates.Item1);
            Assert.AreEqual(newLng, updatedVenue.Coordinates.Item2);

            venue.Coordinates = new Tuple<decimal?, decimal?>(null, null);
            dao.UpdateVenue(venue);
            
            // Query again to check null coordinates
            var nullCoordsVenue = dao.GetVenueData(venueId);
            Assert.IsNull(nullCoordsVenue.Coordinates.Item1);
            Assert.IsNull(nullCoordsVenue.Coordinates.Item2);
        }
        
        [Test]
        public void CanCreateVenueWithNullCoordinates()
        {
            // Create with null coordinates
            var name = "Null Coords Venue " + Guid.NewGuid();
            var mapUrl = "http://test.com/map";
            var description = "Venue with null coordinates";
            decimal? lat = null;
            decimal? lng = null;

            var venueId = dao.CreateNewVenue(name, mapUrl, description, lat, lng);
            Assert.True(venueId > 0);

            // Query
            var venue = dao.GetVenueData(venueId);
            Assert.AreEqual(name, venue.Name);
            Assert.AreEqual(mapUrl, venue.MapUrl);
            Assert.AreEqual(description, venue.Description);
            Assert.IsNull(venue.Coordinates.Item1);
            Assert.IsNull(venue.Coordinates.Item2);
        }

        [Test]
        public void CanDeleteVenue()
        {
            // Create a venue
            var name = "Delete Test Venue " + Guid.NewGuid();
            var mapUrl = "http://test.com/map";
            var description = "Test venue to be deleted";
            var lat = 51.5074m;
            var lng = -0.1278m;

            var venueId = dao.CreateNewVenue(name, mapUrl, description, lat, lng);
            Assert.True(venueId > 0);

            // Verify it exists
            var venue = dao.GetVenueData(venueId);
            Assert.NotNull(venue);
            Assert.AreEqual(name, venue.Name);

            // Delete it
            dao.DeleteVenue(venueId);

            // Verify it's deleted by checking it doesn't appear in GetAllVenueData
            var allVenues = dao.GetAllVenueData();
            Assert.False(allVenues.Any(v => v.ID == venueId));
        }
    }
}