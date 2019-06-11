using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    public class KeeperStats
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

        public static List<KeeperStats> GetAll(DateTime fromDate, DateTime toDate, List<MatchType> matchTypes, Venue venue)
        {
            var keepers = Match.GetResults(fromDate,toDate).Where(a => a.WicketKeeper != null && a.WicketKeeper.ID>0).Select(a => a.WicketKeeper).Distinct(new PlayerComparer());
            List<KeeperStats> c = new List<KeeperStats>();
            foreach (Player p in keepers)
            {
                c.Add(new KeeperStats(p, fromDate, toDate, matchTypes, venue));
            }
            return c;

        }

        public KeeperStats(Player player, DateTime fromDate, DateTime toDate, List<MatchType> matchTypes, Venue venue)
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
                if (cache.Get("KeepersMatchData_" + Player.ID) == null)
                {
                    List<Match> allMatches;
                    allMatches = Match.GetResults().Where(a => a.WicketKeeper.ID == Player.ID).ToList();
                    cache.Insert("KeepersMatchData_" + Player.ID, allMatches, new TimeSpan(365, 0, 0, 0));
                    return allMatches;
                }
                else
                {
                    return (List<Match>)cache.Get("KeepersMatchData_" + Player.ID);
                }
            }
        }

        public int GetGames()
        {
            return FilteredMatchData.Count;
        }

        

        public decimal GetCatchesPerMatch()
        {
            List<BattingCardLine> WicketsData = new List<BattingCardLine>();
            foreach (Match m in FilteredMatchData)
            {
                WicketsData.AddRange(m.GetOurBattingScoreCard().ScorecardData);
                WicketsData.AddRange(m.GetTheirBattingScoreCard().ScorecardData);
            }
            return Math.Round(WicketsData.Where(a => a.Dismissal == ModesOfDismissal.Caught && a.Fielder.ID == this.ID).Count() / (decimal)GetGames(),2);
        }

        public decimal GetStumpingsPerMatch()
        {
            List<BattingCardLine> WicketsData = new List<BattingCardLine>();
            foreach (Match m in FilteredMatchData)
            {
                WicketsData.AddRange(m.GetOurBattingScoreCard().ScorecardData);
                WicketsData.AddRange(m.GetTheirBattingScoreCard().ScorecardData);
            }
            return Math.Round(WicketsData.Where(a => a.Dismissal == ModesOfDismissal.Stumped && a.Fielder.ID == this.ID).Count() / (decimal)GetGames(),2);
        }

        public decimal GetAverageByesPerMatch()
        {
            return Math.Round((decimal)FilteredMatchData.Select(a => new Extras(a.ID, ThemOrUs.Us).Byes).Sum() / GetGames(), 2);
    
        }

        public decimal GetBattingAverageAsKeeper()
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

        public decimal GetBattingAverageNotAsKeeper()
        {
            try
            {
                decimal totalruns = _player.GetRunsScored(_fromDate, _toDate, _matchTypes, _venue);
                decimal totalInning = Player.GetInnings(_fromDate, _toDate, _matchTypes, _venue) - Player.GetNotOuts(_fromDate, _toDate, _matchTypes, _venue);

                decimal runs = FilteredMatchData.Where(a => a.GetOurBattingScoreCard().ScorecardData.Count > 0).Select(a => a.GetOurBattingScoreCard().ScorecardData.Where(b => b.Batsman.ID == this.ID).Select(s=>s.Score).DefaultIfEmpty(0).FirstOrDefault()).Sum();
                decimal innings = FilteredMatchData.Where(a => a.GetOurBattingScoreCard().ScorecardData.Count > 0).Select(a => a.GetOurBattingScoreCard().ScorecardData.Where(b => b.Batsman.ID == this.ID).Where(b => b.Dismissal != ModesOfDismissal.NotOut && b.Dismissal != ModesOfDismissal.RetiredHurt).FirstOrDefault()).Count();

                return Math.Round((totalruns - runs) / (totalInning - innings), 2);
            }
            catch (DivideByZeroException)
            {
                return 0;
            }
        }
    }
}
