using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle.Interactive;
using CricketClubMiddle;
using CricketClubMiddle.Utility;
using System.IO;
using System.Web;

namespace CricketClubMiddle
{
    public class Player
    {

        public static InternalCache cache = InternalCache.GetInstance();

        #region Members

        private PlayerData PlayerData;
        

        

        #endregion

        #region Constructors

        public Player(int PlayerID)
        {
            //Get the player specified by this ID
            Dao myDao = new Dao();
            PlayerData = myDao.GetPlayerData(PlayerID);

        }
        /// <summary>
        /// Used by GetAll();
        /// </summary>
        /// <param name="_data"></param>
        private Player(PlayerData _data)
        {
            PlayerData = _data;
        }
        /// <summary>
        /// A special constructor for use for representing opposition players - returns an object with only the name set.
        /// </summary>
        /// <param name="PlayerName"></param>
        public Player(string PlayerName)
        {
            PlayerData pd = new PlayerData();
            pd.Name = PlayerName;
            PlayerData = pd;
        }

        public static Player CreateNewPlayer(string Name)
        {
            //Creates a new player.
            Dao myDAO = new Dao();
            int newPlayerID = myDAO.CreateNewPlayer(Name);
            return new Player(newPlayerID);
        }

        public static List<Player> GetAll()
        {
            Dao myDAO = new Dao();
            List<PlayerData> data = myDAO.GetAllPlayers();
            return (from a in data select new Player(a)).OrderBy(a=>a.FormalName).ToList();
        }

        #endregion

        #region Properties

        public int ID
        {
            get
            {
                return PlayerData.ID;
            }

        }

        public string EmailAddress
        {
            get
            {
                return PlayerData.EmailAddress;
            }
            set
            {
                PlayerData.EmailAddress = value;
            }
        }

        /// <summary>
        /// Setter is obsolete - use First Name and Surname
        /// </summary>
        public string Name
        {
            get {
                if (this.FirstName != "" && this.Surname != "" && this.FirstName != null && this.Surname != null)
                {
                    return FirstName.Substring(0, 1).ToUpper() + MiddleInitials + " " + Surname;
                }
                else
                {
                    return PlayerData.Name;
                }
                 
            }
            set { PlayerData.Name = value; }
        }

        /// <summary>
        /// Surname, Firstname
        /// </summary>
        public string FormalName
        {
            get
            {
                if (this.FirstName != "" && this.Surname != "")
                {
                    return Surname + ", " + FirstName;
                }
                else
                {
                    string name = PlayerData.Name;
                    int firstSpace = name.IndexOf(' ');
                    string surname = "";
                    string initials = "";
                    if (firstSpace > 0)
                    {
                        surname = name.Substring(firstSpace);
                        initials = name.Substring(0, firstSpace);
                        return surname.Trim() + ", " + initials.Trim();
                    }
                    else
                    {
                        return PlayerData.Name;
                    }
                    
                }
            }
        }

        public DateTime DOB
        {
            get { return PlayerData.DateOfBirth; }
            set { PlayerData.DateOfBirth = value; }
        }

        public string FullName
        {
            get { return PlayerData.FullName; }
            set { PlayerData.FullName = value; }
        }

        public string FirstName 
        {
            get { return PlayerData.FirstName; }
            set { PlayerData.FirstName = value; }
        }

        public string Surname
        {
            get { return PlayerData.Surname; }
            set { PlayerData.Surname = value; }
        
        }

        public string MiddleInitials
        {
            get { return PlayerData.MiddleInitials; }
            set { PlayerData.MiddleInitials = value; }
        
        }

