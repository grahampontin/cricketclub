using CricketClub.WebApi.Domain;
using CricketClubDomain;
using CricketClubMiddle;
using MatchType = CricketClubDomain.MatchType;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace CricketClub.WebApi.Stats
{
    public class BattingStatsRowData
    {
        public static List<StatsColumnDefinitionV1> WithColumns(params StatsColumnDefinitionV1[] columnDefinitionV1s)
        {
            return new List<StatsColumnDefinitionV1>(columnDefinitionV1s);
        }
        
        public static StatsColumnDefinitionV1 MatchType = new StatsColumnDefinitionV1("Format", "tableKey");
        public static StatsColumnDefinitionV1 PlayerName = new StatsColumnDefinitionV1("Name", "tableKey", "LinkToPlayerStatsRenderer");
        public static StatsColumnDefinitionV1 BattingPosition = new StatsColumnDefinitionV1("Bats", "batsAt");
        public static StatsColumnDefinitionV1 MatchesPlayed = new StatsColumnDefinitionV1("Mat", "matches");
        public static StatsColumnDefinitionV1 Innings = new StatsColumnDefinitionV1("Innings", "innings");
        public static StatsColumnDefinitionV1 NotOuts = new StatsColumnDefinitionV1("NO", "notOuts");
        public static StatsColumnDefinitionV1 RunsScored = new StatsColumnDefinitionV1("Runs", "runs");
        public static StatsColumnDefinitionV1 HighScore = new StatsColumnDefinitionV1("HS", "highScore");
        public static StatsColumnDefinitionV1 BattingAverage = new StatsColumnDefinitionV1("Ave", "average");
        public static StatsColumnDefinitionV1 HundredsScored = new StatsColumnDefinitionV1("100s", "tons");
        public static StatsColumnDefinitionV1 FiftiesScored = new StatsColumnDefinitionV1("50s", "fifties");
        public static StatsColumnDefinitionV1 FoursHit = new StatsColumnDefinitionV1("4s", "fours");
        public static StatsColumnDefinitionV1 SixesHit = new StatsColumnDefinitionV1("6s", "sixes");
        public static StatsColumnDefinitionV1 CatchesTaken = new StatsColumnDefinitionV1("Ct", "catches");
        public static StatsColumnDefinitionV1 Stumpings = new StatsColumnDefinitionV1("St", "stumpings");
        public static StatsColumnDefinitionV1 RunOuts = new StatsColumnDefinitionV1("RO", "runOuts");


        public int id { get; private set; }
        public string tableKey { get; private set; }
        public int batsAt { get; private set; }
        public int matches { get; private set; }
        public int innings { get; private set; }
        public int notOuts { get; private set; }
        public int runs { get; private set; }
        public string highScore { get; private set; }
        public decimal average { get; private set; }
        public int tons { get; private set; }
        public int fifties { get; private set; }
        public int fours { get; private set; }
        public int sixes { get; private set; }
        public int catches { get; private set; }
        public int stumpings { get; private set; }
        public int runOuts { get; private set; }

        public BattingStatsRowData(Player player, DateTime startDate, DateTime endDate, List<MatchType> MatchTypes,
            Venue venue)
        {
            id = player.Id;
            tableKey = player.Name;
            batsAt = player.GetBattingPosition(startDate, endDate, MatchTypes, venue);
            matches = player.GetMatchesPlayed(startDate, endDate, MatchTypes, venue);
            innings = player.GetInnings(startDate, endDate, MatchTypes, venue);
            notOuts = player.GetNotOuts(startDate, endDate, MatchTypes, venue);
            tons = player.Get100SScored(startDate, endDate, MatchTypes, venue);
            fifties = player.Get50SScored(startDate, endDate, MatchTypes, venue);
            fours = player.Get4S(startDate, endDate, MatchTypes, venue);
            sixes = player.Get6S(startDate, endDate, MatchTypes, venue);
            catches = player.GetCatchesTaken(startDate, endDate, MatchTypes, venue);
            stumpings = player.GetStumpings(startDate, endDate, MatchTypes, venue);
            runOuts = player.GetRunOuts(startDate, endDate, MatchTypes, venue);
            highScore = player.GetHighScore(startDate, endDate, MatchTypes, venue).ToString();
            runs = player.GetRunsScored(startDate, endDate, MatchTypes, venue);
            if (player.GetHighScoreWasNotOut())
            {
                highScore += "*";
            }

            average = player.GetBattingAverage(startDate, endDate, MatchTypes, venue);
        }
        
        public BattingStatsRowData(Player player, Func<IStatsEntryData, bool> predicate, string tableKey)
        {
            id = player.Id;
            this.tableKey = tableKey;
            batsAt = player.GetBattingPosition(predicate)??11;
            matches = player.GetMatchesPlayed(predicate);
            innings = player.GetInnings(predicate);
            notOuts = player.GetNotOuts(predicate);
            tons = player.Get100SScored(predicate);
            fifties = player.Get50SScored(predicate);
            fours = player.Get4S(predicate);
            sixes = player.Get6S(predicate);
            catches = player.GetCatchesTaken(predicate);
            stumpings = player.GetStumpings(predicate);
            runOuts = player.GetRunOuts(predicate);
            highScore = player.GetHighScore(predicate).ToString();
            runs = player.GetRunsScored(predicate);
            if (player.GetHighScoreWasNotOut(predicate))
            {
                highScore += "*";
            }

            average = player.GetBattingAverage(predicate);
        }
    }
}
