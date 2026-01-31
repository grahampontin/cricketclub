using System.Diagnostics.CodeAnalysis;

namespace CricketClub.WebApi.Domain
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MatchStateUpdateV1
    {
        public int LastCompletedOver { get; set; }
        public int OnStrikeBatsmanId { get; set; }
        public OverV1 Over { get; set; }
        public PlayerStateV1[] Players { get; set; }
        public decimal RunRate { get; set; }
        public int Score { get; set; }
        public string[] Bowlers { get; set; }
        public int MatchId { get; set; }
        public string PreviousBowler { get; set; }
        public string PreviousBowlerButOne { get; set; }
        public PartnershipStubV1 Partnership { get; set; }
        public string NextState { get; set; }
        public int OppositionScore { get; set; }
        public int OppositionWickets { get; set; }
        public string OppositionName { get; set; }
        public string OppositionShortName { get; set; }
        public BowlerInningsDetailsV1[] BowlerDetails { get; set; }
    }
}