        public Player RingerOf
        {
            get
            {
                if (PlayerData.RingerOf > 0)
                {
                    return new Player(PlayerData.RingerOf);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                PlayerData.RingerOf = value.ID;
            }
        }

        public string Nickname
        {
            get { return PlayerData.NickName; }
            set { PlayerData.NickName = value; }
        }

        public string Location
        {
            get { return PlayerData.Location; }
            set { PlayerData.Location = value; }

        }

        public string BowlingStyle
        {
            get { return PlayerData.BowlingStyle; }
            set { PlayerData.BowlingStyle = value; }
        }

        public string BattingStyle
        {
            get { return PlayerData.BattingStyle; }
            set { PlayerData.BattingStyle = value; }
        }

        public PlayingRole PlayingRole
        {
            get;set;
        }

        public int Caps
        {
            get
            {
                return GetMatchesPlayed();
            }
        }

        public DateTime Debut
        {
            get
            {
                return _battingStatsData.Select(a => a.MatchDate).OrderBy(a => a).FirstOrDefault();
            }
        }

        public string Education
        {
            get { return PlayerData.Education; }
            set { PlayerData.Education = value; }
        }

        public string Height
        {
            get { return PlayerData.Height; }
            set { PlayerData.Height = value; }
        }

        public bool IsActive
        {
            get
            {
                return PlayerData.IsActive;
            }
            set
            {
                PlayerData.IsActive = value;
            }
        }

        public string Bio
        {
            get
            {
                string filename = this.FirstName + "_" + this.Surname + "_BIO.html";
                string path = SettingsWrapper.GetSettingString("BioFolder", "/Players/bios/") + filename;
                path = HttpContext.Current.Server.MapPath(path);
                if (File.Exists(path))
                {
                    StreamReader stream = new StreamReader(path);
                    string temp = stream.ReadToEnd();
                    stream.Close();
                    return temp;
                }
                else
                {
                    return "No player bio has been created.";
                }
            }
            set
            {
                string filename = this.FirstName+"_"+this.Surname+"_BIO.html";
                string path = SettingsWrapper.GetSettingString("BioFolder", "/Players/bios/") + filename;
                path = HttpContext.Current.Server.MapPath(path);
                if (File.Exists(path))
                {
                    File.Move(path, path + File.GetLastWriteTime(path).ToString("ddMMyyyyHHmmss"));
                    File.Delete(path);
                }
                File.WriteAllText(path, value);
            }
        }



        #endregion

        #region Methods

        public void Save()
        {
            if (PlayerData.ID != 0)
            {
                Dao myDao = new Dao();
                myDao.UpdatePlayer(PlayerData);
            }
            else
            {
                throw new InvalidOperationException("Player has no player ID - is this an opposition player?");
            }
        }

        public int GetNumberOfMatchesPlayedIn(DateTime startDate, DateTime endDate, List<MatchType> matchType , Venue venue)
        {
            var matches = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue)
                           select a).Count();
            return matches;
        }

        public int NumberOfMatchesPlayedThisSeason
        {
            get
            {
                return _battingStatsData.Count(b => b.MatchDate.Year == DateTime.Now.Year);    
            }
            
        }

        public bool PlayedInMatch(int MatchID)
        {
            return _battingStatsData.Any(a => a.MatchID == MatchID);
            
        }


        #region Batting Stats

        private List<BattingCardLineData> _battingStatsDataCache;

        private List<BattingCardLineData> _battingStatsData
        {
            get
            {
                if (_battingStatsDataCache == null)
                {
                    Dao myDao = new Dao();
                    _battingStatsDataCache = myDao.GetPlayerBattingStatsData(this.ID);
                }
                return _battingStatsDataCache;
            }
        }

        public decimal GetBattingAverage()
        {
            try
            {
                decimal average = ((decimal)this.GetRunsScored() / (this.GetInnings() - this.GetNotOuts()));
                return Math.Round(average, 2);
            }
            catch
            {
                return 0;
            }
            
        }

        public decimal GetBattingAverage(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            try
            {
                decimal average = ((decimal)this.GetRunsScored(startDate, endDate, matchType, venue) / (this.GetInnings(startDate, endDate, matchType, venue) - this.GetNotOuts(startDate, endDate, matchType, venue)));
                return Math.Round(average, 2);
            }
            catch
            {
                return 0;
            }

        }

