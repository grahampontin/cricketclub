using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CricketClubDomain
{
    public class FoWDataLine
    {
        public FoWDataLine() { }

        public int MatchID
        {
            get;
            set;
        }

        /// <summary>
        /// This is the positions in the batting order, not the playerID
        /// </summary>
        public int OutgoingBatsman
        {
            get;
            set;
        }

        public int OutgoingBatsmanScore
        {
            get;
            set;
        }

        /// <summary>
        /// This is the positions in the batting order, not the playerID
        /// </summary>
        public int NotOutBatsman
        {
            get;
            set;
        }

        public int NotOutBatsmanScore
        {
            get;
            set;
        }

        public int Score
        {
            get;
            set;
        }

        public int Partnership
        {
            get;
            set;
        }

        public int OverNumber
        {
            get;
            set;
        }

        public int Wicket
        {
            get;
            set;
        }

        public ThemOrUs Who
        {
            get;
            set;
        }

    }
}
