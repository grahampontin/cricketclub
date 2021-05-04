using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle;
using CricketClubMiddle.Interactive;
using CricketClubMiddle.Utility;

namespace CricketClubMiddle
{
    public class Player
    {
        #region Members

        private readonly PlayerData playerData;

        #endregion

        private static IEnumerable<BattingCardLineData> FilterData(List<BattingCardLineData> data, DateTime startDate,
            DateTime endDate, List<MatchType> matchTypes, Venue venue)
        {
            return from a in data
                where a.MatchDate >= startDate || startDate == null
                where a.MatchDate <= endDate || endDate == null
                where matchTypes.Any(b => (int) b == a.MatchTypeID) || matchTypes.Contains(MatchType.All)
                where venue == null || a.VenueID == venue.ID
                select a;
        }


        private static IEnumerable<BowlingStatsEntryData> FilterBowlingData(List<BowlingStatsEntryData> data,
            DateTime startDate, DateTime endDate, List<MatchType> matchTypes, Venue venue)
        {
            return from a in data
                where a.MatchDate >= startDate || startDate == null
                where a.MatchDate <= endDate || endDate == null
                where matchTypes.Any(b => (int) b == a.MatchTypeID) || matchTypes.Contains(MatchType.All)
                where venue == null || a.VenueID == venue.ID
                select a;
        }

        public override string ToString()
        {
            return FormalName;
        }

        public bool AssociateWithUser(User user)
        {
            var alreadyAssigned = GetAll().Where(a => a.EmailAddress == user.EmailAddress).Any();
            if (alreadyAssigned)
            {
                return false;
            }
            EmailAddress = user.EmailAddress;
            Save();
            return true;
        }

        public IEnumerable<KeyValuePair<Match, int>> GetAllScores()
        {
            return
                _battingStatsData.Where(a => a.ModeOfDismissal != (int) ModesOfDismissal.DidNotBat)
                    .Select(a => new KeyValuePair<Match, int>(new Match(a.MatchID), a.Score))
                    .OrderBy(a => a.Key.MatchDate);
        }

        public bool WasNotOutIn(Match match)
        {
            var modeOfDismissal = _battingStatsData.Where(a => a.MatchID == match.ID).FirstOrDefault().ModeOfDismissal;
            if (modeOfDismissal == (int) ModesOfDismissal.NotOut ||
                modeOfDismissal == (int) ModesOfDismissal.RetiredHurt)
            {
                return true;
            }
            return false;
        }

        public IEnumerable<KeyValuePair<Match, BattingCardLineData>> GetBattingStatsByMatch()
        {
            return
                _battingStatsData.Select(a => new KeyValuePair<Match, BattingCardLineData>(new Match(a.MatchID), a))
                    .OrderBy(a => a.Key.MatchDate);
        }

        public IEnumerable<KeyValuePair<Match, BowlingStatsEntryData>> GetBowlingStatsByMatch()
        {
            return _bowlingStatsData.Select(a => new KeyValuePair<Match, BowlingStatsEntryData>(new Match(a.MatchID), a));
        }
        
        public Dictionary<Match, List<BattingCardLineData>> GetDismissedBatsmenData()
        {
            var playerFieldingStatsData = dao.GetPlayerFieldingStatsData(ID);
            return
                playerFieldingStatsData.Where(d => d.BowlerID == ID)
                    .GroupBy(d => d.MatchID)
                    .ToDictionary(g => new Match(g.Key), g => g.ToList());
        }

        #region Constructors

        public Player(int playerId)
        {
            //Get the player specified by this ID
            dao = new Dao();
            playerData = dao.GetPlayerData(playerId);
        }

        /// <summary>
        ///     Used by GetAll();
        /// </summary>
        /// <param name="data"></param>
        private Player(PlayerData data)
        {
            playerData = data;
        }

        /// <summary>
        ///     A special constructor for use for representing opposition players - returns an object with only the name set.
        /// </summary>
        /// <param name="playerName"></param>
        public Player(string playerName)
        {
            var pd = new PlayerData {Name = playerName};
            playerData = pd;
        }

        public static Player CreateNewPlayer(string name)
        {
            //Creates a new player.
            var newPlayerId = new Dao().CreateNewPlayer(name);
            return new Player(newPlayerId);
        }

        public static List<Player> GetAll()
        {
            var data = new Dao().GetAllPlayers();
            return (from a in data select new Player(a)).OrderBy(a => a.FormalName).ToList();
        }

