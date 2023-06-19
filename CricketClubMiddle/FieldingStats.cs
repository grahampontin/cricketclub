using System;
using CricketClubDomain;

namespace CricketClubMiddle
{
    public class FieldingStats : IStatsEntryData
    {
        public int CatchesTaken { get; }
        public int RunOuts { get; }
        public int Stumpings { get; }

        public FieldingStats(int catchesTaken, int runOuts, int stumpings, Match match, Player player)
        {
            CatchesTaken = catchesTaken;
            RunOuts = runOuts;
            Stumpings = stumpings;
            MatchDate = match.MatchDate;
            MatchTypeID = (int)match.Type;
            VenueID = match.VenueID;
            MatchTypeID = match.ID;
            PlayerID = player.Id;
        }

        public DateTime MatchDate { get; set; }
        public int MatchTypeID { get; set; }
        public int VenueID { get; set; }
        public int MatchID { get; set; }
        public int PlayerID { get; set; }
    }
}