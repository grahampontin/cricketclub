using CricketClub.WebApi.Domain;
using CricketClubDomain;
using CricketClubMiddle;
using MatchType = CricketClubDomain.MatchType;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace CricketClub.WebApi.Stats
{
    public class BowlingStatsRowData
    {
        public static List<StatsColumnDefinitionV1> WithColumns(params StatsColumnDefinitionV1[] columnDefinitionV1s)
        {
            return new List<StatsColumnDefinitionV1>(columnDefinitionV1s);
        }

        public static readonly StatsColumnDefinitionV1 MatchType = new StatsColumnDefinitionV1("Format", "tableKey");
        public static readonly StatsColumnDefinitionV1 PlayerName = new StatsColumnDefinitionV1("Name", "tableKey", "LinkToPlayerStatsRenderer");
        public static readonly StatsColumnDefinitionV1 MatchesPlayed = new StatsColumnDefinitionV1("Mat", "matches");
        public static readonly StatsColumnDefinitionV1 BowlingAverage = new StatsColumnDefinitionV1("Ave", "average");
        public static readonly StatsColumnDefinitionV1 WicketsTaken = new StatsColumnDefinitionV1("Wkt", "wickets");
        public static readonly StatsColumnDefinitionV1 Economy = new StatsColumnDefinitionV1("Eco", "economy");
        public static readonly StatsColumnDefinitionV1 Fivefers = new StatsColumnDefinitionV1("5Fer", "fiveFers");
        public static readonly StatsColumnDefinitionV1 StrikeRate = new StatsColumnDefinitionV1("SR", "strikeRate");
        public static readonly StatsColumnDefinitionV1 OversBowled = new StatsColumnDefinitionV1("O", "overs");
        public static readonly StatsColumnDefinitionV1 RunsConceded = new StatsColumnDefinitionV1("R", "runs");
        public static readonly StatsColumnDefinitionV1 BestBowlingFigures = new StatsColumnDefinitionV1("BBM", "bbm");
        public int id { get; private set; }
        public string tableKey {get; private set; }
        public decimal average {get; private set; }
        public int wickets {get; private set; }
        public int matches {get; private set; }
        public decimal economy {get; private set; }
        public int fiveFers {get; private set; }
        public decimal strikeRate {get; private set; }
        public decimal overs {get; private set; }
        public int runs {get; private set; }
        public string bbm {get; private set; }
    
        public BowlingStatsRowData(Player player, DateTime startDate, DateTime endDate, List<MatchType> matchTypes,
            Venue venue)
        {
            id = player.Id;
            tableKey = player.Name;
            matches = player.GetMatchesPlayed(startDate, endDate, matchTypes, venue);
            average = player.GetBowlingAverage(startDate, endDate, matchTypes, venue);
            wickets = player.GetWicketsTaken(startDate, endDate, matchTypes, venue);
            economy = player.GetEconomy(startDate, endDate, matchTypes, venue);
            fiveFers = player.GetFiveFers(startDate, endDate, matchTypes, venue);
            strikeRate = player.GetStrikeRate(startDate, endDate, matchTypes, venue);
            overs = player.GetOversBowled(startDate, endDate, matchTypes, venue);
            runs = player.GetRunsConceeded(startDate, endDate, matchTypes, venue);
            bbm = player.GetBestMatchFigures(startDate, endDate, matchTypes, venue);
        
        }

        public BowlingStatsRowData(Player player, Func<IStatsEntryData, bool> predicate, string tableKey)
        {
            id = player.Id;
            this.tableKey = tableKey;
            matches = player.GetMatchesPlayed(predicate);
            average = player.GetBowlingAverage(predicate);
            wickets = player.GetWicketsTaken(predicate);
            economy = player.GetEconomy(predicate);
            fiveFers = player.GetFiveFers(predicate);
            strikeRate = player.GetStrikeRate(predicate);
            overs = player.GetOversBowled(predicate);
            runs = player.GetRunsConceeded(predicate);
            bbm = player.GetBestMatchFigures(predicate);
        }
    }
}