        #endregion

        #region Properties

        public int ID
        {
            get { return playerData.ID; }
        }

        public string EmailAddress
        {
            get { return playerData.EmailAddress; }
            set { playerData.EmailAddress = value; }
        }

        /// <summary>
        ///     Setter is obsolete - use First Name and Surname
        /// </summary>
        public string Name
        {
            get
            {
                if (FirstName != "" && Surname != "" && FirstName != null && Surname != null)
                {
                    return FirstName.Substring(0, 1).ToUpper() + MiddleInitials + " " + Surname;
                }
                return playerData.Name;
            }
            set { playerData.Name = value; }
        }

        /// <summary>
        ///     Surname, Firstname
        /// </summary>
        public string FormalName
        {
            get
            {
                if (FirstName != "" && Surname != "")
                {
                    return Surname + ", " + FirstName;
                }
                var name = playerData.Name;
                var firstSpace = name.IndexOf(' ');
                var surname = "";
                var initials = "";
                if (firstSpace > 0)
                {
                    surname = name.Substring(firstSpace);
                    initials = name.Substring(0, firstSpace);
                    return surname.Trim() + ", " + initials.Trim();
                }
                return playerData.Name;
            }
        }

        public DateTime DOB
        {
            get { return playerData.DateOfBirth; }
            set { playerData.DateOfBirth = value; }
        }

        public string FullName
        {
            get { return playerData.FullName; }
            set { playerData.FullName = value; }
        }

        public string FirstName
        {
            get { return playerData.FirstName; }
            set { playerData.FirstName = value; }
        }

        public string Surname
        {
            get { return playerData.Surname; }
            set { playerData.Surname = value; }
        }

        public string MiddleInitials
        {
            get { return playerData.MiddleInitials; }
            set { playerData.MiddleInitials = value; }
        }

        public Player RingerOf
        {
            get
            {
                if (playerData.RingerOf > 0)
                {
                    return new Player(playerData.RingerOf);
                }
                return null;
            }
            set { playerData.RingerOf = value.ID; }
        }

        public string Nickname
        {
            get { return playerData.NickName; }
            set { playerData.NickName = value; }
        }

        public string Location
        {
            get { return playerData.Location; }
            set { playerData.Location = value; }
        }

        public string BowlingStyle
        {
            get { return playerData.BowlingStyle; }
            set { playerData.BowlingStyle = value; }
        }

        public string BattingStyle
        {
            get { return playerData.BattingStyle; }
            set { playerData.BattingStyle = value; }
        }

        public PlayingRole PlayingRole { get; set; }

        public int Caps
        {
            get { return GetMatchesPlayed(); }
        }

        public DateTime Debut
        {
            get { return _battingStatsData.Select(a => a.MatchDate).OrderBy(a => a).FirstOrDefault(); }
        }

        public string Education
        {
            get { return playerData.Education; }
            set { playerData.Education = value; }
        }

        public string Height
        {
            get { return playerData.Height; }
            set { playerData.Height = value; }
        }

        public bool IsActive
        {
            get { return playerData.IsActive; }
            set { playerData.IsActive = value; }
        }
        
        public bool IsRightHandBat
        {
            get { return playerData.IsRightHandBat; }
            set { playerData.IsRightHandBat = value; }
        }

        public string Bio
        {
            get
            {
                var filename = FirstName + "_" + Surname + "_BIO.html";
                var path = SettingsWrapper.GetSettingString("BioFolder", "/Players/bios/") + filename;
                path = HttpContext.Current.Server.MapPath(path);
                if (File.Exists(path))
                {
                    var stream = new StreamReader(path);
                    var temp = stream.ReadToEnd();
                    stream.Close();
                    return temp;
                }
                return "No player bio has been created.";
            }
            set
            {
                var filename = FirstName + "_" + Surname + "_BIO.html";
                var path = SettingsWrapper.GetSettingString("BioFolder", "/Players/bios/") + filename;
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
            if (playerData.ID != 0)
            {
                var myDao = new Dao();
                myDao.UpdatePlayer(playerData);
            }
            else
            {
                throw new InvalidOperationException("Player has no player ID - is this an opposition player?");
            }
        }

        public int GetNumberOfMatchesPlayedIn(DateTime startDate, DateTime endDate, List<MatchType> matchType,
            Venue venue)
        {
            var matches = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue)
                select a).Count();
            return matches;
        }

