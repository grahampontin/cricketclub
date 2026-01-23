using CricketClub.WebApi.Domain;
using CricketClubDomain;
using CricketClubMiddle;
using CricketClubMiddle.Stats;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace CricketClub.WebApi.Stats
{
    public class MatchStatsRowData
    {
        public static List<StatsColumnDefinitionV1> ColumnDefinitions = new List<StatsColumnDefinitionV1>()
        {
            new StatsColumnDefinitionV1("Match", "match"),
            new StatsColumnDefinitionV1("VCC Runs", "vccRuns"),
            new StatsColumnDefinitionV1("Oppo Runs", "oppoRuns"),
            new StatsColumnDefinitionV1("Margin of Victory (defeat)", "victoryMargin"),
            new StatsColumnDefinitionV1("Catches", "catchesAll"),
            new StatsColumnDefinitionV1("VCC Catches", "catchesVcc"),
            new StatsColumnDefinitionV1("LBWs", "lbwsAll"),
            new StatsColumnDefinitionV1("VCC LBWs", "lbwsVcc"),
            new StatsColumnDefinitionV1("Bowleds", "bowledsAll"),
            new StatsColumnDefinitionV1("VCC Bowleds", "bowledsVcc"),
            new StatsColumnDefinitionV1("Stumpings", "stumpingsAll"),
            new StatsColumnDefinitionV1("VCC Stumpings", "stumpingsVcc"),
            new StatsColumnDefinitionV1("Run Outs", "runoutsAll"),
            new StatsColumnDefinitionV1("VCC Run Outs", "runoutsVcc"),
            new StatsColumnDefinitionV1("Extras Conceded (VCC)", "extrasVcc"),
            new StatsColumnDefinitionV1("Extras Conceded (Oppo)", "extrasOppo"),
       
        };
    
    
        public string match { get; private set; }
        public int vccRuns { get; private set; }
        public int oppoRuns { get; private set; }
        public int victoryMargin { get; private set; }
        public int catchesAll { get; private set; }
        public int catchesVcc { get; private set; }
        public int lbwsAll { get; private set; }
        public int lbwsVcc { get; private set; }
        public int bowledsAll { get; private set; }
        public int bowledsVcc { get; private set; }
        public int stumpingsAll { get; private set; }
        public int stumpingsVcc { get; private set; }
        public int runoutsAll { get; private set; }
        public int runoutsVcc { get; private set; }
        public int extrasVcc { get; private set; }
        public int extrasOppo { get; private set; }

        public MatchStatsRowData(Match matchData)
        {
            var villageScore = matchData.HomeOrAway == HomeOrAway.Home
                ? matchData.GetTeamScore(matchData.HomeTeam)
                : matchData.GetTeamScore(matchData.AwayTeam);
            var oppoScore = matchData.HomeOrAway == HomeOrAway.Away
                ? matchData.GetTeamScore(matchData.HomeTeam)
                : matchData.GetTeamScore(matchData.AwayTeam);

            match = matchData.Description;
            vccRuns = villageScore;
            oppoRuns = oppoScore;
            victoryMargin = (villageScore - oppoScore);
            catchesAll = CountForAll(matchData, d => d.Dismissal == ModesOfDismissal.Caught);
            catchesVcc = CountForOppoBatting(matchData, d => d.Dismissal == ModesOfDismissal.Caught);
            lbwsAll = CountForAll(matchData, d => d.Dismissal == ModesOfDismissal.LBW);
            lbwsVcc = CountForOppoBatting(matchData, d => d.Dismissal == ModesOfDismissal.LBW);
            bowledsAll = CountForAll(matchData, d => d.Dismissal == ModesOfDismissal.Bowled);
            bowledsVcc = CountForOppoBatting(matchData, d => d.Dismissal == ModesOfDismissal.Bowled);
            stumpingsAll = CountForAll(matchData, d => d.Dismissal == ModesOfDismissal.Stumped);
            stumpingsVcc = CountForOppoBatting(matchData, d => d.Dismissal == ModesOfDismissal.Stumped);
            runoutsAll = CountForAll(matchData, d => d.Dismissal == ModesOfDismissal.RunOut);
            runoutsVcc = CountForOppoBatting(matchData, d => d.Dismissal == ModesOfDismissal.RunOut);
            extrasVcc = matchData.GetTheirBattingScoreCard().Extras;
            extrasOppo = matchData.GetOurBattingScoreCard().Extras;
        }

        private static int CountForOppoBatting(Match CurrentStats, Func<BattingCardLine, bool> predicate)
        {
            return CurrentStats.GetTheirBattingScoreCard()
                .ScorecardData.Count(predicate);
        }

        private static int CountForVccBatting(Match CurrentStats, Func<BattingCardLine, bool> predicate)
        {
            return CurrentStats.GetOurBattingScoreCard()
                .ScorecardData.Count(predicate);
        }

        private static int CountForAll(Match CurrentStats, Func<BattingCardLine, bool> predicate)
        {
            return (CurrentStats.GetOurBattingScoreCard().ScorecardData
                .Count(predicate) + CurrentStats.GetTheirBattingScoreCard()
                .ScorecardData.Count(predicate));
        }
    }
}