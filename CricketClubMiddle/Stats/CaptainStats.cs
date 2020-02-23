using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    public class CaptainStats
    {
        private Player _player;
        private List<Match> FilteredMatchData;
        private DateTime _fromDate;
        private DateTime _toDate;
        private List<MatchType> _matchTypes;
        private Venue _venue;

        public Player Player
        {
            get
            {
                return _player;
            }
        }

        private int ID
        {
            get;
            set;
        }

        public static List<CaptainStats> GetAll(DateTime fromDate, DateTime toDate, List<MatchType> matchTypes, Venue venue)
        {
            var captains = Match.GetResults(fromDate,toDate).Where(a => a.Captain != null && a.Captain.ID>0).Select(a => a.Captain).Distinct(new PlayerComparer());
            List<CaptainStats> c = new List<CaptainStats>();
            foreach (Player p in captains)
            {
                c.Add(new CaptainStats(p, fromDate, toDate, matchTypes, venue));
            }
            return c;

        }

        public CaptainStats(Player player, DateTime fromDate, DateTime toDate, List<MatchType> matchTypes, Venue venue)
        {
            _player = player;
            _fromDate = fromDate;
            _toDate = toDate;
            _matchTypes = matchTypes;
            _venue = venue;
            ID = player.ID;
            FilteredMatchData = MatchData.Where(a => a.MatchDate > fromDate).Where(a => a.MatchDate < toDate).Where(a => matchTypes.Contains(a.Type)).Where(a => venue==null || a.VenueID == venue.ID).ToList();
        }

        private List<Match> MatchData
        {
            get
            {
                InternalCache cache = InternalCache.GetInstance();
                if (cache.Get("CaptainsMatchData_" + Player.ID) == null)
                {
                    List<Match> allMatches;
                    allMatches = Match.GetResults().Where(a => a.Captain.ID == Player.ID).ToList();
                    cache.Insert("CaptainsMatchData_" + Player.ID, allMatches, new TimeSpan(365, 0, 0, 0));
                    return allMatches;
                }
                else
                {
                    return (List<Match>)cache.Get("CaptainsMatchData_" + Player.ID);
                }
            }
        }

        public int GetGamesInCharge()
        {
            return FilteredMatchData.Count;
        }

        public int GetWins()
        {
            return FilteredMatchData.Where(a => a.Winner!=null && a.Winner.ID == a.Us.ID).Count();
        }

        public int GetLosses()
        {
            return FilteredMatchData.Where(a => a.Loser != null && a.Loser.ID == a.Us.ID).Count();
        }

        public decimal GetPercentageGamesWon()
        {
            try
            {
                return Math.Round((decimal)GetWins() / (decimal)GetGamesInCharge() * 100, 2);
            }
            catch (DivideByZeroException)
            {
                return 0;
            }
        }

        public decimal GetPercentageTossWon()
        {
            try
            {
                return Math.Round((decimal)FilteredMatchData.Where(a => a.TossWinner != null && a.TossWinner.ID == a.Us.ID).Count() / (decimal)GetGamesInCharge() * 100, 2);
            }
            catch (DivideByZeroException)
            {
                return 0;
            }
        }

        public decimal GetPercentageChooseToBat()
        {
            try
            {
                return Math.Round((decimal)FilteredMatchData.Where(a => a.TossWinner != null && a.TossWinner.ID == a.Us.ID).Where(a => a.TossWinnerBatted).Count() / (decimal)FilteredMatchData.Where(a => a.TossWinner != null && a.TossWinner.ID == a.Us.ID).Count() * 100, 2);
            }
            catch (DivideByZeroException)
            {
                return 0;
            }
        }

        public decimal GetBattingAverageAsCaptain()
        {
            try
            {
                decimal runs = FilteredMatchData.Where(a => a.GetOurBattingScoreCard().ScorecardData.Count > 0).Select(a => a.GetOurBattingScoreCard().ScorecardData.Where(b => b.Batsman.ID == this.ID).FirstOrDefault().Score).Sum();
                decimal innings = FilteredMatchData.Where(a => a.GetOurBattingScoreCard().ScorecardData.Count > 0).Select(a => a.GetOurBattingScoreCard().ScorecardData.Where(b => b.Batsman.ID == this.ID).FirstOrDefault()).Where(a => a.Dismissal != ModesOfDismissal.NotOut && a.Dismissal != ModesOfDismissal.RetiredHurt).Count();

                return Math.Round(runs / innings, 2);
            }
            catch (DivideByZeroException)
            {
                return 0;
            }
        }

        public decimal GetBattingAverageNotAsCaptain()
        {
            try
            {
                decimal totalruns = _player.GetRunsScored(_fromDate, _toDate, _matchTypes, _venue);
                decimal totalInning = Player.GetInnings(_fromDate, _toDate, _matchTypes, _venue) - Player.GetNotOuts(_fromDate, _toDate, _matchTypes, _venue);

                decimal runs = FilteredMatchData.Select(a => a.GetOurBattingScoreCard().ScorecardData.Where(b => b.Batsman.ID == this.ID).FirstOrDefault().Score).Sum();
                decimal innings = FilteredMatchData.Select(a => a.GetOurBattingScoreCard().ScorecardData.Where(b => b.Batsman.ID == this.ID).FirstOrDefault()).Where(a => a.Dismissal != ModesOfDismissal.NotOut && a.Dismissal != ModesOfDismissal.RetiredHurt).Count();

                return Math.Round((totalruns - runs) / (totalInning - innings), 2);
            }
            catch (DivideByZeroException)
            {
                return 0;
            }
        }
    }
}
