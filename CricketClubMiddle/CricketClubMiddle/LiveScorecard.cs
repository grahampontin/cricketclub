using System.Collections.Generic;
using CricketClubDomain;
using CricketClubMiddle.Stats;

namespace CricketClubMiddle
{
    public class LiveScorecard
    {
        public BatsmanInningsDetails OnStrikeBatsman { get; set; }
        public BatsmanInningsDetails OtherBatsman { get; set; }
        public BatsmanInningsDetails LastBatsmanOut { get; set; }
        public string Opposition { get; set; }
        public int LastCompletedOver { get; set; }
        public int OversRemaining { get; set; }
        public bool DeclarationGame { get; set; }
        public int Score { get; set; }
        public int Wickets { get; set; }
        public decimal RunRate { get; set; }
        public Partnership CurrentPartnership { get; set; }
        public Partnership PreviousPartnership { get; set; }
        public FallOfWicket LastManOut { get; set; }

        public List<FallOfWicket> FallOfWickets { get; set; }

        public List<OverSummary> CompletedOvers { get; set; }

        public BowlerInningsDetails BowlerOneDetails { get; set; }
        public BowlerInningsDetails BowlerTwoDetails { get; set; }

        public LiveBattingCard LiveBattingCard { get; set; }
    }

    public class LiveBattingCard
    {
        public Dictionary<string, LiveBattingCardEntry> Players;
        public LiveExtras Extras;
    }

    public class LiveBattingCardEntry
    {
        public BatsmanInningsDetails BatsmanInningsDetails;
        public Wicket Wicket;
    }

    public class LiveExtras
    {
        public int Byes;
        public int LegByes;
        public int Wides;
        public int NoBalls;
        public int Penalty;
    }
}