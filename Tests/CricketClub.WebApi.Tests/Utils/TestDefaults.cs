#nullable disable
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
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
