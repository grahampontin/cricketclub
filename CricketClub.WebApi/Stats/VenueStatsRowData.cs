using System.Globalization;
using CricketClub.WebApi.Domain;
using CricketClubDomain;
using CricketClubMiddle.Stats;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace CricketClub.WebApi.Stats
{
    public class VenueStatsRowData {
        public static List<StatsColumnDefinitionV1> ColumnDefinitions = new List<StatsColumnDefinitionV1>()
        {
            new StatsColumnDefinitionV1("Name", "venueName"),
            new StatsColumnDefinitionV1("Matches", "matches"),
            new StatsColumnDefinitionV1("Wins", "wins"),
            new StatsColumnDefinitionV1("Losses", "losses"),
            new StatsColumnDefinitionV1("Ave VCC Score", "aveVccScore"),
            new StatsColumnDefinitionV1("Ave Oppo Score", "aveOppoScore"),
            new StatsColumnDefinitionV1("% Bat First", "pcTossWinnerBats"),
            new StatsColumnDefinitionV1("% Bat First Wins", "pcBatFirstWins"),
            new StatsColumnDefinitionV1("Ave Wkts Taken", "aveWicketsVCC"),
            new StatsColumnDefinitionV1("Ave Wkts Lost", "aveWicketsOppo"),
            new StatsColumnDefinitionV1("Ave LBWs PI", "aveLBW"),
            new StatsColumnDefinitionV1("Ave Catches PI", "aveCatch"),
            new StatsColumnDefinitionV1("Ave Bowled PI", "aveBowled"),
       
        };
    
        public string venueName {get; private set; }
        public int wins {get; private set; }
        public int losses {get; private set; }
        public decimal aveVccScore {get; private set; }
        public decimal aveOppoScore {get; private set; }
        public string pcTossWinnerBats {get; private set; }
        public string pcBatFirstWins {get; private set; }
        public int aveWicketsVCC {get; private set; }
        public int aveWicketsOppo {get; private set; }
        public decimal aveLBW {get; private set; }
        public decimal aveCatch {get; private set; }
        public decimal aveBowled {get; private set; }
        public int matches {get; private set; }
    
        public VenueStatsRowData(VenueStats venueStats)
        {
            venueName = venueStats.Venue.Name;
            wins = venueStats.GetVillagWins();
            losses = venueStats.GetVillagLosses();
            aveVccScore = venueStats.GetAverageVillageScore();
            aveOppoScore = venueStats.GetAverageOpponentScore();
            pcTossWinnerBats = venueStats.GetPercentageTossWinnerBats().ToString(CultureInfo.InvariantCulture) + "%";
            pcBatFirstWins = venueStats.GetPercentageTeamBattingFirstWins().ToString(CultureInfo.InvariantCulture) + "%";
            aveWicketsVCC = venueStats.GetAverageWicketsTakenByVillage();
            aveWicketsOppo = venueStats.GetAverageWicketsTakenByOpposition();
            aveLBW = venueStats.GetNumberOfWicketsPerInnings(ModesOfDismissal.LBW);
            aveCatch = venueStats.GetNumberOfWicketsPerInnings(ModesOfDismissal.Caught);
            aveBowled = venueStats.GetNumberOfWicketsPerInnings(ModesOfDismissal.Bowled);
            matches = venueStats.GetMatchesPlayed();
        }
    }
}