using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CricketClubDomain
{
    public class BattingCardLineData : IStatsEntryData
    {
        public BattingCardLineData() { }

        public int PlayerID
        {
            get;
            set;
        }

        public int MatchID
        {
            get;
            set;
        }

        public int VenueID
        {
            get;
            set;
        }

        public string PlayerName
        {
            get;
            set;
        }

        public int Runs
        {
            get;
            set;
        }

        public int ModeOfDismissal
        {
            get;
            set;
        }

        /// <summary>
        /// Set this if it is a bowling card for the special team, else set the BowlerName
        /// </summary>
        public int BowlerID
        {
            get;
            set;
        }

        public string BowlerName
        {
            get;
            set;
        }

        /// <summary>
        /// Set this if it is a bowling card for the special team, else set the BowlerName
        /// </summary>
        public int FielderID
        {
            get;
            set;
        }

        public string FielderName
        {
            get;
            set;
        }

        public int Fours
        {
            get;
            set;
        }

        public int Sixes
        {
            get;
            set;
        }

        public int BattingAt
        {
            get;
            set;
        }

        public int Score
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

        public int BallsFaced { get; set; }
        public int DotBalls { get; set; }
    }
}
