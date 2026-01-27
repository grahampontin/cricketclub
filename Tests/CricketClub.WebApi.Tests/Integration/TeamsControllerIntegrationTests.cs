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
using CricketClub.WebApi.Tests.Utils;

namespace CricketClub.WebApi.Tests.Integration
{
    public class TeamsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;
        private readonly Mock<IDao> mockDao;

        public TeamsControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            this.factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDao));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddScoped(_ => mockDao.Object);
                });
            });
        }

        [Fact]
        public async Task GetTeams_ReturnsJsonArray()
        {
            // Arrange
            mockDao.Setup(d => d.GetAllTeamData()).Returns(new[]
            {
                new TeamData { ID = 0, Name = "Us" },
                new TeamData { ID = 1, Name = "Opp" }
            });

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/teams");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var teams = JsonSerializer.Deserialize<List<TeamV1>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(teams);
            Assert.Single(teams);
            Assert.Equal(1, teams[0].Id);
        }
    }
}
