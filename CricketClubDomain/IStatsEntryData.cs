using System;

namespace CricketClubDomain
{
    public interface IStatsEntryData
    {
        DateTime MatchDate { get; set; }
        int MatchTypeID { get; set; }
        int VenueID { get; set; }
        int MatchID { get; set; }
    }
}