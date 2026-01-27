using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle.Stats;

namespace CricketClubMiddle
{
    public class Team
    {
        private readonly IDao dao;
        private InternalCache teamCache = InternalCache.GetInstance();
        private TeamData _teamData;

        public Team(int TeamID) : this(TeamID, new Dao())
        {
        }

        public Team(int TeamID, IDao dao)
        {
            this.dao = dao;
            if (teamCache.Get("team" + TeamID) == null)
            {
                _teamData = dao.GetTeamData(TeamID);
                teamCache.Insert("team" + TeamID, _teamData, new TimeSpan(24, 0, 0));
            }
            else
            {
                _teamData = (TeamData)teamCache.Get("team" + TeamID);
            }
        }

        public TeamStats GetStats(DateTime fromDate, DateTime toDate, List<MatchType> matchTypes, Venue venue)
        {
            return new TeamStats(this, fromDate, toDate, matchTypes, venue);
        }

        public static Team CreateNewTeam(string TeamName)
        {
            return CreateNewTeam(TeamName, new Dao());
        }

        public static Team CreateNewTeam(string TeamName, IDao dao)
        {
            var newTeamid = dao.CreateNewTeam(TeamName);
            return new Team(newTeamid, dao);
        }

        public string Name
        {
            get
            {
                return _teamData.Name;
            }
            set
            {
                _teamData.Name = value;
            }
        }

        public int ID
        {
            get
            {
                return _teamData.ID;
            }
        }

        public bool IsUs => ID == 0;

        public void Save()
        {
            var myDao = dao ?? new Dao();
            myDao.UpdateTeam(_teamData);
        }


        public static List<Team> GetAll()
        {
            return GetAll(new Dao());
        }

        public static List<Team> GetAll(IDao dao)
        {
            var data = dao.GetAllTeamData();
            var teams = new List<Team>();
            foreach (var item in data)
            {
                teams.Add(new Team(item, dao));

            }
            return teams;
        }

        public static Team GetByName(string Name)
        {
            var team = (from a in Team.GetAll() where a.Name == Name select a).FirstOrDefault();
            return team;
        }

        private Team(TeamData data, IDao dao)
        {
            _teamData = data;
            this.dao = dao;
        }


        public override string ToString()
        {
            return Name;
        }

        public static Team OurTeam
        {
            get
            {
                return GetOurTeam(new Dao());
            }
        }

        public static Team GetOurTeam(IDao dao)
        {
            return new Team(0, dao);
        }

        protected bool Equals(Team other)
        {
            return this.ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Team)obj);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }
    }
}
