using CricketClub.WebApi.Domain;
using CricketClubDomain;
using CricketClubMiddle.Stats;

namespace CricketClub.WebApi.Stats
{
    public class TeamStatsRowData
    {
        public static List<StatsColumnDefinitionV1> ColumnDefinitions = new List<StatsColumnDefinitionV1>()
        {
            new StatsColumnDefinitionV1("Name", "name"),
            new StatsColumnDefinitionV1("Matches", "matches"),
            new StatsColumnDefinitionV1("Wins", "wins"),
            new StatsColumnDefinitionV1("Losses", "losses"),
            new StatsColumnDefinitionV1("Draws", "draws"),
            new StatsColumnDefinitionV1("Ave Bat", "aveBatScore"),
            new StatsColumnDefinitionV1("Ave Bowl", "aveBowlScore"),
            new StatsColumnDefinitionV1("Wkts Taken", "wicketsTaken"),
            new StatsColumnDefinitionV1("Wkts Lost", "wicketsLost"),
            new StatsColumnDefinitionV1("LBWs Given", "lbwsGiven"),
            new StatsColumnDefinitionV1("LBWs Conceded", "lbwsConceded"),
       
        };
    
    
        public string name {get; private set; }
        public int wins {get; private set; }
        public int losses {get; private set; }
        public int draws {get; private set; }
        public decimal aveBatScore {get; private set; }
        public decimal aveBowlScore {get; private set; }
        public int wicketsTaken {get; private set; }
        public int wicketsLost {get; private set; }
        public int lbwsGiven {get; private set; }
        public int lbwsConceded {get; private set; }
        public int matches {get; private set; }
    
        public TeamStatsRowData(TeamStats teamStats)
        {
            name = teamStats.Team.Name;
            wins = teamStats.GetVictories();
            losses = teamStats.GetDefeats();
            draws = teamStats.GetDraws();
            aveBatScore = teamStats.GetAverageBattingScore();
            aveBowlScore = teamStats.GetAverageBowlingScore();
            wicketsTaken = teamStats.GetWicketsTaken();
            wicketsLost = teamStats.GetWicketsLost();
            lbwsGiven = teamStats.GetNumberOfDismissals(ModesOfDismissal.LBW);
            lbwsConceded = teamStats.GetNumberOfWickets(ModesOfDismissal.LBW);
            matches = teamStats.GetMatchesPlayed();           
        }
    }
}