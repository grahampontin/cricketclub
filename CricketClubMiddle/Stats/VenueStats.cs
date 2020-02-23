using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    public class VenueStats
    {
        private Venue _venue;
        private List<Match> FilteredMatchData;

        public Venue Venue
        {
            get
            {
                return _venue;
            }
        }

        private int ID
        {
            get;
            set;
        }

        public VenueStats(Venue venue, DateTime fromDate, DateTime toDate, List<MatchType> matchTypes)
        {
            _venue = venue;
            ID = venue.ID;
            FilteredMatchData = MatchData.Where(a => a.MatchDate > fromDate).Where(a => a.MatchDate < toDate).Where(a => matchTypes.Contains(a.Type)).ToList();
        }

        private List<Match> MatchData
        {
            get
            {
                InternalCache cache = InternalCache.GetInstance();
                if (cache.Get("VenueMatchData_" + _venue.ID) == null)
                {
                    List<Match> allMatches;
                    allMatches = Match.GetResults().Where(a => a.VenueID == _venue.ID).Where(a=>a.GetOurBattingScoreCard().Total>0).ToList();
                    cache.Insert("VenueMatchData_" + _venue.ID, allMatches, new TimeSpan(365, 0, 0, 0));
                    return allMatches;
                }
                else
                {
                    return (List<Match>)cache.Get("VenueMatchData_" + _venue.ID);
                }
            }
        }

        public int GetAverageVillageScore()
        {
            return FilteredMatchData.Sum(a => a.GetOurBattingScoreCard().Total) / GetMatchesPlayed();
        }

        public int GetAverageOpponentScore()
        {
            return FilteredMatchData.Sum(a => a.GetTheirBattingScoreCard().Total) / GetMatchesPlayed();
        }

        public int GetAverageWicketsTakenByVillage()
        {
            return FilteredMatchData.Sum(a => a.GetTeamWicketsDown(a.Opposition)) / GetMatchesPlayed();
        }

        public int GetAverageWicketsTakenByOpposition()
        {
            return FilteredMatchData.Sum(a => a.GetTeamWicketsDown(a.Us)) / GetMatchesPlayed();
        }

        public decimal GetPercentageTossWinnerBats()
        {
            return Math.Round((decimal)FilteredMatchData.Where(a => a.TossWinnerElectedTo == "bat").Count() / (decimal)GetMatchesPlayed() * 100, 2);
        }

        public decimal GetPercentageTeamBattingFirstWins()
        {
            return Math.Round((decimal)FilteredMatchData.Where(a => (a.TossWinnerBatted && a.TossWinner != null && a.Winner != null && a.TossWinner.ID == a.Winner.ID) || !a.TossWinnerBatted && (a.Loser != null && a.TossWinner != null && a.TossWinner.ID == a.Loser.ID)).Count() / (decimal)GetMatchesPlayed() * 100, 2);
        }

        public int GetMatchesPlayed()
        {
            return FilteredMatchData.Count;
        }

        public decimal GetNumberOfWicketsPerInnings(ModesOfDismissal HowOut)
        {
            List<BattingCardLine> WicketsData = new List<BattingCardLine>();
            foreach (Match m in FilteredMatchData)
            {
                WicketsData.AddRange(m.GetOurBattingScoreCard().ScorecardData);
                WicketsData.AddRange(m.GetTheirBattingScoreCard().ScorecardData);
            }
            return WicketsData.Where(a => a.Dismissal == HowOut).Count()/ GetMatchesPlayed() /2;
        }

        public int GetVillagWins()
        {
            return FilteredMatchData.Where(a => a.Winner != null && a.Winner.ID == a.Us.ID).Count();
        }

        public int GetVillagLosses()
        {
            return FilteredMatchData.Where(a => a.Loser != null && a.Loser.ID == a.Us.ID).Count();
        }
    }
}
