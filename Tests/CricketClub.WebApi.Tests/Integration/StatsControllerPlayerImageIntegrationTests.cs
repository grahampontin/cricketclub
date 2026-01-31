using System.Net;
using System.Text.Json;
using CricketClubDAL;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;
using CricketClub.WebApi.Tests.Utils;
using System.Collections.Generic;
using System.Linq;

namespace CricketClub.WebApi.Tests.Integration
{
    public class StatsControllerPlayerImageIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;
        private readonly Mock<IDao> mockDao;

        public StatsControllerPlayerImageIntegrationTests(WebApplicationFactory<Program> factory)
        {
            TestDefaults.ResetInternalCache();

            mockDao = new Mock<IDao>();
            TestDefaults.SetupSafeVenueAndTeamLookups(mockDao);

            mockDao.Setup(d => d.GetPlayerData(It.IsAny<int>()))
                .Returns((int id) => new CricketClubDomain.PlayerData { ID = id, Name = $"Player {id}", IsActive = true });

            // Player detail builds batting/bowling stats; return empty deterministic collections.
            mockDao.Setup(d => d.GetPlayerBattingStatsData(It.IsAny<int>()))
                .Returns(new List<CricketClubDomain.BattingCardLineData>());
            mockDao.Setup(d => d.GetPlayerBowlingStatsData(It.IsAny<int>()))
                .Returns(new List<CricketClubDomain.BowlingStatsEntryData>());
            mockDao.Setup(d => d.GetPlayerFieldingStatsData(It.IsAny<int>()))
                .Returns(new List<CricketClubDomain.BattingCardLineData>());

            // Player internally hydrates caches using the aggregate lookup methods; ensure they return non-null.
            mockDao.Setup(d => d.GetAllBattingStatsData())
                .Returns(Enumerable.Empty<CricketClubDomain.BattingCardLineData>().ToLookup(_ => 0));
            mockDao.Setup(d => d.GetAllBowlingStatsData())
                .Returns(Enumerable.Empty<CricketClubDomain.BowlingStatsEntryData>().ToLookup(_ => 0));
            mockDao.Setup(d => d.GetAllFieldingStatsData())
                .Returns(new Dictionary<int, List<CricketClubDomain.BattingCardLineData>>());

            this.factory = factory.WithDao(mockDao.Object);
        }

        [Fact]
        public async Task GetPlayerDetail_ReturnsPlayerImageUrl_NotBase64()
        {
            var client = factory.CreateClient();

            // Player 0 always has a placeholder image (0.png)
            var response = await client.GetAsync("/api/stats/player/0/detail");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var problem = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Expected 200 OK but got {(int)response.StatusCode} {response.StatusCode}. Body: {problem}");
            }

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
