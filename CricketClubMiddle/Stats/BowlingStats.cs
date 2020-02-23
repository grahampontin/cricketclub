using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDAL;
using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    public class BowlingStats
    {

        /// <summary>
        /// Create a new set of bowling stats
        /// </summary>
        /// <param name="MatchID">The id of the match</param>
        /// <param name="Us">is this for us (true), or for the opposition (false)</param>
        internal BowlingStats(int MatchID, ThemOrUs who)
        {
            Dao myDAO = new Dao();
            BowlingStatsData = (from a in myDAO.GetBowlingStats(MatchID, who)
                                select new BowlingStatsLine(a)).ToList();
            Who = who;
        }

        public ThemOrUs Who
        {
            get;
            set;
        }

        public List<BowlingStatsLine> BowlingStatsData
        {
            get;
            set;
        }

        public void Save()
        {
            Dao myDao = new Dao();
            List<BowlingStatsEntryData> data = (from a in BowlingStatsData select a._data).ToList();
            myDao.UpdateBowlingStats(data, Who);
        }
    }
}
