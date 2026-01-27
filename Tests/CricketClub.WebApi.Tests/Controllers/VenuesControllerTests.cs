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
    public class VenuesControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly VenuesController controller;

        public VenuesControllerTests()
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            controller = new VenuesController(mockDao.Object);
        }

        [Fact]
        public void GetAllVenues_ReturnsVenues()
        {
            // Arrange
            mockDao.Setup(d => d.GetAllVenueData()).Returns(new[]
            {
                new VenueData { ID = 1, Name = "V1", MapUrl = "m", Description = "d", Coordinates = new Tuple<decimal?, decimal?>(1m,2m) }
            });

            // Act
            var result = controller.GetAllVenues();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var venues = Assert.IsAssignableFrom<List<VenueV1>>(ok.Value);
            Assert.Single(venues);
            Assert.Equal(1, venues[0].Id);
        }

        [Fact]
        public void GetVenue_ReturnsVenue()
        {
            // Arrange
            mockDao.Setup(d => d.GetVenueData(2)).Returns(new VenueData { ID = 2, Name = "V2", MapUrl = "m", Description = "d", Coordinates = new Tuple<decimal?, decimal?>(null,null) });

            // Act
            var result = controller.GetVenue(2);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var venue = Assert.IsType<VenueV1>(ok.Value);
            Assert.Equal(2, venue.Id);
        }

        [Fact]
        public void CreateVenue_CallsDaoCreateNewVenue()
        {
            // Arrange
            mockDao.Setup(d => d.CreateNewVenue("New", "map", "desc", 1m, 2m)).Returns(5);

            // CreateVenue resolves the created venue by calling Venue.GetAll(_database)
            mockDao.Setup(d => d.GetAllVenueData()).Returns(new[]
            {
                new VenueData { ID = 4, Name = "Other", MapUrl = "m", Description = "d", Coordinates = new Tuple<decimal?, decimal?>(0m,0m) },
                new VenueData { ID = 5, Name = "New", MapUrl = "map", Description = "desc", Coordinates = new Tuple<decimal?, decimal?>(1m,2m) }
            });

            // Act
            var result = controller.CreateVenue(new VenueV1 { Name = "New", MapUrl = "map", Description = "desc", Latitude = 1, Longitude = 2 });

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result);
            var venue = Assert.IsType<VenueV1>(created.Value);
            Assert.Equal(5, venue.Id);
            mockDao.Verify(d => d.CreateNewVenue("New", "map", "desc", 1m, 2m), Times.Once);
        }

        [Fact]
        public void UpdateVenue_CallsDaoUpdateVenue()
        {
            // Arrange
            mockDao.Setup(d => d.GetVenueData(5)).Returns(new VenueData { ID = 5, Name = "Old", MapUrl = "m", Description = "d", Coordinates = new Tuple<decimal?, decimal?>(null,null) });
            mockDao.Setup(d => d.UpdateVenue(It.IsAny<VenueData>()));

            // Act
            var result = controller.UpdateVenue(new VenueV1 { Id = 5, Name = "Updated", MapUrl = "m2", Description = "d2", Latitude = null, Longitude = null });

            // Assert
            Assert.IsType<OkObjectResult>(result);
            mockDao.Verify(d => d.UpdateVenue(It.Is<VenueData>(v => v.ID == 5 && v.Name == "Updated")), Times.Once);
        }

        [Fact]
        public void DeleteVenue_CallsDaoDeleteVenue()
        {
            // Arrange
            mockDao.Setup(d => d.DeleteVenue(9));

            // Act
            var result = controller.DeleteVenue(9);

            // Assert
            Assert.IsType<NoContentResult>(result);
            mockDao.Verify(d => d.DeleteVenue(9), Times.Once);
        }
    }
}
