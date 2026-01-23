using CricketClub.WebApi.Domain;
using CricketClubMiddle.Stats;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace CricketClub.WebApi.Stats
{
    public class KeeperStatsRowData
    {
        public static readonly List<StatsColumnDefinitionV1> ColumnDefinitions = new List<StatsColumnDefinitionV1>()
        {
            new StatsColumnDefinitionV1("Name", "playerName", "LinkToPlayerStatsRenderer"),
            new StatsColumnDefinitionV1("Matches", "matches"),
            new StatsColumnDefinitionV1("Catches", "catches"),
            new StatsColumnDefinitionV1("Stumpings", "stumpings"),
            new StatsColumnDefinitionV1("Byes per Match", "byes"),
            new StatsColumnDefinitionV1("Bat Ave with Gloves", "aveWithGloves"),
            new StatsColumnDefinitionV1("Bat Ave without Gloves", "aveWithoutGloves"),
       
        };
    
    
        public int id { get; private set; }
        public string playerName { get; private set; }
        public int matches { get; private set; }
        public decimal catches { get; private set; }
        public decimal stumpings { get; private set; }
        public decimal byes { get; private set; }
        public decimal aveWithGloves { get; private set; }
        public decimal aveWithoutGloves { get; private set; }
    
        public KeeperStatsRowData(KeeperStats keeperStats)
        {
            id = keeperStats.Player.Id;
            playerName = keeperStats.Player.FormalName;
            matches = keeperStats.GetGames();
            catches = keeperStats.GetCatchesPerMatch();
            stumpings = keeperStats.GetStumpingsPerMatch();
            byes = keeperStats.GetAverageByesPerMatch();
            aveWithGloves = keeperStats.GetBattingAverageAsKeeper();
            aveWithoutGloves = keeperStats.GetBattingAverageNotAsKeeper(); 
        }
    }
}