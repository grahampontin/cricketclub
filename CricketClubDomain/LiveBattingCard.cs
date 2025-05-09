using System.Collections.Generic;

namespace CricketClubDomain
{
    public class LiveBattingCard
    {
        public Dictionary<string, LiveBattingCardEntry> Players;
        public LiveExtras Extras;
    }
}