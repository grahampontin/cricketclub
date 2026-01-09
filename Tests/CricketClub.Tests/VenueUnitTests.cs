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
    /// Unit tests demonstrating the testability of Venue class using the IDao interface with Moq.
    /// These tests show how to mock the DAO for unit testing without requiring a database.
    /// </summary>
    [TestFixture]
    public class VenueUnitTests
    {
        [Test]
        public void Venue_CanBeConstructedWithMockDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            mockDao.Setup(dao => dao.GetVenueData(1))
                .Returns(new VenueData
                {
                    ID = 1,
                    Name = "Test Venue",
                    MapUrl = "http://maps.example.com",
                    Description = "Test venue description",
                    Coordinates = new Tuple<decimal?, decimal?>(51.5m, -0.1m)
                });
            
            // Act - Create a venue with the mock DAO
            var venue = new Venue(1, mockDao.Object);
            
            // Assert - Verify the venue was created and the DAO was called
            Assert.IsNotNull(venue);
            Assert.AreEqual(1, venue.ID);
            Assert.AreEqual("Test Venue", venue.Name);
            mockDao.Verify(dao => dao.GetVenueData(1), Times.Once);
        }

        [Test]
        public void Venue_GetAll_CanBeCalledWithMockDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            mockDao.Setup(dao => dao.GetAllVenueData())
                .Returns(new List<VenueData>
                {
                    new VenueData { ID = 1, Name = "Venue 1" },
                    new VenueData { ID = 2, Name = "Venue 2" }
                });
            
            // Act
            var venues = Venue.GetAll(mockDao.Object);
            
            // Assert
            Assert.IsNotNull(venues);
            Assert.AreEqual(2, venues.Count);
            mockDao.Verify(dao => dao.GetAllVenueData(), Times.Once);
        }

        [Test]
        public void Venue_CreateNewVenue_CanBeCalledWithMockDao()
        {
            // Arrange - Create a mock DAO using Moq
            var mockDao = new Mock<IDao>();
            mockDao.Setup(dao => dao.CreateNewVenue("New Venue", "http://maps.example.com", "Test description", 51.5m, -0.1m))
                .Returns(88);
            mockDao.Setup(dao => dao.GetVenueData(88))
                .Returns(new VenueData
                {
                    ID = 88,
                    Name = "New Venue",
                    MapUrl = "http://maps.example.com",
                    Description = "Test description",
                    Coordinates = new Tuple<decimal?, decimal?>(51.5m, -0.1m)
                });
            
            // Act
            var newVenue = Venue.CreateNewVenue("New Venue", "http://maps.example.com", "Test description", 51.5m, -0.1m, mockDao.Object);
            
            // Assert
            Assert.IsNotNull(newVenue);
            Assert.AreEqual(88, newVenue.ID);
            mockDao.Verify(dao => dao.CreateNewVenue("New Venue", "http://maps.example.com", "Test description", 51.5m, -0.1m), Times.Once);
        }
    }
}
