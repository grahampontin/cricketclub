using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    /// <summary>
    /// A fall-of-wicket record for the opposition ball-by-ball innings.
    /// Mirrors <see cref="FallOfWicket"/> but uses batter names instead of player IDs.
    /// </summary>
    public class OppositionFallOfWicket
    {
        public OppositionFallOfWicket(
            int wicketNumber,
            int teamScore,
            string outgoingBatsmanName,
            int outgoingBatsmanScore,
            string notOutBatsmanName,
            int notOutBatsmanScore,
            string overAsString,
            OppositionWicket wicket,
            int bowlerPlayerId,
            string bowlerName,
            OppositionPartnership partnership)
        {
            WicketNumber = wicketNumber;
            TeamScore = teamScore;
            OutgoingBatsmanName = outgoingBatsmanName;
            OutgoingBatsmanScore = outgoingBatsmanScore;
            NotOutBatsmanName = notOutBatsmanName;
            NotOutBatsmanScore = notOutBatsmanScore;
            OverAsString = overAsString;
            Wicket = wicket;
            BowlerPlayerId = bowlerPlayerId;
            BowlerName = bowlerName;
            Partnership = partnership;
        }

        public int WicketNumber { get; }
        public int TeamScore { get; }
        public string OutgoingBatsmanName { get; }
        public int OutgoingBatsmanScore { get; }
        public string NotOutBatsmanName { get; }
        public int NotOutBatsmanScore { get; }
        public string OverAsString { get; }
        public OppositionWicket Wicket { get; }
        public int BowlerPlayerId { get; }
        public string BowlerName { get; }
        public OppositionPartnership Partnership { get; }
    }
}