        public int GetRunsScored(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var runsScored = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue)
                              select a.Score).Sum();
            return runsScored;
        }

        public int GetRunsScored(int MatchID)
        {
            var runsScored = (from a in _battingStatsData
                              where a.MatchID == MatchID
                              select a.Score).Sum();
            return runsScored;
        }

        public int GetRunsScored()
        {
            var runsScored = (from a in _battingStatsData
                              select a.Score).Sum();
            return runsScored;
        }

        public int GetDucks(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var ducks = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue)
                        where a.Score == 0
                        where   (
                                    (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.DidNotBat &&
                                    (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.NotOut &&
                                    (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.Retired
                                )
                        select a).Count();
            return ducks;
        }

        public int GetDucks(int MatchID)
        {
            var tons = (from a in _battingStatsData
                        where a.MatchID == MatchID
                        where a.Score == 0
                        where (
                                    (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.DidNotBat &&
                                    (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.NotOut &&
                                    (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.Retired
                                )
                        select a).Count();
            return tons;
        }

        public int GetDucks()
        {
            var tons = (from a in _battingStatsData
                        where a.Score == 0
                        where (
                                    (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.DidNotBat &&
                                    (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.NotOut &&
                                    (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.Retired
                                )
                        select a).Count();
            return tons;
        }


        public int Get100sScored(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var tons = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue)
                        where a.Score >= 100
                        select a).Count();
            return tons;
        }

        public int Get100sScored(int MatchID)
        {
            var tons = (from a in _battingStatsData
                        where a.MatchID == MatchID
                        where a.Score >= 100
                        select a).Count();
            return tons;
        }

        public int Get100sScored()
        {
            var tons = (from a in _battingStatsData
                        where a.Score >= 100
                        select a).Count();
            return tons;
        }

        public int Get50sScored(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var fifties = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue)
                        where a.Score >= 50
                        where a.Score < 100
                        select a).Count();
            return fifties;
        }

        public int Get50sScored(int MatchID)
        {
            var fifties = (from a in _battingStatsData
                           where a.MatchID == MatchID
                           where a.Score >= 50
                           where a.Score < 100
                           select a).Count();
            return fifties;
        }

        public int Get50sScored()
        {
            var fifties = (from a in _battingStatsData
                           where a.Score >= 50
                           where a.Score < 100
                           select a).Count();
            return fifties;
        }

        public int GetNotOuts(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var notouts = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue)
                           where (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.NotOut ||
                           (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.RetiredHurt
                           select a).Count();
            return notouts;
        }

        public int GetNotOuts(int MatchID)
        {
            var notouts = (from a in _battingStatsData
                           where (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.NotOut ||
                           (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.RetiredHurt
                           where a.MatchID == MatchID
                           select a).Count();
            return notouts;
        }

        public int GetNotOuts()
        {
            var notouts = (from a in _battingStatsData
                           where (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.NotOut ||
                           (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.RetiredHurt
                           select a).Count();
            return notouts;
        }

        public int GetInnings(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var knocks = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue)
                         where (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.DidNotBat
                         select a).Count();
            return knocks;


        }

        public int GetBattingPosition()
        {
            if (_battingStatsData.Count > 0)
            {
                var x = (from a in _battingStatsData select a.BattingAt).Average();
                return (int)Math.Round(x, 0);
            }
            else
            {
                return 11;
            }
        }

        public int GetBattingPosition(int MatchID)
        {
            try
            {
                var x = (from a in _battingStatsData.Where(a => a.MatchID == MatchID) select a.BattingAt).Average();
                return (int)Math.Round(x, 0)+1;
            }
            catch
            {
                throw new ApplicationException("Player Not Batting in this Match");
            }
        }
        public int GetBattingPosition(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            try
            {
                var x = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue) select a.BattingAt).Average();
                return (int)Math.Round(x, 0)+1;
            }
            catch
            {
                return 11;
            }
        }

        public int GetMatchesPlayed()
        {
            return TheGreaterOf(_battingStatsData.Count, _bowlingStatsData.Count);
        }

        private int TheGreaterOf(int valueOne, int valueTwo)
        {
            return valueOne >= valueTwo ? valueOne : valueTwo;
        }

        public int GetMatchesPlayed(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            int battingCount = FilterData(_battingStatsData, startDate, endDate, matchType, venue).Count();
            int bowlingCount = FilterBowlingData(_bowlingStatsData, startDate, endDate, matchType, venue).Count();
            return TheGreaterOf(battingCount,bowlingCount);
        }


        public int GetInnings(int MatchID)
        {
            var knocks = (from a in _battingStatsData
                          where (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.DidNotBat
                          where a.MatchID == MatchID
                          select a).Count();
            return knocks;
        }

        public int GetInnings()
        {
            var knocks = (from a in _battingStatsData
                          where (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.DidNotBat
                          select a).Count();
            return knocks;
        }

        public int GetHighScore(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            try
            {
                var highScore = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue)
                                 select a.Score).Max();
                return highScore;
            }
            catch
            {
                return 0;
            }
        }

        public int GetHighScore()
        {
            try
            {
                var highScore = (from a in _battingStatsData
                                 select a.Score).Max();
                return highScore;
            }
            catch
            {
                return 0;
            }
        }

        public bool GetHighScoreWasNotOut()
        {
            var dismissalIDs = from a in _battingStatsData
                              where a.Score == this.GetHighScore()
                              select a.ModeOfDismissal;
            if (dismissalIDs.Contains((int)ModesOfDismissal.NotOut) || dismissalIDs.Contains((int)ModesOfDismissal.RetiredHurt))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int Get4s(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var fours = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue)
                         select a.Fours).Sum();
            return fours;
        }

        public int Get4s(int MatchID)
        {
            var fours = (from a in _battingStatsData
                         where a.MatchID == MatchID
                         select a.Fours).Sum();
            return fours;
        }

        public int Get4s()
        {
            var fours = (from a in _battingStatsData
                         select a.Fours).Sum();
            return fours;
        }
        public int Get6s(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var sixes = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue)
                         select a.Sixes).Sum();
            return sixes;
        }

        public int Get6s(int MatchID)
        {
            var sixes = (from a in _battingStatsData
                         where a.MatchID == MatchID
                         select a.Sixes).Sum();
            return sixes;
        }

        public int Get6s()
        {
            var sixes = (from a in _battingStatsData
                         select a.Sixes).Sum();
            return sixes;
        }

        #endregion

        #region Bowling Stats

        private List<BowlingStatsEntryData> _bowlingStatsDataCache;

        private List<BowlingStatsEntryData> _bowlingStatsData
        {
            get
            {
                if (_bowlingStatsDataCache == null)
                {
                    Dao myDao = new Dao();
                    _bowlingStatsDataCache = myDao.GetPlayerBowlingStatsData(this.ID);
                }
                return _bowlingStatsDataCache;
            }
        }

        public int GetWicketsTaken()
        {
            var wt = (from a in _bowlingStatsData
                      select a.Wickets).Sum();
            return wt;
        }

        public int GetWicketsTaken(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var wt = (from a in FilterBowlingData(_bowlingStatsData, startDate, endDate, matchType, venue)
                      select a.Wickets).Sum();
            return wt;
        }

        public int GetWicketsTaken(int MatchID)
        {
            var wt = (from a in _bowlingStatsData
                      where a.MatchID == MatchID
                      select a.Wickets).Sum();
            return wt;
        
        }

        public decimal GetBowlingAverage()
        {
            try
            {
                decimal fraction = (GetRunsConceeded() / GetWicketsTaken());
                return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetBowlingAverage(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            try
            {
                decimal fraction = (GetRunsConceeded(startDate, endDate, matchType, venue) / GetWicketsTaken(startDate, endDate, matchType, venue));
                return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetBowlingAverage(int MatchID)
        {
            try
            {
                decimal fraction = (GetRunsConceeded(MatchID) / GetWicketsTaken(MatchID));
                return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetEconomy()
        {
            try
            {
                decimal fraction = (GetRunsConceeded() / GetOversBowled());
                return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        
        }

        public decimal GetEconomy(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            try
            {
                decimal fraction = (GetRunsConceeded(startDate, endDate, matchType, venue) / GetOversBowled(startDate, endDate, matchType, venue));
                return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetEconomy(int MatchID)
        {
            try
            {
                decimal fraction = (GetRunsConceeded(MatchID) / GetOversBowled(MatchID));
                return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        

        public int GetFiveFers()
        {
            var fivefers = (from a in _bowlingStatsData
                            where a.Wickets >= 5
                            select a).Count();
            return fivefers;
        }

        public int GetFiveFers(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var fivefers = (from a in FilterBowlingData(_bowlingStatsData, startDate, endDate, matchType, venue)
                            where a.Wickets >= 5
                            select a).Count();
            return fivefers;
        }

        public int GetFiveFers(int MatchID)
        {
            var fivefers = (from a in _bowlingStatsData
                            where a.Wickets >= 5
                            where a.MatchID == MatchID
                            select a).Count();
            return fivefers;
        }

        public int GetThreeFers()
        {
            var threefers = (from a in _bowlingStatsData
                            where a.Wickets >= 3
                            select a).Count();
            return threefers;
        }

        public int GetThreeFers(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var threefers = (from a in FilterBowlingData(_bowlingStatsData, startDate, endDate, matchType, venue)
                            where a.Wickets >= 3
                            select a).Count();
            return threefers;
        }

        public int GetThreeFers(int MatchID)
        {
            var threefers = (from a in _bowlingStatsData
                            where a.Wickets >= 3
                            where a.MatchID == MatchID
                            select a).Count();
            return threefers;
        }

        public int GetTenFers()
        {
            var tenfers = (from a in _bowlingStatsData
                             where a.Wickets >= 10
                             select a).Count();
            return tenfers;
        }

        public int GetTenFers(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var tenfers = (from a in FilterBowlingData(_bowlingStatsData, startDate, endDate, matchType, venue)
                             where a.Wickets >= 10
                             select a).Count();
            return tenfers;
        }

        public int GetTenFers(int MatchID)
        {
            var tenfers = (from a in _bowlingStatsData
                             where a.Wickets >= 10
                             where a.MatchID == MatchID
                             select a).Count();
            return tenfers;
        }

        public decimal GetStrikeRate()
        {
            try {
            decimal fraction = (GetOversBowled() * 6 / GetWicketsTaken());
            return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetStrikeRate(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            try {
            decimal fraction = (GetOversBowled(startDate, endDate, matchType, venue) * 6 / GetWicketsTaken(startDate, endDate, matchType, venue));
            return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetStrikeRate(int MatchID)
        {
            try {
            decimal fraction = (GetOversBowled(MatchID)*6 / GetWicketsTaken(MatchID));
            return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetOversBowled()
        {
            var ob = (from a in _bowlingStatsData
                      select a.Overs).Sum();
            return ob;
        }

        public decimal GetOversBowled(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var ob = (from a in FilterBowlingData(_bowlingStatsData, startDate, endDate, matchType, venue)
                      select a.Overs).Sum();
            return ob;
        }

        public decimal GetOversBowled(int MatchID)
        {
            var ob = (from a in _bowlingStatsData
                      where a.MatchID == MatchID
                      select a.Overs).Sum();
            return ob;
        }


        public int GetRunsConceeded()
        {
            var rc = (from a in _bowlingStatsData
                      select a.Runs).Sum();
            return rc;
        }

        public int GetRunsConceeded(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var rc = (from a in FilterBowlingData(_bowlingStatsData, startDate, endDate,matchType, venue)
                      select a.Runs).Sum();
            return rc;
        }

        public int GetRunsConceeded(int MatchID)
        {
            var rc = (from a in _bowlingStatsData
                      where a.MatchID == MatchID
                      select a.Runs).Sum();
            return rc;
        }

        public string GetBestMatchFigures()
        {
            int mostwickets;
            try
            {
                mostwickets = (from a in _bowlingStatsData
                               select a.Wickets).Max();
            }
            catch
            {
                return "0/0";
            }
            var runs = (from a in _bowlingStatsData
                        where a.Wickets == mostwickets
                        select a.Runs).Min();
            return mostwickets.ToString() + "/" + runs.ToString();
        }

        public string GetBestMatchFigures(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            int mostwickets;
            try
            {
                mostwickets = (from a in FilterBowlingData(_bowlingStatsData, startDate, endDate, matchType, venue)
                               select a.Wickets).Max();
            }
            catch
            {
                return "0/0";
            }
            var runs = (from a in FilterBowlingData(_bowlingStatsData, startDate, endDate, matchType, venue)
                        where a.Wickets == mostwickets
                        select a.Runs).Min();
            return mostwickets.ToString() + "/" + runs.ToString();
        }

        public string GetMatchFigures(int MatchID)
        {
            //Max/Min here are a bit lazy - there will only be one entry
            if (_bowlingStatsData.Any())
            {
                var mostwickets = (from a in _bowlingStatsData
                                   where a.MatchID == MatchID
                                   select a.Wickets).Max();
                var runs = (from a in _bowlingStatsData
                            where a.Wickets == mostwickets
                            where a.MatchID == MatchID
                            select a.Runs).Min();
                return mostwickets.ToString() + "/" + runs.ToString();
            }
            else
            {
                return "0/0";
            }
        }
        



        #endregion

        #region Fielding Stats

        private List<BattingCardLineData> _fieldingStatsDataCache;

        private List<BattingCardLineData> _fieldingStatsData
        {
            get
            {
                if (_fieldingStatsDataCache == null)
                {
                    Dao myDao = new Dao();
                    _fieldingStatsDataCache = myDao.GetPlayerFieldingStatsData(this.ID);
                }
                return _fieldingStatsDataCache;
            }
        }

        public int GetCatchesTaken()
        {
            var ct = (from a in _fieldingStatsData
                      where (
                      ((ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.Caught && a.FielderID == this.ID)
                      ||
                      ((ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.CaughtAndBowled && a.BowlerID == this.ID)
                      )
                      select a).Count();
            return ct;
                     
        }

        public int GetCatchesTaken(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var ct = (from a in FilterData(_fieldingStatsData, startDate, endDate, matchType, venue)
                      where (
                      ((ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.Caught && a.FielderID == this.ID)
                      ||
                      ((ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.CaughtAndBowled && a.BowlerID == this.ID)
                      )
                      select a).Count();
            return ct;
        }

        public int GetCatchesTaken(int MatchID)
        {
            var ct = (from a in _fieldingStatsData
                      where (
                      ((ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.Caught && a.FielderID == this.ID)
                      ||
                      ((ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.CaughtAndBowled && a.BowlerID == this.ID)
                      )
                      where a.MatchID == MatchID
                      select a).Count();
            return ct;
        }

        public int GetStumpings()
        {
            var stmp = (from a in _fieldingStatsData
                        where a.FielderID == this.ID
                        where (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.Stumped
                        select a).Count();
            return stmp;
        }

        public int GetStumpings(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var stmp = (from a in FilterData(_fieldingStatsData, startDate, endDate, matchType, venue)
                        where a.FielderID == this.ID
                        where (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.Stumped
                        select a).Count();
            return stmp;
        }

        public int GetStumpings(int MatchID)
        {
            var stmp = (from a in _fieldingStatsData
                        where a.FielderID == this.ID
                        where a.MatchID == MatchID
                        where (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.Stumped
                        select a).Count();
            return stmp;
        }

        public int GetRunOuts()
        {
            var stmp = (from a in _fieldingStatsData
                        where a.FielderID == this.ID
                        where (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.RunOut
                        select a).Count();
            return stmp;
        }

        public int GetRunOuts(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var stmp = (from a in FilterData(_fieldingStatsData, startDate, endDate, matchType, venue)
                        where a.FielderID == this.ID
                        where (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.RunOut
                        select a).Count();
            return stmp;
        }

        public int GetRunOuts(int MatchID)
        {
            var stmp = (from a in _fieldingStatsData
                        where a.FielderID == this.ID
                        where a.MatchID == MatchID
                        where (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.RunOut
                        select a).Count();
            return stmp;
        }

        #endregion

        #endregion


        private static IEnumerable<BattingCardLineData> FilterData(List<BattingCardLineData> data, DateTime startDate, DateTime endDate, List<MatchType> matchTypes, Venue venue)
        {
            return from a in data
                   where (a.MatchDate >= startDate || startDate == null)
                   where (a.MatchDate <= endDate || endDate == null)
                   where (matchTypes.Any(b=>(int)b ==  a.MatchTypeID) || matchTypes.Contains(MatchType.All))
                   where (venue == null || a.VenueID == venue.ID)
                   select a;
        }

        

        private static IEnumerable<BowlingStatsEntryData> FilterBowlingData(List<BowlingStatsEntryData> data, DateTime startDate, DateTime endDate, List<MatchType> matchTypes, Venue venue)
        {
            return from a in data
                   where (a.MatchDate >= startDate || startDate == null)
                   where (a.MatchDate <= endDate || endDate == null)
                   where (matchTypes.Any(b => (int)b == a.MatchTypeID) || matchTypes.Contains(MatchType.All))
                   where (venue == null || a.VenueID == venue.ID)
                   select a;
        }

        public override string ToString()
        {
            return this.FormalName;
        }

        public bool AssociateWithUser(User user)
        {
            var alreadyAssigned = Player.GetAll().Where(a=> a.EmailAddress == user.EmailAddress).Any();
            if (alreadyAssigned)
            {
                return false;
            }
            else
            {
                this.EmailAddress = user.EmailAddress;
                this.Save();
                return true;
            }
        }

        public IEnumerable<KeyValuePair<Match,int>> GetAllScores()
        {
            return _battingStatsData.Where(a=>a.ModeOfDismissal != (int) ModesOfDismissal.DidNotBat).Select(a => new KeyValuePair<Match, int>(new Match(a.MatchID), a.Score)).OrderBy(a=>a.Key.MatchDate);
        }

        public bool WasNotOutIn(Match match)
        {
            int modeOfDismissal = _battingStatsData.Where(a => a.MatchID == match.ID).FirstOrDefault().ModeOfDismissal;
            if (modeOfDismissal == ((int)ModesOfDismissal.NotOut) || modeOfDismissal == ((int)ModesOfDismissal.RetiredHurt))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<KeyValuePair<Match, BattingCardLineData>> GetBattingStatsByMatch()
        {
            return _battingStatsData.Select(a => new KeyValuePair<Match,BattingCardLineData>(new Match(a.MatchID), a)).OrderBy(a=>a.Key.MatchDate);
        }

        public IEnumerable<KeyValuePair<Match, BowlingStatsEntryData>> GetBowlingStatsByMatch()
        {
            return _bowlingStatsData.Select(a => new KeyValuePair<Match,BowlingStatsEntryData>(new Match(a.MatchID), a));
        }

        public IEnumerable<KeyValuePair<Match, BattingCardLineData>> GetDismissedBatsmenData()
        {
            //TODO
            return null;
        }
    }
}

public class PlayerComparer : IEqualityComparer<Player>
{

    #region IEqualityComparer<Player> Members

    public bool Equals(Player x, Player y)
    {
        if (x.ID == y.ID) return true;
        return false;
    }

    public int GetHashCode(Player obj)
    {
        return obj.ID;
    }

    #endregion
}
