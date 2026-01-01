using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDomain;
using CricketClubDAL;

namespace CricketClubMiddle.Stats
{
    public class FoWStats
    {
        
        public FoWStats(int MatchID, ThemOrUs who)
        {
            Who = who;
            var myDAO = new Dao();
            Data = (from a in myDAO.GetFoWData(MatchID, who) 
                   select new FoWStatsLine(a)).ToList();
        }


        public ThemOrUs Who
        {
            get;
            set;
        }
        public List<FoWStatsLine> Data
        {
            get;
            set;
        }

        public void Save()
        {
            var myDAO = new Dao();

            var _data = (from a in Data select a._data).ToList();
            myDAO.UpdateFoWData(_data, Who);
        }


    }
}
