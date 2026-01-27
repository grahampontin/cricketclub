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
    public class ScorecardsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;
        private readonly Mock<IDao> mockDao;

        public ScorecardsControllerIntegrationTests(WebApplicationFactory<Program> factory)
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
        public async Task GetScorecard_ReturnsValidJson()
        {
            // Arrange
            mockDao.Setup(d => d.GetMatchData(1)).Returns(new MatchData
            {
                ID = 1,
                Date = DateTime.Today,
                OppositionID = 1,
                VenueID = 1,
                MatchType = 1,
                HomeOrAway = "H",
                CaptainID = 1,
                WicketKeeperID = 2,
                Overs = 40,
                WonToss = true,
                Batted = true
            });

            // Match conditions needs player lookups
            mockDao.Setup(d => d.GetPlayerData(It.IsAny<int>()))
                .Returns((int id) => new PlayerData { ID = id, Name = "P" + id, IsActive = true });

            mockDao.Setup(d => d.GetBattingCard(It.IsAny<int>(), It.IsAny<ThemOrUs>())).Returns(new List<BattingCardLineData>());
            mockDao.Setup(d => d.GetBowlingStats(It.IsAny<int>(), It.IsAny<ThemOrUs>())).Returns(new List<BowlingStatsEntryData>());
            mockDao.Setup(d => d.GetFoWData(It.IsAny<int>(), It.IsAny<ThemOrUs>())).Returns(new List<FoWDataLine>());
            mockDao.Setup(d => d.GetExtras(It.IsAny<int>(), It.IsAny<ThemOrUs>())).Returns(new ExtrasData());

            var client = this.factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/scorecards/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var scorecard = JsonSerializer.Deserialize<MatchScorecardV1>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(scorecard);
            Assert.NotNull(scorecard.MatchConditions);
        }
    }
}
