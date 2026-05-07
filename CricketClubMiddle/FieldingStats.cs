using System;
using CricketClubDomain;

namespace CricketClubMiddle
{
    public class FieldingStats : IStatsEntryData
    {
        public int CatchesTaken { get; }
        public int RunOuts { get; }
        public int Stumpings { get; }
        public int DropsCount { get; }

        public FieldingStats(int catchesTaken, int runOuts, int stumpings, Match match, Player player)
            : this(catchesTaken, runOuts, stumpings, dropsCount: 0, match, player)
        {
        }

        public FieldingStats(int catchesTaken, int runOuts, int stumpings, int dropsCount, Match match, Player player)
        {
            CatchesTaken = catchesTaken;
            RunOuts = runOuts;
            Stumpings = stumpings;
            DropsCount = dropsCount;
            MatchDate = match.MatchDate;
            MatchTypeID = (int)match.Type;
            VenueID = match.VenueID;
            MatchID = match.ID;
            PlayerID = player.Id;
        }

        public DateTime MatchDate { get; set; }
        public int MatchTypeID { get; set; }
        public int VenueID { get; set; }
        public int MatchID { get; set; }
        public int PlayerID { get; set; }
    }
}