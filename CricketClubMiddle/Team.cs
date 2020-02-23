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
        private Dao myDAO = new Dao();
        private InternalCache teamCache = InternalCache.GetInstance();
        private TeamData _teamData;

        public Team(int TeamID)
        {
            if (teamCache.Get("team" + TeamID) == null)
            {
                _teamData = myDAO.GetTeamData(TeamID);
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
            Dao myDAO = new Dao();
            int newTeamid = myDAO.CreateNewTeam(TeamName);
            return new Team(newTeamid);
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
            myDAO.UpdateTeam(_teamData);
        }


        public static List<Team> GetAll()
        {
            IEnumerable<TeamData> data = new Dao().GetAllTeamData();
            List<Team> teams = new List<Team>();
            foreach (TeamData item in data)
            {
                teams.Add(new Team(item));

            }
            return teams;
        }

        public static Team GetByName(string Name)
        {
            Team team = (from a in Team.GetAll() where a.Name == Name select a).FirstOrDefault();
            return team;
        }

        private Team(TeamData data)
        {
            _teamData = data;
        }


        public override string ToString()
        {
            return Name;
        }

        public static Team OurTeam
        {
            get
            {
                return new Team(0);
            }
        }
    }
}
