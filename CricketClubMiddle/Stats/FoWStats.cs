using System;
using System.Collections.Generic;
using System.Linq;
using CricketClubDomain;
using CricketClubDAL;

namespace CricketClubMiddle.Stats
{
    public class FoWStats
    {
        private readonly IDao dao;

        public FoWStats(int MatchID, ThemOrUs who) : this(MatchID, who, new Dao())
        {
        }

        public FoWStats(int MatchID, ThemOrUs who, IDao dao)
        {
            this.dao = dao;
            Who = who;
            Data = dao.GetFoWData(MatchID, who).Select(a => new FoWStatsLine(a)).ToList();
        }

        public ThemOrUs Who { get; set; }

        public List<FoWStatsLine> Data { get; set; }

        public void Save()
        {
            var myDao = dao ?? new Dao();
            var _data = Data.Select(a => a._data).ToList();
            myDao.UpdateFoWData(_data, Who);
        }
    }
}
