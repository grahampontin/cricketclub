using System.Diagnostics.CodeAnalysis;
using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    
    public class LiveScorecardV1
    {
        public LiveScorecard InPlayData { get; set; }
        public MatchScorecardV1 FinalScorecard { get; set; }
        public MatchReportV1 MatchReport { get; set; }
        public MatchV1 MatchData { get; set; }
        public ResultV1 Result { get; set; }
    }
}