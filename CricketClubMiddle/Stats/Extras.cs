using System;
using CricketClubDomain;
using CricketClubDAL;

namespace CricketClubMiddle.Stats
{
    public class Extras
    {
        private readonly IDao dao;
        private ExtrasData _data;
        private ThemOrUs _who;

        /// <summary>
        /// Get an existing set of extras if it exists or an empty one if not yet created
        /// </summary>
        /// <param name="MatchID">The Match ID</param>
        /// <param name="Us">For them or us?</param>
        public Extras(int MatchID, ThemOrUs Who) : this(MatchID, Who, new Dao())
        {
        }

        public Extras(int MatchID, ThemOrUs Who, IDao dao)
        {
            this.dao = dao;
            _who = Who;
            _data = dao.GetExtras(MatchID, Who);
        }

        public void Save()
        {
            var myDao = dao ?? new Dao();
            myDao.UpdateExtras(_data, _who);
        }

        #region Properties

        public int Byes
        {
            get => _data.Byes;
            set => _data.Byes = value;
        }

        public int LegByes
        {
            get => _data.LegByes;
            set => _data.LegByes = value;
        }

        public int Wides
        {
            get => _data.Wides;
            set => _data.Wides = value;
        }

        public int Penalty
        {
            get => _data.Penalty;
            set => _data.Penalty = value;
        }

        public int NoBalls
        {
            get => _data.NoBalls;
            set => _data.NoBalls = value;
        }

        #endregion
    }
}
