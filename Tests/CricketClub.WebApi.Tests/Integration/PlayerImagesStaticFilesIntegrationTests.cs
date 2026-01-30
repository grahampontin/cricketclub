using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CricketClub.WebApi.Tests.Integration
{
    public class PlayerImagesStaticFilesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public PlayerImagesStaticFilesIntegrationTests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task GetPlayerImage_ReturnsPng()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/images/players/0.png");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);

            var bytes = await response.Content.ReadAsByteArrayAsync();
            Assert.NotEmpty(bytes);
        }

        [Fact]
        public async Task GetMissingPlayerImage_Returns404()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/images/players/does-not-exist.png");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
