using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    public class TeamStats
    {

        private Team _team;
        private List<Match> FilteredMatchData;

        public Team Team
        {
            get
            {
                return _team;
            }
        }

        private int ID
        {
            get;
            set;
        }

        public TeamStats(Team team, DateTime fromDate, DateTime toDate, List<MatchType> matchTypes, Venue venue)
        {
            _team = team;
            ID = team.ID;
            FilteredMatchData = MatchData.Where(a => a.MatchDate > fromDate).Where(a => a.MatchDate < toDate).Where(a => matchTypes.Contains(a.Type)).Where(a => venue==null || a.VenueID == venue.ID).ToList();
        }

        private List<Match> MatchData
        {
            get
            {
                InternalCache cache = InternalCache.GetInstance();
                if (cache.Get("TeamMatchData_" + _team.ID) == null)
                {
                    List<Match> allMatches;
                    if (_team.ID != 0)
                    {
                        allMatches = Match.GetResults().Where(a => a.OppositionID == _team.ID).ToList();
                    }
                    else
                    {
                        allMatches = Match.GetResults().ToList();
                    }
                    cache.Insert("TeamMatchData_" + _team.ID, allMatches, new TimeSpan(365, 0, 0, 0));
                    return allMatches;
                }
                else
                {
                    return (List<Match>)cache.Get("TeamMatchData_" + _team.ID);
                }
            }
        }


        public int GetMatchesPlayed()
        {
            return FilteredMatchData.Count;
        }

        public int GetVictories()
        {
            return FilteredMatchData.Where(a => a.Winner != null).Where(a => a.Winner.ID == this.ID).Count();
        }

        public int GetDefeats()
        {
            return FilteredMatchData.Where(a => a.Loser != null).Where(a => a.Loser.ID == this.ID).Count();
        }

        public int GetDraws()
        {
            return FilteredMatchData.Where(a => a.ResultTied).Count();
        }

        /// <summary>
        /// The average score made by this team when batting
        /// </summary>
        /// <returns></returns>
        public decimal GetAverageBattingScore()
        {
            if (GetMatchesPlayed() == 0) return 0;
            int runsScored = 0;
            foreach (Match m in FilteredMatchData)
            {
                runsScored += m.GetTeamScore(this._team);
            }
            return runsScored / GetMatchesPlayed();
        }

        /// <summary>
        /// The average score conceeded while bowling
        /// </summary>
        /// <returns></returns>
        public decimal GetAverageBowlingScore()
        {
            if (GetMatchesPlayed() == 0) return 0;
            int runsScored = 0;
            foreach (Match m in FilteredMatchData)
            {
                if (this.ID == 0)
                {
                    runsScored += m.GetTheirBattingScoreCard().Total;
                }
                else
                {
                    runsScored += m.GetOurBattingScoreCard().Total;
                }
            }
            return runsScored / GetMatchesPlayed();
        }

        /// <summary>
        /// The total number of wickets lost by this team while batting
        /// </summary>
        /// <returns></returns>
        public int GetWicketsLost()
        {
            int totalWicketsLost = 0;
            foreach (Match m in FilteredMatchData)
            {
                totalWicketsLost += m.GetTeamWicketsDown(this._team);
            }
            return totalWicketsLost;
        }

        /// <summary>
        /// The total number of wickets taken by this team
        /// </summary>
        /// <returns></returns>
        public int GetWicketsTaken()
        {
            int totalWicketsLost = 0;
            foreach (Match m in FilteredMatchData)
            {
                if (this.ID == 0)
                {
                    totalWicketsLost += m.GetTeamWicketsDown(this._team);
                }
                else
                {
                    totalWicketsLost += m.GetTeamWicketsDown(new Team(0));
                }
            }
            return totalWicketsLost;
        }

        /// <summary>
        /// The number of wickets taken by this team's bowlers of a certain type
        /// </summary>
        /// <param name="HowOut"></param>
        /// <returns></returns>
        public int GetNumberOfWickets(ModesOfDismissal HowOut)
        {
            List<BattingCardLine> WicketsData = new List<BattingCardLine>();
            foreach (Match m in FilteredMatchData)
            {
                if (this.ID == 0)
                {
                    WicketsData.AddRange(m.GetTheirBattingScoreCard().ScorecardData);
                }
                else
                {
                    WicketsData.AddRange(m.GetOurBattingScoreCard().ScorecardData);
                }
            }
            return WicketsData.Where(a => a.Dismissal == HowOut).Count();
        }

        /// <summary>
        /// The number of dismissals of this team's batamen of a certain type.
        /// </summary>
        /// <param name="HowOut"></param>
        /// <returns></returns>
        public int GetNumberOfDismissals(ModesOfDismissal HowOut)
        {
            List<BattingCardLine> WicketsData = new List<BattingCardLine>();
            foreach (Match m in FilteredMatchData)
            {
                if (this.ID == 0)
                {
                    WicketsData.AddRange(m.GetOurBattingScoreCard().ScorecardData);
                }
                else
                {
                    WicketsData.AddRange(m.GetTheirBattingScoreCard().ScorecardData);
                }
            }
            return WicketsData.Where(a => a.Dismissal == HowOut).Count();
        }


    }
}
