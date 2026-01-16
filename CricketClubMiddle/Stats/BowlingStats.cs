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
        private readonly IDao dao;

        /// <summary>
        /// Create a new set of bowling stats
        /// </summary>
        /// <param name="MatchID">The id of the match</param>
        /// <param name="Us">is this for us (true), or for the opposition (false)</param>
        public BowlingStats(int MatchID, ThemOrUs who) : this(MatchID, who, new Dao())
        {
        }

        /// <summary>
        /// Create a new set of bowling stats (with DAO injection for testing)
        /// </summary>
        /// <param name="MatchID">The id of the match</param>
        /// <param name="who">is this for us (true), or for the opposition (false)</param>
        /// <param name="dao">The DAO to use for data access</param>
        public BowlingStats(int MatchID, ThemOrUs who, IDao dao)
        {
            this.dao = dao;
            BowlingStatsData = (from a in dao.GetBowlingStats(MatchID, who)
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
            var myDao = dao ?? new Dao();
            var data = (from a in BowlingStatsData select a._data).ToList();
            myDao.UpdateBowlingStats(data, Who);
        }
    }
}
