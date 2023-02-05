using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle.Interactive;
using CricketClubMiddle.Utility;

namespace CricketClubMiddle
{
    public class Player
    {
        private readonly PlayerData playerData;

        private static Func<T, bool> DefaultPredicate<T>(DateTime startDate, DateTime endDate,
            List<MatchType> matchTypes, Venue venue) where T : IStatsEntryData
        {
            return a =>
                a.MatchDate >= startDate && a.MatchDate <= endDate &&
                (matchTypes.Any(b => (int)b == a.MatchTypeID) || matchTypes.Contains(MatchType.All)) &&
                (venue == null || a.VenueID == venue.ID);
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
                BattingStatsData.Where(a => a.ModeOfDismissal != (int)ModesOfDismissal.DidNotBat)
                    .Select(a => new KeyValuePair<Match, int>(new Match(a.MatchID), a.Score))
                    .OrderBy(a => a.Key.MatchDate);
        }

        public bool WasNotOutIn(Match match)
        {
            var modeOfDismissal = BattingStatsData.FirstOrDefault(a => a.MatchID == match.ID)?.ModeOfDismissal??(int)ModesOfDismissal.DidNotBat;
            return modeOfDismissal == (int)ModesOfDismissal.NotOut ||
                   modeOfDismissal == (int)ModesOfDismissal.RetiredHurt;
        }

        public IEnumerable<KeyValuePair<Match, BattingCardLineData>> GetBattingStatsByMatch()
        {
            return
                BattingStatsData.Select(a => new KeyValuePair<Match, BattingCardLineData>(new Match(a.MatchID), a))
                    .OrderBy(a => a.Key.MatchDate);
        }

        public IEnumerable<KeyValuePair<Match, BowlingStatsEntryData>> GetBowlingStatsByMatch()
        {
            return BowlingStatsData.Select(
                a => new KeyValuePair<Match, BowlingStatsEntryData>(new Match(a.MatchID), a));
        }

        public Dictionary<Match, List<BattingCardLineData>> GetDismissedBatsmenData()
        {
            var playerFieldingStatsData = dao.GetPlayerFieldingStatsData(Id);
            return
                playerFieldingStatsData.Where(d => d.BowlerID == Id)
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
            var pd = new PlayerData { Name = playerName };
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

        public int Id
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
                if (firstSpace > 0)
                {
                    var surname = name.Substring(firstSpace);
                    var initials = name.Substring(0, firstSpace);
                    return surname.Trim() + ", " + initials.Trim();
                }

                return playerData.Name;
            }
        }

