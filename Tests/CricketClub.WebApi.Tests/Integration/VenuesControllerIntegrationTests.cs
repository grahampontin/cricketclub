#nullable disable
using System.Net;
using System.Text.Json;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CricketClub.WebApi.Tests.Integration
{
    public class VenuesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IDao> _mockDao;

        public VenuesControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _mockDao = new Mock<IDao>();
            
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Remove the existing IDao registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDao));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add mock IDao
                    services.AddScoped<IDao>(_ => _mockDao.Object);
                });
            });
        }

        [Fact]
        public async Task GetAllVenues_ShowsInvalidJsonResponse()
        {
            // Arrange
            var venueData1 = new VenueData 
            { 
                ID = 1, 
                Name = "Test Ground 1",
                MapUrl = "https://maps.google.com/test1",
                Description = "Test venue 1",
                Coordinates = new Tuple<decimal?, decimal?>(51.5m, -0.1m)
            };
            
            var venueData2 = new VenueData 
            { 
                ID = 2, 
                Name = "Test Ground 2",
                MapUrl = "https://maps.google.com/test2",
                Description = "Test venue 2",
                Coordinates = new Tuple<decimal?, decimal?>(52.5m, -1.1m)
            };

            _mockDao.Setup(d => d.GetAllVenueData()).Returns(new List<VenueData> { venueData1, venueData2 });

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/venues");

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            
            // Output the response to understand what's wrong
            System.Diagnostics.Debug.WriteLine("Response content:");
            System.Diagnostics.Debug.WriteLine(content);
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(content);
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task GetAllVenues_ShouldReturnValidJson()
        {
            // Arrange
            var venueData1 = new VenueData 
            { 
                ID = 1, 
                Name = "Test Ground 1",
                MapUrl = "https://maps.google.com/test1",
                Description = "Test venue 1",
                Coordinates = new Tuple<decimal?, decimal?>(51.5m, -0.1m)
            };
            
            var venueData2 = new VenueData 
            { 
                ID = 2, 
                Name = "Test Ground 2",
                MapUrl = "https://maps.google.com/test2",
                Description = "Test venue 2",
                Coordinates = new Tuple<decimal?, decimal?>(52.5m, -1.1m)
            };

            _mockDao.Setup(d => d.GetAllVenueData()).Returns(new List<VenueData> { venueData1, venueData2 });

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/venues");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            
            // This should not throw if JSON is valid
            var venues = JsonSerializer.Deserialize<List<VenueV1>>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            Assert.NotNull(venues);
            Assert.Equal(2, venues.Count);
            Assert.Equal("Test Ground 1", venues[0].Name);
            Assert.Equal("Test Ground 2", venues[1].Name);
        }

        [Fact]
        public async Task GetVenueById_ShouldReturnValidJson()
        {
            // Arrange
            var venueData = new VenueData 
            { 
                ID = 1, 
                Name = "Test Ground",
                MapUrl = "https://maps.google.com/test",
                Description = "Test venue",
                Coordinates = new Tuple<decimal?, decimal?>(51.5m, -0.1m)
            };

            _mockDao.Setup(d => d.GetVenueData(1)).Returns(venueData);

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/venues/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
            Assert.NotEmpty(content);
            
            // This should not throw if JSON is valid
            var venue = JsonSerializer.Deserialize<VenueV1>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            Assert.NotNull(venue);
            Assert.Equal(1, venue.Id);
            Assert.Equal("Test Ground", venue.Name);
        }
    }
}

