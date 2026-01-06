using System.Diagnostics;
using System.Linq;
using CricketClubMiddle;
using log4net;
using NUnit.Framework;

namespace CricketClub.Tests
{
    public class PlayersIntegrationTest : IntegrationTestSupport
    {
        [Test]
        public void GetAllPlayersWithHydration()
        {
            var repo = LogManager.GetRepository();
            ((log4net.Repository.Hierarchy.Hierarchy)repo).Root.Level = log4net.Core.Level.Info;;

            var playersWithOutHydration = Player.GetAll();
            var playersWithHydration = Player.GetAll(true);

            
            var unhydratedPlayersById = playersWithOutHydration.ToDictionary(k => k.Id, v => v);
            foreach (var player in playersWithHydration)
            {
                var playerWithoutHydration = unhydratedPlayersById[player.Id];
                var unhyrdatedValue = playerWithoutHydration.NumberOfMatchesPlayedThisSeason;
                var hydratedValue = player.NumberOfMatchesPlayedThisSeason;
                Log.Info("Player " + player.FullName + " Matches this season: unhydrated=" + unhyrdatedValue + ", hydrated=" + hydratedValue);
                Assert.AreEqual(unhyrdatedValue, hydratedValue);
                
                unhyrdatedValue = playerWithoutHydration.GetWicketsTaken();
                hydratedValue = player.GetWicketsTaken();
                Log.Info("Player " + player.FullName + " Wickets taken: unhydrated=" + unhyrdatedValue + ", hydrated=" + hydratedValue);
                Assert.AreEqual(unhyrdatedValue, hydratedValue);
                
                
                unhyrdatedValue = playerWithoutHydration.GetCatchesTaken();
                hydratedValue = player.GetCatchesTaken();
                Log.Info("Player " + player.FullName + " Catches taken: unhydrated=" + unhyrdatedValue + ", hydrated=" + hydratedValue);
                Assert.AreEqual(unhyrdatedValue, hydratedValue);
                
                
            }
        }

        [Test]
        public void HydratingIsFaster()
        {
            var repo = LogManager.GetRepository();
            ((log4net.Repository.Hierarchy.Hierarchy)repo).Root.Level = log4net.Core.Level.Info;;
            var stopwatch = Stopwatch.StartNew();
            var unhydratd = Player.GetAll().Where(p => p.Id > 0)
                .OrderByDescending(p => p.NumberOfMatchesPlayedThisSeason)
                .ThenBy(p=>p.GetWicketsTaken())
                .ThenBy(p=>p.GetCatchesTaken()).ToList();
            var unhdratedTime = stopwatch.ElapsedMilliseconds;
            Log.Info("Unhydrated time: " + unhdratedTime + "ms");
            stopwatch.Reset();
            stopwatch.Start();
            var hydratd = Player.GetAll(true).Where(p => p.Id > 0)
                .OrderByDescending(p => p.NumberOfMatchesPlayedThisSeason).ToList();
            var hydratedTime = stopwatch.ElapsedMilliseconds;
            Log.Info("Hydrated time: " + hydratedTime + "ms");
            
            Assert.Less(hydratedTime, unhdratedTime,
                $"Hydrated time {hydratedTime}ms should be less than unhydrated time {unhdratedTime}ms");
            
        }
    }
}