        public int NumberOfMatchesPlayedThisSeason
        {
            get { return _battingStatsData.Count(b => b.MatchDate.Year == DateTime.Now.Year); }
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
                    var myDao = new Dao();
                    _battingStatsDataCache = myDao.GetPlayerBattingStatsData(ID);
                }
                return _battingStatsDataCache;
            }
        }

        public decimal GetBattingAverage()
        {
            try
            {
                var average = (decimal) GetRunsScored()/(GetInnings() - GetNotOuts());
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
                var average = (decimal) GetRunsScored(startDate, endDate, matchType, venue)/
                              (GetInnings(startDate, endDate, matchType, venue) -
                               GetNotOuts(startDate, endDate, matchType, venue));
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
                where (ModesOfDismissal) a.ModeOfDismissal != ModesOfDismissal.DidNotBat &&
                      (ModesOfDismissal) a.ModeOfDismissal != ModesOfDismissal.NotOut &&
                      (ModesOfDismissal) a.ModeOfDismissal != ModesOfDismissal.Retired
                select a).Count();
            return ducks;
        }

        public int GetDucks(int MatchID)
        {
            var tons = (from a in _battingStatsData
                where a.MatchID == MatchID
                where a.Score == 0
                where (ModesOfDismissal) a.ModeOfDismissal != ModesOfDismissal.DidNotBat &&
                      (ModesOfDismissal) a.ModeOfDismissal != ModesOfDismissal.NotOut &&
                      (ModesOfDismissal) a.ModeOfDismissal != ModesOfDismissal.Retired
                select a).Count();
            return tons;
        }

        public int GetDucks()
        {
            var tons = (from a in _battingStatsData
                where a.Score == 0
                where (ModesOfDismissal) a.ModeOfDismissal != ModesOfDismissal.DidNotBat &&
                      (ModesOfDismissal) a.ModeOfDismissal != ModesOfDismissal.NotOut &&
                      (ModesOfDismissal) a.ModeOfDismissal != ModesOfDismissal.Retired
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
                where (ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.NotOut ||
                      (ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.RetiredHurt
                select a).Count();
            return notouts;
        }

        public int GetNotOuts(int MatchID)
        {
            var notouts = (from a in _battingStatsData
                where (ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.NotOut ||
                      (ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.RetiredHurt
                where a.MatchID == MatchID
                select a).Count();
            return notouts;
        }

        public int GetNotOuts()
        {
            var notouts = (from a in _battingStatsData
                where (ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.NotOut ||
                      (ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.RetiredHurt
                select a).Count();
            return notouts;
        }

        public int GetInnings(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var knocks = (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue)
                where (ModesOfDismissal) a.ModeOfDismissal != ModesOfDismissal.DidNotBat
                select a).Count();
            return knocks;
        }

        public int GetBattingPosition()
        {
            if (_battingStatsData.Count > 0)
            {
                var x = (from a in _battingStatsData select a.BattingAt).Average();
                return (int) Math.Round(x, 0);
            }
            return 11;
        }

        public int GetBattingPosition(int MatchID)
        {
            try
            {
                var x = (from a in _battingStatsData.Where(a => a.MatchID == MatchID) select a.BattingAt).Average();
                return (int) Math.Round(x, 0) + 1;
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
                var x =
                    (from a in FilterData(_battingStatsData, startDate, endDate, matchType, venue) select a.BattingAt)
                        .Average();
                return (int) Math.Round(x, 0) + 1;
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
            var battingCount = FilterData(_battingStatsData, startDate, endDate, matchType, venue).Count();
            var bowlingCount = FilterBowlingData(_bowlingStatsData, startDate, endDate, matchType, venue).Count();
            return TheGreaterOf(battingCount, bowlingCount);
        }


        public int GetInnings(int MatchID)
        {
            var knocks = (from a in _battingStatsData
                where (ModesOfDismissal) a.ModeOfDismissal != ModesOfDismissal.DidNotBat
                where a.MatchID == MatchID
                select a).Count();
            return knocks;
        }

        public int GetInnings()
        {
            var knocks = (from a in _battingStatsData
                where (ModesOfDismissal) a.ModeOfDismissal != ModesOfDismissal.DidNotBat
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
                where a.Score == GetHighScore()
                select a.ModeOfDismissal;
            if (dismissalIDs.Contains((int) ModesOfDismissal.NotOut) ||
                dismissalIDs.Contains((int) ModesOfDismissal.RetiredHurt))
            {
                return true;
            }
            return false;
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
                    var myDao = new Dao();
                    _bowlingStatsDataCache = myDao.GetPlayerBowlingStatsData(ID);
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
                decimal fraction = GetRunsConceeded()/GetWicketsTaken();
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
                decimal fraction = GetRunsConceeded(startDate, endDate, matchType, venue)/
                                   GetWicketsTaken(startDate, endDate, matchType, venue);
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
                decimal fraction = GetRunsConceeded(MatchID)/GetWicketsTaken(MatchID);
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
                var fraction = GetRunsConceeded()/GetOversBowled();
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
                var fraction = GetRunsConceeded(startDate, endDate, matchType, venue)/
                               GetOversBowled(startDate, endDate, matchType, venue);
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
                var fraction = GetRunsConceeded(MatchID)/GetOversBowled(MatchID);
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
            try
            {
                var fraction = GetOversBowled()*6/GetWicketsTaken();
                return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetStrikeRate(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            try
            {
                var fraction = GetOversBowled(startDate, endDate, matchType, venue)*6/
                               GetWicketsTaken(startDate, endDate, matchType, venue);
                return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetStrikeRate(int MatchID)
        {
            try
            {
                var fraction = GetOversBowled(MatchID)*6/GetWicketsTaken(MatchID);
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
            var rc = (from a in FilterBowlingData(_bowlingStatsData, startDate, endDate, matchType, venue)
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
            return mostwickets + "/" + runs;
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
            return mostwickets + "/" + runs;
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
                return mostwickets + "/" + runs;
            }
            return "0/0";
        }

        #endregion

        #region Fielding Stats

        private List<BattingCardLineData> _fieldingStatsDataCache;
        private readonly Dao dao;

        private List<BattingCardLineData> _fieldingStatsData
        {
            get
            {
                if (_fieldingStatsDataCache == null)
                {
                    var myDao = new Dao();
                    _fieldingStatsDataCache = myDao.GetPlayerFieldingStatsData(ID);
                }
                return _fieldingStatsDataCache;
            }
        }

        public int GetCatchesTaken()
        {
            var ct = (from a in _fieldingStatsData
                where ((ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.Caught && a.FielderID == ID)
                      ||
                      ((ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.CaughtAndBowled && a.BowlerID == ID)
                select a).Count();
            return ct;
        }

        public int GetCatchesTaken(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var ct = (from a in FilterData(_fieldingStatsData, startDate, endDate, matchType, venue)
                where ((ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.Caught && a.FielderID == ID)
                      ||
                      ((ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.CaughtAndBowled && a.BowlerID == ID)
                select a).Count();
            return ct;
        }

        public int GetCatchesTaken(int MatchID)
        {
            var ct = (from a in _fieldingStatsData
                where ((ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.Caught && a.FielderID == ID)
                      ||
                      ((ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.CaughtAndBowled && a.BowlerID == ID)
                where a.MatchID == MatchID
                select a).Count();
            return ct;
        }

        public int GetStumpings()
        {
            var stmp = (from a in _fieldingStatsData
                where a.FielderID == ID
                where (ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.Stumped
                select a).Count();
            return stmp;
        }

        public int GetStumpings(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var stmp = (from a in FilterData(_fieldingStatsData, startDate, endDate, matchType, venue)
                where a.FielderID == ID
                where (ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.Stumped
                select a).Count();
            return stmp;
        }

        public int GetStumpings(int MatchID)
        {
            var stmp = (from a in _fieldingStatsData
                where a.FielderID == ID
                where a.MatchID == MatchID
                where (ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.Stumped
                select a).Count();
            return stmp;
        }

        public int GetRunOuts()
        {
            var stmp = (from a in _fieldingStatsData
                where a.FielderID == ID
                where (ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.RunOut
                select a).Count();
            return stmp;
        }

        public int GetRunOuts(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            var stmp = (from a in FilterData(_fieldingStatsData, startDate, endDate, matchType, venue)
                where a.FielderID == ID
                where (ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.RunOut
                select a).Count();
            return stmp;
        }

        public int GetRunOuts(int MatchID)
        {
            var stmp = (from a in _fieldingStatsData
                where a.FielderID == ID
                where a.MatchID == MatchID
                where (ModesOfDismissal) a.ModeOfDismissal == ModesOfDismissal.RunOut
                select a).Count();
            return stmp;
        }

        #endregion

        #endregion
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