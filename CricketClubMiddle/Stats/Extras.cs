using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDomain;
using CricketClubDAL;

namespace CricketClubMiddle.Stats
{
    public class Extras
    {

        private ExtrasData _data;
        private ThemOrUs _who;

        /// <summary>
        /// Get an existing set of extras if it exists or an empty one if not yet created
        /// </summary>
        /// <param name="MatchID">The Match ID</param>
        /// <param name="Us">For them or us?</param>
        public Extras(int MatchID, ThemOrUs Who) 
        {
            _who = Who;
            Dao myDao = new Dao();
            _data = myDao.GetExtras(MatchID, Who);
        }

        public void Save()
        {
            Dao myDAO = new Dao();
            myDAO.UpdateExtras(_data, _who);
        }

        #region Properties

        public int Byes
        {
            get
            {
                return _data.Byes;
            }
            set
            {
                _data.Byes = value;
            }
        }

        public int LegByes
        {
            get
            {
                return _data.LegByes;
            }
            set
            {
                _data.LegByes = value;
            }
        }

        public int Wides
        {
            get
            {
                return _data.Wides;
            }
            set
            {
                _data.Wides = value;
            }
        }

        public int Penalty
        {
            get
            {
                return _data.Penalty;
            }
            set
            {
                _data.Penalty = value;
            }
        }

        public int NoBalls
        {
            get
            {
                return _data.NoBalls;
            }
            set
            {
                _data.NoBalls = value;
            }
        }

        #endregion
    }
}
