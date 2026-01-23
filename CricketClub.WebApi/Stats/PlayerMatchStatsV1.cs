using System.Diagnostics.CodeAnalysis;
using CricketClub.WebApi.Domain;

namespace CricketClub.WebApi.Stats
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeEvident")]
    public class PlayerMatchStatsV1
    {
        public int id { get; private set; }
        public string matchDate { get; private set; }
        public string opposition { get; private set; }
        public int runs { get; private set; }
        public int batsAt { get; private set; }
        public string bowlingFigures { get; private set; }
        public string venue { get; private set; }
        public string format { get; private set; }

        
        public PlayerMatchStatsV1(MatchV1 match, BattingStatsRowData battingStatsRowData, BowlingStatsRowData bowlingStatsRowData)
        {
            id = match.Id;
            matchDate = match.Date;
            opposition = match.IsHome
                ? "The Village vs " + match.Opposition.Name
                : match.Opposition.Name + " vs The Village";
            batsAt = battingStatsRowData.batsAt;
            runs = battingStatsRowData.runs;
            bowlingFigures = bowlingStatsRowData.overs == 0 ? "-/-" : bowlingStatsRowData.wickets + "/" + bowlingStatsRowData.runs;
            venue = match.Venue.Name;
            format = match.Type;
        }

        public static List<StatsColumnDefinitionV1> ColumnDefs
        {
            get
            {
                return new List<StatsColumnDefinitionV1>()
                {
                    new StatsColumnDefinitionV1("Match", "opposition", "LinkToMatchReportRenderer"),
                    new StatsColumnDefinitionV1("Bat", "runs"),
                    new StatsColumnDefinitionV1("Bowl", "bowlingFigures"),
                    new StatsColumnDefinitionV1("Date", "matchDate"),
                    new StatsColumnDefinitionV1("At", "venue"),
                    new StatsColumnDefinitionV1("Format", "format"),
                };
            }
        }
    }
}