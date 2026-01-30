using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CricketClub.WebApi.Tests.Integration
{
    public class StatsControllerPlayerImageIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public StatsControllerPlayerImageIntegrationTests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task GetPlayerDetail_ReturnsPlayerImageUrl_NotBase64()
        {
            var client = factory.CreateClient();

            // Player 0 always has a placeholder image (0.png)
            var response = await client.GetAsync("/api/stats/player/0/detail");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            Assert.True(doc.RootElement.TryGetProperty("playerImageUrl", out var playerImageUrl));
            var url = playerImageUrl.GetString();

            Assert.False(string.IsNullOrWhiteSpace(url));
            Assert.Contains("/images/players/0.png", url);
            Assert.StartsWith("http", url);
        }
    }
}
