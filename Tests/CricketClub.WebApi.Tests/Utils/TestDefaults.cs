#nullable disable
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CricketClub.WebApi.Tests.Utils
{
    /// <summary>
    /// Shared test defaults for controller tests.
    /// 
    /// IMPORTANT: CricketClubMiddle uses a process-wide InternalCache.
    /// Tests should clear it to avoid cross-test pollution.
    /// </summary>
    public static class TestDefaults
    {
        public static void ResetInternalCache()
        {
            InternalCache.GetInstance().Clear();
        }

        /// <summary>
        /// Sets Request.Scheme/Host/PathBase on a controller so tests that build URLs don't get NullReferenceExceptions.
        /// </summary>
        public static void SetupHttpContext(ControllerBase controller)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("localhost");
            httpContext.Request.PathBase = PathString.Empty;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        public static void SetupSafeVenueAndTeamLookups(Mock<IDao> dao)
        {
            dao
                .Setup(d => d.GetVenueData(It.IsAny<int>()))
                .Returns((int id) => new VenueData
                {
                    ID = id,
                    Name = $"Venue {id}",
                    MapUrl = "http://maps.test.com",
                    Description = "Test venue",
                    Coordinates = new Tuple<decimal?, decimal?>(51.5m, -0.1m)
                });

            dao
                .Setup(d => d.GetTeamData(It.IsAny<int>()))
                .Returns((int id) => new TeamData
                {
                    ID = id,
                    Name = id == 0 ? "The Village" : $"Team {id}"
                });

            // Player.GetAll(fullyHydrated: true) expects these to be non-null.
            dao
                .Setup(d => d.GetAllBattingStatsData())
                .Returns(Enumerable.Empty<BattingCardLineData>().ToLookup(b => b.PlayerID));

            dao
                .Setup(d => d.GetAllBowlingStatsData())
                .Returns(Enumerable.Empty<BowlingStatsEntryData>().ToLookup(b => b.PlayerID));

            dao
                .Setup(d => d.GetAllFieldingStatsData())
                .Returns(new Dictionary<int, List<BattingCardLineData>>());

            // Team stats cache — return empty collections so Match.Save() and GetTeamDetails work without a real DB.
            dao
                .Setup(d => d.GetAllMatchScoreSummaries())
                .Returns(new List<MatchScoreSummaryData>());

            dao
                .Setup(d => d.GetAllTeamStatsCache())
                .Returns(new Dictionary<int, TeamStatsCacheData>());

            dao
                .Setup(d => d.UpsertTeamStatsCache(It.IsAny<TeamStatsCacheData>()));

            // Venue stats cache — return empty collections so VenueStatsRecalculator works without a real DB.
            dao
                .Setup(d => d.GetAllVenueStatsCache())
                .Returns(new Dictionary<int, VenueStatsCacheData>());

            dao
                .Setup(d => d.UpsertVenueStatsCache(It.IsAny<VenueStatsCacheData>()));

            // Match lookups used by GetMatchesByVenue
            dao
                .Setup(d => d.GetMatchesByVenue(It.IsAny<int>()))
                .Returns(new List<MatchData>());

            // Match lookups used by GetMatchesByTeam
            dao
                .Setup(d => d.GetMatchesByTeam(It.IsAny<int>()))
                .Returns(new List<MatchData>());

            // Some production code paths (e.g., ResultV1.FromInternal) call Team.OurTeam which uses new Dao().
            // To keep unit tests isolated from the real database, pre-populate the global cache with common team IDs.
            var cache = InternalCache.GetInstance();
            cache.Insert("team0", new TeamData { ID = 0, Name = "The Village" }, TimeSpan.FromHours(24));
            cache.Insert("team1", new TeamData { ID = 1, Name = "Team 1" }, TimeSpan.FromHours(24));
            cache.Insert("team2", new TeamData { ID = 2, Name = "Team 2" }, TimeSpan.FromHours(24));
        }
        public static Mock<IDao> CreateMockDao()
        {
            var mockDao = new Mock<IDao>();
            SetupSafeVenueAndTeamLookups(mockDao);
            return mockDao;
        }

        /// <summary>
        /// Returns a mock IWebHostEnvironment whose ContentRootPath points to a temp folder.
        /// Sufficient for tests where logo files do not need to physically exist
        /// (ResolveTeamLogoUrl will fall back to the 0.png placeholder URL).
        /// </summary>
        public static Mock<IWebHostEnvironment> MockEnvironment()
        {
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
            return env;
        }

        public static void SetupPlayerLookups(Mock<IDao> dao, params (int id, string name)[] players)
        {
            foreach (var (id, name) in players)
            {
                dao
                    .Setup(d => d.GetPlayerData(id))
                    .Returns(new PlayerData { ID = id, Name = name });
            }
        }
    }
}