        public DateTime Dob
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
            set { playerData.RingerOf = value.Id; }
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
            get { return BattingStatsData.Select(a => a.MatchDate).OrderBy(a => a).FirstOrDefault(); }
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
            var matches = BattingStatsData
                .Where(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue))
                .Select(a => a).Count();
            return matches;
        }

        public int GetNumberOfMatchesPlayedIn(Predicate<BattingCardLineData> predicate)
        {
            var matches = BattingStatsData.Where(a => predicate(a)).Select(a => a).Count();
            return matches;
        }

        public int NumberOfMatchesPlayedThisSeason
        {
            get { return BattingStatsData.Count(b => b.MatchDate.Year == DateTime.Now.Year); }
        }

        public bool PlayedInMatch(int matchId)
        {
            return BattingStatsData.Any(a => a.MatchID == matchId);
        }

        #region Batting Stats

        private List<BattingCardLineData> battingStatsDataCache;

        private List<BattingCardLineData> BattingStatsData
        {
            get
            {
                if (battingStatsDataCache == null)
                {
                    var myDao = new Dao();
                    battingStatsDataCache = myDao.GetPlayerBattingStatsData(Id);
                }

                return battingStatsDataCache;
            }
        }

        public decimal GetBattingAverage()
        {
            return GetBattingAverage(a => true);
        }

        public decimal GetBattingAverage(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetBattingAverage(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public decimal GetBattingAverage(Func<BattingCardLineData, bool> predicate)
        {
            try
            {
                var average =
                    (decimal)GetRunsScored(
                        predicate) /
                    (GetInnings(predicate) -
                     GetNotOuts(predicate));
                return Math.Round(average, 2);
            }
            catch
            {
                return 0;
            }
        }

        public int GetRunsScored(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetRunsScored(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public int GetRunsScored(Func<BattingCardLineData, bool> predicate)
        {
            var runsScored = BattingStatsData.Where(predicate).Select(a => a.Score).Sum();
            return runsScored;
        }

        public int GetRunsScored(int matchId)
        {
            return GetRunsScored(a => a.MatchID == matchId);
        }

        public int GetRunsScored()
        {
            return GetRunsScored(a => true);
        }

        public int GetDucks(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetDucks(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public int GetDucks(Func<BattingCardLineData, bool> predicate)
        {
            return BattingStatsData
                .Where(predicate)
                .Where(a => a.Score == 0)
                .Count(a => (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.DidNotBat &&
                            (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.NotOut &&
                            (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.Retired);
        }

        public int GetDucks(int matchId)
        {
            return GetDucks(a => a.MatchID == matchId);
        }

        public int GetDucks()
        {
            return GetDucks(a => true);
        }


        public int Get100SScored(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return Get100SScored(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public int Get100SScored(Func<BattingCardLineData, bool> predicate)
        {
            return BattingStatsData
                .Where(predicate).Count(a => a.Score >= 100);
        }

        public int Get100SScored(int matchId)
        {
            return Get100SScored(a => a.MatchID == matchId);
        }

        public int Get100SScored()
        {
            return Get100SScored(a => true);
        }

        public int Get50SScored(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return Get50SScored(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public int Get50SScored(Func<BattingCardLineData, bool> predicate)
        {
            var fifties = BattingStatsData
                .Where(predicate)
                .Where(a => a.Score >= 50).Count(a => a.Score < 100);
            return fifties;
        }

        public int Get50SScored(int matchId)
        {
            return Get50SScored(a => a.MatchID == matchId);
        }

        public int Get50SScored()
        {
            return Get50SScored(a => true);
        }

        public int GetNotOuts(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetNotOuts(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public int GetNotOuts(Func<BattingCardLineData, bool> predicate)
        {
            return BattingStatsData
                .Where(predicate).Count(a => (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.NotOut ||
                                             (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.RetiredHurt);
        }

        public int GetNotOuts(int matchId)
        {
            return GetNotOuts(a => a.MatchID == matchId);
        }

        public int GetNotOuts()
        {
            return GetNotOuts(a => true);
        }

        public int GetInnings(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetInnings(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public int GetInnings(Func<BattingCardLineData, bool> predicate)
        {
            var knocks = (from a in BattingStatsData
                    .Where(predicate)
                where (ModesOfDismissal)a.ModeOfDismissal != ModesOfDismissal.DidNotBat
                select a).Count();
            return knocks;
        }

        public int GetBattingPosition()
        {
            return GetBattingPosition(a => true) ?? 11;
        }

        public int GetBattingPosition(int matchId)
        {
            return GetBattingPosition(a => a.MatchID == matchId) ??
                   throw new Exception("Player did not bat in match " + matchId);
        }

        public int GetBattingPosition(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetBattingPosition(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue)) ??
                   11;
        }

        public int? GetBattingPosition(Func<BattingCardLineData, bool> predicate)
        {
            try
            {
                var x =
                    BattingStatsData.Where(predicate)
                        .Select(a => a.BattingAt)
                        .Average();
                return (int)Math.Round(x, 0) + 1;
            }
            catch
            {
                return null;
            }
        }

        public int GetMatchesPlayed()
        {
            return TheGreaterOf(BattingStatsData.Count, BowlingStatsData.Count);
        }

        private int TheGreaterOf(int valueOne, int valueTwo)
        {
            return valueOne >= valueTwo ? valueOne : valueTwo;
        }

        public int GetMatchesPlayed(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetMatchesPlayed(DefaultPredicate<IStatsEntryData>(startDate, endDate, matchType, venue));
        }

        public int GetMatchesPlayed(Func<IStatsEntryData, bool> predicate)
        {
            var battingCount = BattingStatsData.Where(predicate).Count();
            var bowlingCount = BowlingStatsData.Where(predicate).Count();
            return TheGreaterOf(battingCount, bowlingCount);
        }


        public int GetInnings(int matchId)
        {
            return GetInnings(a => a.MatchID == matchId);
        }

        public int GetInnings()
        {
            return GetInnings(a => true);
        }

        public int GetHighScore(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetHighScore(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public int GetHighScore(Func<BattingCardLineData, bool> predicate)
        {
            try
            {
                return BattingStatsData.Where(predicate).Select(a => a.Score).Max();
            }
            catch
            {
                return 0;
            }
        }

        public int GetHighScore()
        {
            return GetHighScore(a => true);
        }

        public bool GetHighScoreWasNotOut()
        {
            return GetHighScoreWasNotOut(a => true);
        }

        public bool GetHighScoreWasNotOut(Func<BattingCardLineData, bool> predicate)
        {
            var dismissalIDs = BattingStatsData
                .Where(a => a.Score == GetHighScore(predicate))
                .Select(a => a.ModeOfDismissal).ToArray();
            return dismissalIDs.Contains((int)ModesOfDismissal.NotOut) ||
                   dismissalIDs.Contains((int)ModesOfDismissal.RetiredHurt);
        }

        public int Get4S(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return Get4S(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public int Get4S(Func<BattingCardLineData, bool> predicate)
        {
            return BattingStatsData.Where(predicate).Select(a => a.Fours).Sum();
        }

        public int Get4S(int matchId)
        {
            return Get4S(a => a.MatchID == matchId);
        }

        public int Get4S()
        {
            return Get4S(a => true);
        }

        public int Get6S(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return Get6S(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public int Get6S(Func<BattingCardLineData, bool> predicate)
        {
            return BattingStatsData.Where(predicate).Select(a => a.Sixes).Sum();
        }

        public int Get6S(int matchId)
        {
            return Get6S(a => a.MatchID == matchId);
        }

        public int Get6S()
        {
            return Get6S(a => true);
        }

        #endregion

        #region Bowling Stats

        private List<BowlingStatsEntryData> bowlingStatsDataCache;

        private List<BowlingStatsEntryData> BowlingStatsData
        {
            get
            {
                if (bowlingStatsDataCache == null)
                {
                    var myDao = new Dao();
                    bowlingStatsDataCache = myDao.GetPlayerBowlingStatsData(Id);
                }

                return bowlingStatsDataCache;
            }
        }

        public int GetWicketsTaken()
        {
            return GetWicketsTaken(a => true);
        }

        public int GetWicketsTaken(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetWicketsTaken(DefaultPredicate<BowlingStatsEntryData>(startDate, endDate, matchType, venue));
        }

        public int GetWicketsTaken(Func<BowlingStatsEntryData, bool> predicate)
        {
            return BowlingStatsData.Where(predicate).Select(a => a.Wickets).Sum();
        }

        public int GetWicketsTaken(int matchId)
        {
            return GetWicketsTaken(a => a.MatchID == matchId);
        }

        public decimal GetBowlingAverage()
        {
            return GetBowlingAverage(a => true);
        }

        public decimal GetBowlingAverage(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetBowlingAverage(DefaultPredicate<BowlingStatsEntryData>(startDate, endDate, matchType, venue));
        }

        public decimal GetBowlingAverage(Func<BowlingStatsEntryData, bool> predicate)
        {
            try
            {
                // ReSharper disable once PossibleLossOfFraction
                decimal fraction = GetRunsConceeded(predicate) /
                                   GetWicketsTaken(predicate);
                return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetBowlingAverage(int matchId)
        {
            return GetBowlingAverage(a => a.MatchID == matchId);
        }

        public decimal GetEconomy()
        {
            return GetEconomy(a => true);
        }

        public decimal GetEconomy(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetEconomy(DefaultPredicate<BowlingStatsEntryData>(startDate, endDate, matchType, venue));
        }

        public decimal GetEconomy(Func<BowlingStatsEntryData, bool> predicate)
        {
            try
            {
                var fraction = GetRunsConceeded(predicate) /
                               
                               GetOversBowled(predicate);
                return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetEconomy(int matchId)
        {
            return GetEconomy(a => a.MatchID == matchId);
        }


        public int GetFiveFers()
        {
            return GetFiveFers(a => true);
        }

        public int GetFiveFers(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetFiveFers(DefaultPredicate<BowlingStatsEntryData>(startDate, endDate, matchType, venue));
        }

        public int GetFiveFers(Func<BowlingStatsEntryData, bool> predicate)
        {
            return GetCountOfMatchesWithGreaterThanNWickets(predicate, 5);
        }

        private int GetCountOfMatchesWithGreaterThanNWickets(Func<BowlingStatsEntryData, bool> predicate, int wickets)
        {
            return BowlingStatsData
                .Where(predicate).Count(a => a.Wickets >= wickets);
        }

        public int GetFiveFers(int matchId)
        {
            return GetFiveFers(a => a.MatchID == matchId);
        }

        public int GetThreeFers()
        {
            return GetThreeFers(a => true);
        }

        public int GetThreeFers(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetThreeFers(DefaultPredicate<BowlingStatsEntryData>(startDate, endDate, matchType, venue));
        }

        public int GetThreeFers(Func<BowlingStatsEntryData, bool> predicate)
        {
            return GetCountOfMatchesWithGreaterThanNWickets(predicate, 3);
        }

        public int GetThreeFers(int matchId)
        {
            return GetThreeFers(a => a.MatchID == matchId);
        }

        public int GetTenFers()
        {
            return GetTenFers(a => true);
        }

        public int GetTenFers(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetTenFers(DefaultPredicate<BowlingStatsEntryData>(startDate, endDate, matchType, venue));
        }

        public int GetTenFers(Func<BowlingStatsEntryData, bool> predicate)
        {
            return GetCountOfMatchesWithGreaterThanNWickets(predicate, 10);
        }

        public int GetTenFers(int matchId)
        {
            return GetTenFers(a => a.MatchID == matchId);
        }

        public decimal GetStrikeRate()
        {
            return GetStrikeRate(a => true);
        }

        public decimal GetStrikeRate(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetStrikeRate(DefaultPredicate<BowlingStatsEntryData>(startDate, endDate, matchType, venue));
        }

        public decimal GetStrikeRate(Func<BowlingStatsEntryData, bool> predicate)
        {
            try
            {
                var fraction = GetOversBowled(predicate) * 6 /
                               GetWicketsTaken(predicate);
                return Math.Round(fraction, 2);
            }
            catch
            {
                return 0;
            }
        }

        public decimal GetStrikeRate(int matchId)
        {
            return GetStrikeRate(a => a.MatchID == matchId);
        }

        public decimal GetOversBowled()
        {
            return GetOversBowled(a => true);
        }

        public decimal GetOversBowled(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetOversBowled(DefaultPredicate<BowlingStatsEntryData>(startDate, endDate, matchType, venue));
        }

        public decimal GetOversBowled(Func<BowlingStatsEntryData, bool> predicate)
        {
            return BowlingStatsData.Where(predicate).Select(a => a.Overs).Sum();
        }

        public decimal GetOversBowled(int matchId)
        {
            return GetOversBowled(a => a.MatchID == matchId);
        }


        public int GetRunsConceeded()
        {
            return GetRunsConceeded(a => true);
        }

        public int GetRunsConceeded(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetRunsConceeded(DefaultPredicate<BowlingStatsEntryData>(startDate, endDate, matchType, venue));
        }

        public int GetRunsConceeded(Func<BowlingStatsEntryData, bool> predicate)
        {
            return BowlingStatsData.Where(predicate).Select(a => a.Runs).Sum();
        }

        public int GetRunsConceeded(int matchId)
        {
            return GetRunsConceeded(a => a.MatchID == matchId);
        }

        public string GetBestMatchFigures()
        {
            return GetBestMatchFigures(a => true);
        }

        public string GetBestMatchFigures(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetBestMatchFigures(DefaultPredicate<BowlingStatsEntryData>(startDate, endDate, matchType, venue));
        }

        public string GetBestMatchFigures(Func<BowlingStatsEntryData, bool> predicate)
        {
            int mostWickets;
            try
            {
                mostWickets =
                    BowlingStatsData.Where(
                            predicate)
                        .Select(a => a.Wickets).Max();
            }
            catch
            {
                return "0/0";
            }

            var runs = BowlingStatsData
                .Where(predicate)
                .Where(a => a.Wickets == mostWickets)
                .Select(a => a.Runs).Min();
            return mostWickets + "/" + runs;
        }

        public string GetMatchFigures(int matchId)
        {
            //Max/Min here are a bit lazy - there will only be one entry
            if (BowlingStatsData.Any())
            {
                var mostwickets = (from a in BowlingStatsData
                    where a.MatchID == matchId
                    select a.Wickets).Max();
                var runs = (from a in BowlingStatsData
                    where a.Wickets == mostwickets
                    where a.MatchID == matchId
                    select a.Runs).Min();
                return mostwickets + "/" + runs;
            }

            return "0/0";
        }

        #endregion

        #region Fielding Stats

        private List<BattingCardLineData> fieldingStatsDataCache;
        private readonly Dao dao;

        private List<BattingCardLineData> FieldingStatsData
        {
            get
            {
                if (fieldingStatsDataCache == null)
                {
                    var myDao = new Dao();
                    fieldingStatsDataCache = myDao.GetPlayerFieldingStatsData(Id);
                }

                return fieldingStatsDataCache;
            }
        }

        public int GetCatchesTaken()
        {
            return GetCatchesTaken(a => true);
        }

        public int GetCatchesTaken(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetCatchesTaken(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public int GetCatchesTaken(Func<BattingCardLineData, bool> predicate)
        {
            return FieldingStatsData
                .Where(predicate)
                .Count(a =>
                    ((ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.Caught && a.FielderID == Id) ||
                    ((ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.CaughtAndBowled &&
                     a.BowlerID == Id));
        }

        public int GetCatchesTaken(int matchId)
        {
            return GetCatchesTaken(a => a.MatchID == matchId);
        }

        public int GetStumpings()
        {
            return GetStumpings(a => true);
        }

        public int GetStumpings(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetStumpings(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public int GetStumpings(Func<BattingCardLineData, bool> predicate)
        {
            return FieldingStatsData
                .Where(predicate)
                .Where(a => a.FielderID == Id).Count(a => (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.Stumped);
        }

        public int GetStumpings(int matchId)
        {
            return GetStumpings(a => a.MatchID == matchId);
        }

        public int GetRunOuts()
        {
            return GetRunOuts(a => true);
        }

        public int GetRunOuts(DateTime startDate, DateTime endDate, List<MatchType> matchType, Venue venue)
        {
            return GetRunOuts(DefaultPredicate<BattingCardLineData>(startDate, endDate, matchType, venue));
        }

        public int GetRunOuts(Func<BattingCardLineData, bool> predicate)
        {
            return FieldingStatsData
                .Where(predicate)
                .Where(a => a.FielderID == Id)
                .Count(a => (ModesOfDismissal)a.ModeOfDismissal == ModesOfDismissal.RunOut);
        }

        public int GetRunOuts(int matchId)
        {
            return GetRunOuts(a => a.MatchID == matchId);
        }

        #endregion

        #endregion
    }
}