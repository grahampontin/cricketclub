using CricketClub.WebApi.Domain;
using CricketClubMiddle.Stats;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace CricketClub.WebApi.Stats
{
    public class CaptainStatsRowData
    {
        public static readonly List<StatsColumnDefinitionV1> ColumnDefinitions = new List<StatsColumnDefinitionV1>()
        {
            new StatsColumnDefinitionV1("Name", "playerName", "LinkToPlayerStatsRenderer"),
            new StatsColumnDefinitionV1("Matches", "matches"),
            new StatsColumnDefinitionV1("Wins", "wins"),
            new StatsColumnDefinitionV1("Losses", "losses"),
            new StatsColumnDefinitionV1("Win %", "pcWins"),
            new StatsColumnDefinitionV1("Toss %", "pcTossesWon"),
            new StatsColumnDefinitionV1("% Bats 1st", "pcChoseToBat"),
            new StatsColumnDefinitionV1("Ave As Captain", "aveAsCapt"),
            new StatsColumnDefinitionV1("Ave Not Captain", "aveNotCapt"),
       
        };
        public int id { get; private set; }
        public string playerName {get; private set; }
        public string matches {get; private set; }
        public string wins {get; private set; }
        public string losses {get; private set; }
        public string pcWins {get; private set; }
        public string pcTossesWon {get; private set; }
        public string pcChoseToBat {get; private set; }
        public decimal aveAsCapt {get; private set; }
        public decimal aveNotCapt {get; private set; }
    
        public CaptainStatsRowData(CaptainStats captainStats)
        {
            id = captainStats.Player.Id;
            playerName = captainStats.Player.FormalName;
            matches = captainStats.GetGamesInCharge().ToString();
            wins = captainStats.GetWins().ToString();
            losses = captainStats.GetLosses().ToString();
            pcWins = captainStats.GetPercentageGamesWon() + "%";
            pcTossesWon = captainStats.GetPercentageTossWon() + "%";
            pcChoseToBat = captainStats.GetPercentageChooseToBat() + "%";
            aveAsCapt = captainStats.GetBattingAverageAsCaptain();
            aveNotCapt = captainStats.GetBattingAverageNotAsCaptain();
        }
    }
}