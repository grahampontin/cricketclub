using CricketClub.WebApi.Domain;

namespace CricketClub.WebApi.Stats
{
    public class BestInningsStatsRowData
    {
        public static List<StatsColumnDefinitionV1> ColumnDefinitions = new List<StatsColumnDefinitionV1>()
        {
            new StatsColumnDefinitionV1("Player", "playerName", "LinkToPlayerStatsRenderer"),
            new StatsColumnDefinitionV1("Most Runs", "runs", "ParameterizedLinkToMatchReportRenderer"),
            new StatsColumnDefinitionV1("Most Fours", "fours", "ParameterizedLinkToMatchReportRenderer"),
            new StatsColumnDefinitionV1("Most Sixes", "sixes", "ParameterizedLinkToMatchReportRenderer"),
            new StatsColumnDefinitionV1("Most Dots", "dots", "ParameterizedLinkToMatchReportRenderer"),
            new StatsColumnDefinitionV1("Most Balls Faced", "ballsFaced", "ParameterizedLinkToMatchReportRenderer"),
            new StatsColumnDefinitionV1("Most Catches", "catches", "ParameterizedLinkToMatchReportRenderer"),
            new StatsColumnDefinitionV1("Most Stumpings", "stumpings", "ParameterizedLinkToMatchReportRenderer"),
            new StatsColumnDefinitionV1("Most Byes", "byes", "ParameterizedLinkToMatchReportRenderer"),
            new StatsColumnDefinitionV1("Most Run Outs", "runouts", "ParameterizedLinkToMatchReportRenderer"),
            new StatsColumnDefinitionV1("Most Wickets", "wickets", "ParameterizedLinkToMatchReportRenderer"),
            new StatsColumnDefinitionV1("Most Runs Conceded", "runsConceded", "ParameterizedLinkToMatchReportRenderer"),
        };
        
        public string playerName { get; private set; }
        public int runs { get; private set; }
        public int fours { get; private set; }
        public int sixes { get; private set; }
        public int dots { get; private set; }
        public int ballsFaced { get; private set; }
        public int catches { get; private set; }
        public int stumpings { get; private set; }
        public int byes { get; private set; }
        public int runouts { get; private set; }
        public int wickets { get; private set; }
        public int runsConceded { get; private set; }
        public decimal overs { get; private set; }
        public int runsMatchId { get; private set; }
        public int foursMatchId { get; private set; }
        public int sixesMatchId { get; private set; }
        public int dotsMatchId { get; private set; }
        public int ballsFacedMatchId { get; private set; }
        public int catchesMatchId { get; private set; }
        public int stumpingsMatchId { get; private set; }
        public int byesMatchId { get; private set; }
        public int runoutsMatchId { get; private set; }
        public int wicketsMatchId { get; private set; }
        public int runsConcededMatchId { get; private set; }
        public int oversMatchId { get; private set; }
        
        public BestInningsStatsRowData(InningsPerformanceStatsV1 i)
        {
            playerName = i.player.Name;
            runs = i.runs;
            fours = i.fours;
            sixes = i.sixes;
            dots = i.dots;
            ballsFaced = i.ballsFaced;
            catches = i.catches;
            stumpings = i.stumpings;
            runouts = i.runouts;
            wickets = i.wickets;
            runsConceded = i.runsConceded;
            overs = i.overs;
            runsMatchId = i.runsMatchId;
            foursMatchId = i.foursMatchId;
            sixesMatchId = i.sixesMatchId;
            dotsMatchId = i.dotsMatchId;
            ballsFacedMatchId = i.ballsFacedMatchId;
            catchesMatchId = i.catchesMatchId;
            stumpingsMatchId = i.stumpingsMatchId;
            runoutsMatchId = i.runoutsMatchId;
            wicketsMatchId = i.wicketsMatchId;
            runsConcededMatchId = i.runsConcededMatchId;
            oversMatchId = i.oversMatchId;
        }
    }
}