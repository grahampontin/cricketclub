using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CricketClubDomain
{
    public class BowlingStatsEntryData : IStatsEntryData
    {
        public BowlingStatsEntryData() {}

        public int PlayerID
        {
            get;
            set;
        }

        public string PlayerName
        {
            get;
            set;
        }

        public int MatchID
        {
            get;
            set;
        }

        public decimal Overs
        {
            get;
            set;

        }

        public int Runs
        {
            get;
            set;
        }

        public int Wickets
        {
            get;
            set;
        }

        public int Maidens
        {
            get;
            set;
        }

        public DateTime MatchDate
        {
            get;
            set;
        }

        public int MatchTypeID
        {
            get;
            set;
        }

        public int VenueID
        {
            get;
            set;
        }

    }
}
