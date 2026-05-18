using System.Collections.Generic;

namespace CricketClub.WebApi.Domain
{
    public class InPlayScorecardV1
    {
        public BatsmanInningsDetailsV1 OnStrikeBatsman { get; set; }
        public BatsmanInningsDetailsV1 OtherBatsman { get; set; }
        public BatsmanInningsDetailsV1 LastBatsmanOut { get; set; }
        public string Opposition { get; set; }
        public int OurLastCompletedOver { get; set; }
        public int OversRemaining { get; set; }
        public bool DeclarationGame { get; set; }
        public int Score { get; set; }
        public int Wickets { get; set; }
        public decimal RunRate { get; set; }
        public PartnershipV1 CurrentPartnership { get; set; }
        public PartnershipV1 PreviousPartnership { get; set; }
        public FallOfWicketV1 LastManOut { get; set; }
        public List<FallOfWicketV1> FallOfWickets { get; set; }
        public List<OverSummaryV1> CompletedOvers { get; set; }
        public BowlerInningsDetailsV1 BowlerOneDetails { get; set; }
        public BowlerInningsDetailsV1 BowlerTwoDetails { get; set; }
        public LiveBattingCardV1 LiveBattingCard { get; set; }
        public int Overs { get; set; }
        public bool TossWinnerBatted { get; set; }
        public bool WonToss { get; set; }
        public string OurInningsStatus { get; set; }
        public string TheirInningsStatus { get; set; }
        public int TheirScore { get; set; }
        public int TheirWickets { get; set; }
        public int TheirOver { get; set; }
        public decimal TheirRunRate { get; set; }
        public bool IsFirstInnings { get; set; }
        public List<OppositionInningsDetailsV1> TheirCompletedOvers { get; set; }
        public bool IsMatchComplete { get; set; }
        public string ResultText { get; set; }
        public string OurInningsCommentary { get; set; }
        public string TheirInningsCommentary { get; set; }
        public List<BowlerInningsDetailsV1> LiveBowlingCard { get; set; }
        public List<PartnershipV1> Partnerships { get; set; }

        /// <summary>
        /// VCC players who have not yet come to the crease in the current innings, in batting order.
        /// </summary>
        public List<YetToBatEntryV1> YetToBat { get; set; }
    }
}
