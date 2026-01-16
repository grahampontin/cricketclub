using System;
using System.Collections.Generic;
using System.Linq;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle.Stats;

namespace CricketClubMiddle
{
    public class Match
    {
        private readonly IDao dao;
        private readonly MatchData data;
        private BattingCard ourBatting;
        private BowlingStats ourBowling;
        private BattingCard theirBatting;
        private BowlingStats theirBowling;
        private Team us;
        private Team opposition;
        private Player captain;
        private Player wicketKeeper;
        private Venue venue;

        public Match(int MatchID) : this(MatchID, new Dao())
        {
        }

        public Match(int MatchID, IDao dao)
        {
            this.dao = dao;
            data = dao.GetMatchData(MatchID);
        }

        private Match(MatchData data, IDao dao)
        {
            this.data = data;
            this.dao = dao;
        }

        public int ID => data.ID;

        public int VenueID
        {
            get => data.VenueID;
            set => data.VenueID = value;
        }

        public Venue Venue
        {
            get
            {
                if (venue != null) return venue;
                venue = new Venue(VenueID, dao);
                return venue;
            }
        }

        public string VenueName => Venue.Name;

        public DateTime MatchDate
        {
            get => data.Date;
            set => data.Date = value;
        }

        public string MatchDateString =>
            MatchDate.DayOfWeek.ToString().Substring(0, 3) + " " + MatchDate.ToLongDateString();

        public string MatchDateStartString => MatchDate.ToString("yyyy-MM-dd") + " 10:00";

        public string MatchDateEndString => MatchDate.ToString("yyyy-MM-dd") + " 19:00";

        public MatchType Type
        {
            get => (MatchType) data.MatchType;
            set => data.MatchType = (int) value;
        }

        public int OppositionID
        {
            get => data.OppositionID;
            set => data.OppositionID = value;
        }

        public Team Opposition
        {
            get
            {
                if (opposition != null) return opposition;
                opposition = new Team(OppositionID, dao);
                return opposition;
            }
        }

        public Team Us
        {
            get
            {
                if (us != null) return us;
                us = new Team(0, dao);
                return us;
            }
        }

        public bool Abandoned
        {
            get => data.Abandoned;

            set => data.Abandoned = value;
        }

        public Team TossWinner => data.WonToss ? Us : Opposition;

        /// <summary>
        ///     Did the special case team (The Village) win the toss - try not to use - prefer TossWinner
        /// </summary>
        public bool WonToss
        {
            get => data.WonToss;
            set => data.WonToss = value;
        }

        /// <summary>
        ///     Did the side that won the toss choose to Bat?
        /// </summary>
        public bool TossWinnerBatted
        {
            get => data.Batted;
            set => data.Batted = value;
        }

        /// <summary>
        ///     What did the toss winner do? returns "bat" or "field"
        /// </summary>
        public string TossWinnerElectedTo
        {
            get
            {
                if (TossWinnerBatted) return "bat";

                return "field";
            }
        }

        public HomeOrAway HomeOrAway
        {
            get
            {
                if (data.HomeOrAway.ToUpper() == "H") return HomeOrAway.Home;

                return HomeOrAway.Away;
            }

            set => data.HomeOrAway = value.ToString().Substring(0, 1);
        }

        public Team HomeTeam
        {
            get
            {
                if (HomeOrAway == HomeOrAway.Home) return Us;

                return Opposition;
            }
        }

        public Team AwayTeam
        {
            get
            {
                if (HomeOrAway == HomeOrAway.Away) return Us;

                return Opposition;
            }
        }

        public string HomeTeamName => HomeTeam.Name;

        public string AwayTeamName => AwayTeam.Name;

        public string HomeTeamScore
        {
            get
            {
                var result = GetTeamScore(HomeTeam) + " for " + GetTeamWicketsDown(HomeTeam);
                result = result.Replace("for 10", "all out");
                if (HomeOrAway == HomeOrAway.Home && WeDeclared) result = result + " dec";

                if (HomeOrAway == HomeOrAway.Away && TheyDeclared) result = result + " dec";

                return result;
            }
        }

        public string AwayTeamScore
        {
            get
            {
                var result = GetTeamScore(AwayTeam) + " for " + GetTeamWicketsDown(AwayTeam);
                result = result.Replace("for 10", "all out");

                if (HomeOrAway == HomeOrAway.Away && WeDeclared) result = result + " dec";

                if (HomeOrAway == HomeOrAway.Home && TheyDeclared) result = result + " dec";

                return result;
            }
        }

        public bool ResultDrawn
        {
            get
            {
                if (Type == MatchType.Declaration)
                    if (GetTeamScore(TeamBattingSecond()) < GetTeamScore(TeamBattingFirst()) &&
                        GetTeamWicketsDown(TeamBattingSecond()) < 10)
                        return true;

                return false;
            }
        }

        public bool ResultTied
        {
            get
            {
                if (GetTeamScore(Us) == GetTeamScore(Opposition) && GetTeamScore(Us) > 0 &&
                    GetTeamScore(Opposition) > 0)
                    return true;

                return false;
            }
        }

        public Team Winner
        {
            get
            {
                if (GetTeamScore(Us) > GetTeamScore(Opposition) && !ResultDrawn) return Us;

                if (GetTeamScore(Us) < GetTeamScore(Opposition) && !ResultDrawn) return Opposition;

                return null;
            }
        }

        public Team Loser
        {
            get
            {
                if (GetTeamScore(Us) < GetTeamScore(Opposition) && !ResultDrawn) return Us;

                if (GetTeamScore(Us) > GetTeamScore(Opposition) && !ResultDrawn) return Opposition;

                return null;
            }
        }

        public string ResultText
        {
            get
            {
                if (Abandoned) return "abandoned";

                if (Winner != null && Winner.ID == Us.ID)
                {
                    if (HomeOrAway == HomeOrAway.Home) return "beat";

                    return "lost to";
                }

                if (Winner != null && Winner.ID == Opposition.ID)
                    switch (HomeOrAway)
                    {
                        case HomeOrAway.Away:
                            return "beat";
                        default:
                            return "lost to";
                    }

                if (ResultDrawn) return "drew with";

                if (ResultTied) return "tied with";

                return "vs";
            }
        }

        public string ResultMargin
        {
            get
            {
                if (null != Winner)
                {
                    if (TeamBattingFirst().ID == Winner.ID)
                    {
                        var margin = GetTeamScore(Winner) - GetTeamScore(Loser);
                        var resultText = "by " + margin + " runs";
                        if (margin == 1) resultText = resultText.Substring(0, resultText.Length - 1);

                        return resultText;
                    }
                    else
                    {
                        var margin = 10 - GetTeamWicketsDown(Winner);
                        var resultText = "by " + margin + " wickets";
                        if (margin == 1) resultText = resultText.Substring(0, resultText.Length - 1);

                        return resultText;
                    }
                }

                if (!Abandoned && !ResultDrawn) return "result not yet in";

                if (ResultDrawn) return "";

                return "no result";
            }
        }

        public Player Captain
        {
            get
            {
                if (captain != null) return captain;
                captain = new Player(data.CaptainID, dao);
                return captain;
            }
            set
            {
                captain = null;
                data.CaptainID = value.Id;
            }
        }

        public Player WicketKeeper
        {
            get
            {
                if (wicketKeeper != null) return wicketKeeper;
                wicketKeeper = new Player(data.WicketKeeperID, dao);
                return wicketKeeper;
            }
            set
            {
                wicketKeeper = null;
                data.WicketKeeperID = value.Id;
            }
        }

        public int Overs
        {
            get => data.Overs;
            set => data.Overs = value;
        }

        public bool WasDeclaration
        {
            get => data.WasDeclarationGame;
            set => data.WasDeclarationGame = value;
        }

        public bool WeDeclared
        {
            get => data.WeDeclared;
            set => data.WeDeclared = value;
        }

        public bool TheyDeclared
        {
            get => data.TheyDeclared;
            set => data.TheyDeclared = value;
        }

        public double OurInningsLength
        {
            get => data.OurInningsLength;
            set => data.OurInningsLength = value;
        }

        public double TheirInningsLength
        {
            get => data.TheirInningsLength;
            set => data.TheirInningsLength = value;
        }

        public string Description => HomeTeam + " vs " + AwayTeam + " (" + MatchDateString + ")";

        // ReSharper disable once UnusedMember.Global
        public int BallByBallOver => GetCurrentBallByBallState().LastCompletedOver;

        public bool OppositionInningsComplete => GetCurrentBallByBallState().GetInningsStatus().TheirInningsStatus ==
                                                 InningsStatus.Completed;

        public bool OppositionBattedFirst =>
            TossWinner.IsUs && !TossWinnerBatted || !TossWinner.IsUs && TossWinnerBatted;

        public bool OurInningsInProgress
        {
            get
            {
                var currentBallByBallState = GetCurrentBallByBallState();
                return currentBallByBallState.GetInningsStatus().OurInningsStatus == InningsStatus.InProgress;
            }
        }

        public bool TheirInningsInProgress
        {
            get
            {
                var currentBallByBallState = GetCurrentBallByBallState();
                return currentBallByBallState.GetInningsStatus().TheirInningsStatus == InningsStatus.InProgress;
            }
        }

        public int OppositionBallByBallOver => GetCurrentBallByBallState().OppositionOver;

        public static Match CreateNewMatch(Team opposition, DateTime matchDay, Venue venue, MatchType type,
            HomeOrAway homeAway)
        {
            return CreateNewMatch(opposition, matchDay, venue, type, homeAway, new Dao());
        }

        public static Match CreateNewMatch(Team opposition, DateTime matchDay, Venue venue, MatchType type,
            HomeOrAway homeAway, IDao dao)
        {
            var id = dao.CreateNewMatch(opposition.ID, matchDay, venue.ID, (int) type,
                homeAway);
            return new Match(id, dao);
        }

        public static Match GetNextMatch()
        {
            return GetNextMatch(new Dao());
        }

        public static Match GetNextMatch(IDao dao)
        {
            var matchID = dao.GetNextMatch(DateTime.Today);
            if (matchID >= 0)
                return new Match(matchID, dao);
            return null;
        }

        public static Match GetLastMatch()
        {
            return GetLastMatch(new Dao());
        }

        public static Match GetLastMatch(IDao dao)
        {
            var matchID = dao.GetPreviousMatch(DateTime.Today);
            if (matchID >= 0) return new Match(matchID, dao);

            return null;
        }

        public Match GetPreviousMatch()
        {
            var myDao = dao ?? new Dao();
            var matchID = myDao.GetPreviousMatch(MatchDate);
            if (matchID >= 0)
                return new Match(matchID, myDao);
            return null;
        }

        public static IList<Match> GetFixtures()
        {
            return GetFixtures(new Dao());
        }

        // Added overload for testing - allow injection of a DAO
        public static IList<Match> GetFixtures(IDao dao)
        {
            // inline BuildFixturesQuery: preserve original semantics (inclusive today's fixtures)
            return BuildMatchQuery(dao)
                .Where(a => a.MatchDate >= DateTime.Today)
                .OrderBy(a => a.MatchDate)
                .ToList();
        }

        public static IList<Match> GetResults()
        {
            return GetResults(new Dao());
        }

        // Overload for testing / DAO injection
        public static IList<Match> GetResults(IDao dao)
        {
            // inline BuildResultsQuery: preserve original semantics (inclusive today's results)
            return BuildMatchQuery(dao)
                .Where(a => a.MatchDate <= DateTime.Today)
                .OrderBy(a => a.MatchDate)
                .ToList();
        }

        public static IList<Match> GetResults(DateTime startDate, DateTime endDate)
        {
            return GetResults(new Dao()).Where(a => a.MatchDate > startDate && a.MatchDate < endDate).ToList();
        }

        // Refactored: central query builder to allow reuse of filters between overloads
        private static IEnumerable<Match> BuildMatchQuery(IDao dao)
        {
            var data = dao.GetAllMatches();
            return data.Select(a => new Match(a, dao));
        }

        public static IList<Match> GetAll(DateTime fromDate, DateTime toDate, List<MatchType> matchTypes, Venue venue, IDao dao)
        {
            var q = BuildMatchQuery(dao);
            if (fromDate != DateTime.MinValue)
                q = q.Where(m => m.MatchDate > fromDate);

            if (toDate != DateTime.MinValue)
                q = q.Where(m => m.MatchDate < toDate);

            if (!matchTypes.IsNullOrEmpty())
                q = q.Where(m => matchTypes.Contains(m.Type));

            if (venue != null)
                q = q.Where(m => m.Venue == venue);

            return q.ToList();
        }

        public static IList<Match> GetAll(DateTime fromDate, DateTime toDate, List<MatchType> matchTypes, Venue venue)
        {
            return GetAll(fromDate, toDate, matchTypes, venue, new Dao());
        }

        // Make this overload public so tests can pass a mocked Dao
        public static IList<Match> GetAll(IDao dao)
        {
            return BuildMatchQuery(dao).ToList();
        }

        public void Save()
        {
            ClearCache();
            if (data.ID != 0)
            {
                var myDao = dao ?? new Dao();
                myDao.UpdateMatch(data);
            }
            else
            {
                throw new InvalidOperationException("Match has no Match ID");
            }
        }

        public Team TeamBattingFirst()
        {
            Team teamBattingFirst;
            if (TossWinnerBatted)
            {
                teamBattingFirst = TossWinner;
            }
            else
            {
                if (0 == TossWinner.ID)
                    teamBattingFirst = Opposition;
                else
                    teamBattingFirst = Us;
            }

            return teamBattingFirst;
        }

        public Team TeamBattingSecond()
        {
            if (TeamBattingFirst().ID == 0) return Opposition;

            return Us;
        }

        public BattingCard GetOurBattingScoreCard()
        {
            if (ourBatting == null) ourBatting = new BattingCard(ID, ThemOrUs.Us, dao);

            return ourBatting;
        }

        public BattingCard GetTheirBattingScoreCard()
        {
            if (theirBatting == null) theirBatting = new BattingCard(ID, ThemOrUs.Them, dao);

            return theirBatting;
        }

        public BowlingStats GetOurBowlingStats()
        {
            if (ourBowling == null) ourBowling = new BowlingStats(ID, ThemOrUs.Us, dao);

            return ourBowling;
        }

        public BowlingStats GetThierBowlingStats()
        {
            if (theirBowling == null) theirBowling = new BowlingStats(ID, ThemOrUs.Them, dao);

            return theirBowling;
        }


        /// <summary>
        ///     Get the score for the home or away team
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public int GetTeamScore(Team team)
        {
            var sc = GetScoreCardForTeam(team);
            var score = 0;
            if (sc != null && sc.ScorecardData != null)
                score = sc.ScorecardData.Select(a => a.Score).Sum();
            // guard extras in case sc is null (defensive)
            var extras = sc != null ? sc.Extras : 0;
            return score + extras;
        }

        public int GetTeamWicketsDown(Team team)
        {
            var sc = GetScoreCardForTeam(team);
            if (sc == null || sc.ScorecardData == null) return 0;
            return sc.ScorecardData
                .Where(a => a.Dismissal != ModesOfDismissal.NotOut)
                .Where(a => a.Dismissal != ModesOfDismissal.DidNotBat)
                .Where(a => a.Dismissal != ModesOfDismissal.RetiredHurt)
                .Where(a => a.BattingAt != 12)
                .Count();
        }

        private BattingCard GetScoreCardForTeam(Team team)
        {
            BattingCard sc;
            if (team.ID == Us.ID)
                sc = GetOurBattingScoreCard();
            else if (team.ID == Opposition.ID)
                sc = GetTheirBattingScoreCard();
            else
                throw new Exception("The team you specified didn't play in this game");

            return sc;
        }

        public decimal GetTeamOversBowled(Team team)
        {
            return 0;
        }

        public override string ToString()
        {
            return Opposition.Name + " (" + MatchDate.ToShortDateString() + ")";
        }

        public void ClearCache()
        {
            ourBatting = null;
            ourBowling = null;
            theirBatting = null;
            theirBowling = null;
        }

        public MatchReport GetMatchReport(string folder)
        {
            return new MatchReport(ID, folder);
        }

        public void CreateOrUpdateMatchReport(string conditions, string report, string reportImage)
        {
            var myDao = dao ?? new Dao();
            myDao.CreateOrUpdateMatchReport(ID, conditions, report, reportImage);
        }

        public MatchReportAndConditions GetMatchReport()
        {
            var myDao = dao ?? new Dao();
            return myDao.GetMatchReport(ID);
        }

        public void StartBallByBallCoverage(BallByBallMatchConditions ballByBallMatchConditions)
        {
            var myDao = dao ?? new Dao();
            if (!myDao.IsBallByBallCoverageInProgress(ID))
            {
                data.CaptainID = ballByBallMatchConditions.Captain;
                data.WicketKeeperID = ballByBallMatchConditions.Keeper;
                data.WonToss = ballByBallMatchConditions.WonToss;
                data.Batted = ballByBallMatchConditions.Batted;
                data.WasDeclarationGame = ballByBallMatchConditions.Declaration;
                data.Overs = ballByBallMatchConditions.Overs;

                myDao.StartBallByBallCoverage(ID, ballByBallMatchConditions.PlayerIds, data);
                var inningsStatus = BallByBallInningsStatus.NotStarted(ID);
                if (data.WonToss && data.Batted || !data.WonToss && !data.Batted)
                    inningsStatus.OurInningsStatus = InningsStatus.InProgress;
                else
                    inningsStatus.TheirInningsStatus = InningsStatus.InProgress;

                myDao.UpdateInningsStatus(inningsStatus);
            }
            else
            {
                throw new InvalidOperationException("Cannot start coverage for match " + Description +
                                                    " game is already in progress.");
            }
        }

        public BallByBallMatch GetCurrentBallByBallState()
        {
            return BallByBallMatch.Load(ID, this);
        }

        public void UpdateCurrentBallByBallState(MatchState stateFromClient)
        {
            if (stateFromClient.Over.Balls.Any(b => string.IsNullOrEmpty(b.Bowler)))
                throw new InvalidOperationException("Cannot add a ball with a blank bowler");

            var currentBallByBallState = GetCurrentBallByBallState();
            var inningsStatus = currentBallByBallState.GetInningsStatus();
            if (Overs <= currentBallByBallState.LastCompletedOver && !WasDeclaration)
                throw new InvalidOperationException("this match is only " + Overs +
                                                    " overs long and we've have that many. Time to poke the end innings button.");
            if (stateFromClient.Over.Balls.IsNullOrEmpty())
            {
                throw new InvalidOperationException("Can't add a new over with no balls in it, noone counts that badly");
            }
            var myDao = dao ?? new Dao();
            if (inningsStatus.OurInningsStatus != InningsStatus.InProgress)
            {
                inningsStatus.OurInningsStatus = InningsStatus.InProgress;
                myDao.UpdateInningsStatus(inningsStatus);
            }

            myDao.UpdateCurrentBallByBallState(stateFromClient, ID);
        }

        public static IEnumerable<Match> GetInProgressGames()
        {
            return GetInProgressGames(new Dao());
        }

        // Added overload for testing - allow injection of a DAO
         public static IEnumerable<Match> GetInProgressGames(IDao dao)
         {
            // inline BuildInProgressQuery: reuse central builder and apply in-progress filter
            return BuildMatchQuery(dao).Where(m => m.GetIsBallByBallInProgress() && !m.BallByBallComplete());
         }

        private bool BallByBallComplete()
        {
            return GetCurrentBallByBallState().IsMatchComplete();
        }

        public bool GetIsBallByBallInProgress()
        {
            var myDao = dao ?? new Dao();
            return myDao.IsBallByBallCoverageInProgress(ID);
        }

        public LiveScorecard GetLiveScorecard()
        {
            var currentBallByBallState = GetCurrentBallByBallState();
            var matchState = currentBallByBallState.GetMatchState();
            var inningsStatus = currentBallByBallState.GetInningsStatus();

            var liveScorecard = new LiveScorecard();
            liveScorecard.Opposition = Opposition.Name;
            liveScorecard.OurInningsStatus = inningsStatus.OurInningsStatus.ToString();
            liveScorecard.OurInningsCommentary = inningsStatus.OurInningsCommentary;
            liveScorecard.TheirInningsStatus = inningsStatus.TheirInningsStatus.ToString();
            liveScorecard.TheirInningsCommentary = inningsStatus.TheirInningsCommentary;
            liveScorecard.WonToss = WonToss;
            liveScorecard.TossWinnerBatted = TossWinnerBatted;
            liveScorecard.DeclarationGame = WasDeclaration;
            liveScorecard.Overs = Overs;
            liveScorecard.OversRemaining = OurInningsInProgress
                ? Overs - currentBallByBallState.LastCompletedOver
                : Overs - OppositionBallByBallOver;
            liveScorecard.IsFirstInnings = inningsStatus.OurInningsStatus == InningsStatus.NotStarted ||
                                           inningsStatus.TheirInningsStatus == InningsStatus.NotStarted;

            liveScorecard.IsMatchComplete = inningsStatus.OurInningsStatus == InningsStatus.Completed &&
                                            inningsStatus.TheirInningsStatus == InningsStatus.Completed;

            if (liveScorecard.IsMatchComplete)
            {
                liveScorecard.ResultText =
                    currentBallByBallState.GetResultText(TeamBattingFirst(), TeamBattingSecond());

                
                

            }
                

            if (liveScorecard.OurInningsStatus != InningsStatus.NotStarted.ToString() &&
                currentBallByBallState.Overs.Any())
            {
                liveScorecard.OnStrikeBatsman = currentBallByBallState.GetOnStrikeBatsmanDetails();
                liveScorecard.OtherBatsman = currentBallByBallState.GetOtherBatsmanDetails();
                liveScorecard.LastBatsmanOut = currentBallByBallState.GetLastBatsmanOutDetails();
                liveScorecard.OurLastCompletedOver = currentBallByBallState.LastCompletedOver;
                liveScorecard.Score = matchState.Score;
                liveScorecard.Wickets = matchState.Players.Count(p => p.State == PlayerState.Out);
                liveScorecard.RunRate = matchState.LastCompletedOver == 0
                    ? 0
                    : Math.Round((decimal) matchState.Score / matchState.LastCompletedOver, 2);

                var partnershipsAndFallOfWickets = currentBallByBallState.GetPartnershipsAndFallOfWickets();

                liveScorecard.CurrentPartnership =
                    partnershipsAndFallOfWickets.GetPartnershipData(liveScorecard.OnStrikeBatsman.PlayerId,
                        liveScorecard.OtherBatsman.PlayerId);

                var currentPartnershipIndex =
                    partnershipsAndFallOfWickets.Partnerships.IndexOf(liveScorecard.CurrentPartnership);

                try
                {
                    liveScorecard.PreviousPartnership = currentPartnershipIndex == 0
                        ? null
                        : partnershipsAndFallOfWickets.Partnerships[currentPartnershipIndex - 1];
                }
                catch (Exception)
                {
                    //
                }
                var fallOfWickets = partnershipsAndFallOfWickets.FallOfWickets;
                liveScorecard.LastManOut = fallOfWickets.Any() ? fallOfWickets.Last() : null;

                liveScorecard.FallOfWickets = fallOfWickets;
                liveScorecard.Partnerships = partnershipsAndFallOfWickets.Partnerships;

                liveScorecard.LiveBattingCard = GetLiveBattingCard(currentBallByBallState, fallOfWickets);
                


                liveScorecard.CompletedOvers = currentBallByBallState.GetOverSummaries();

                liveScorecard.BowlerOneDetails = currentBallByBallState.GetBowlerOneDetails();
                liveScorecard.BowlerTwoDetails = currentBallByBallState.GetBowlerTwoDetails();
                liveScorecard.LiveBowlingCard = GetLiveBowlingCard(currentBallByBallState);
            }

            if (liveScorecard.TheirInningsStatus != InningsStatus.NotStarted.ToString())
            {
                liveScorecard.TheirScore = currentBallByBallState.OppositionScore;
                liveScorecard.TheirWickets = currentBallByBallState.OppositionWickets;
                liveScorecard.TheirOver = currentBallByBallState.OppositionOver;
                liveScorecard.TheirRunRate = liveScorecard.TheirOver == 0
                    ? 0
                    : Math.Round(liveScorecard.TheirScore / (decimal) liveScorecard.TheirOver, 2);
                liveScorecard.TheirCompletedOvers = currentBallByBallState.OppositionOvers;
            }

            return liveScorecard;
        }

        private List<BowlerInningsDetails> GetLiveBowlingCard(BallByBallMatch currentBallByBallState)
        {
            var bowlerInningsDetailses = currentBallByBallState.Overs.SelectMany(o => o.Balls)
                .Select(b => b.Bowler)
                .Distinct()
                .Select(currentBallByBallState.GetBowlerDetails)
                .ToList();
            return bowlerInningsDetailses;
        }

        private static LiveBattingCard GetLiveBattingCard(BallByBallMatch currentBallByBallState,
            List<FallOfWicket> fallOfWickets)
         {
             var liveBattingCard = new LiveBattingCard();
             var liveBattingCardEntries =
                 currentBallByBallState.GetMatchState()
                     .Players.Where(ps => ps.State != PlayerState.Waiting)
                     .ToDictionary(playerState => playerState.Position.ToString(),
                         playerState => new LiveBattingCardEntry
                         {
                             BatsmanInningsDetails =
                                 currentBallByBallState.GetBatsmanInningsDetails(playerState.PlayerId),
                             Wicket =
                                 fallOfWickets.FirstOrDefault(f => f.OutGoingPlayerId == playerState.PlayerId)?
                                     .Wicket
                         });
             liveBattingCard.Players = liveBattingCardEntries;
             liveBattingCard.Extras = currentBallByBallState.GetExtras();
             return liveBattingCard;
         }


        public void UpdateOppositionScore(OppositionInningsDetails oppositionInningsDetails)
        {
            if (oppositionInningsDetails.Over > Overs)
                throw new InvalidOperationException(oppositionInningsDetails.Over +
                                                    " overs is more than the match conditions stipulate (" + Overs +
                                                    ")");

            if (oppositionInningsDetails.Wickets > 10)
                throw new InvalidOperationException("There can't be more than 10 wickets in an innings");

            var currentBallByBallState = GetCurrentBallByBallState();

            var oppositionOver = currentBallByBallState.OppositionOver;
            var oppositionScore = currentBallByBallState.OppositionScore;
            if (oppositionOver > oppositionInningsDetails.Over)
                throw new InvalidOperationException("You are trying to input over " + oppositionInningsDetails.Over +
                                                    " but over " + oppositionOver + " has already been entered");

            if (oppositionScore > oppositionInningsDetails.Score)
                throw new InvalidOperationException("They were " + oppositionScore + " after " + oppositionOver +
                                                    " overs, they can't be " + oppositionInningsDetails.Score +
                                                    " now.");

            var inningsStatus = currentBallByBallState.GetInningsStatus();
            if (inningsStatus.OurInningsStatus == InningsStatus.InProgress)
                throw new InvalidOperationException(
                    "Our innings is in progress, you can't add their scores until it's done.");

            var myDao = dao ?? new Dao();
            if (inningsStatus.TheirInningsStatus != InningsStatus.InProgress)
            {
                inningsStatus.TheirInningsStatus = InningsStatus.InProgress;
                myDao.UpdateInningsStatus(inningsStatus);
            }

            myDao.CreateOrUpdateOppositionInningsDetails(oppositionInningsDetails, ID);
        }


        public NextInnings EndInnings(InningsEndDetails inningsEndDetails)
        {
            var myDao = dao ?? new Dao();
            if (inningsEndDetails.InningsType.Equals("Batting", StringComparison.InvariantCultureIgnoreCase))
            {
                var currentBallByBallState = GetCurrentBallByBallState();
                var inningsStatus = currentBallByBallState.GetInningsStatus();

                if (inningsStatus.OurInningsStatus != InningsStatus.InProgress)
                    throw new InvalidOperationException("Can't finish an innings before it has started");

                if (inningsEndDetails.WasDeclared && !WasDeclaration)
                    throw new InvalidOperationException("Can't declare in a game that's not a declaration game");

                inningsStatus.OurInningsStatus = InningsStatus.Completed;
                inningsStatus.OurInningsWasDeclared = inningsEndDetails.WasDeclared;
                inningsStatus.OurInningsCommentary = inningsEndDetails.Commentary;
                if (inningsStatus.TheirInningsStatus == InningsStatus.NotStarted)
                    inningsStatus.TheirInningsStatus = InningsStatus.InProgress;

                myDao.UpdateInningsStatus(inningsStatus);
            }
            else
            {
                var currentBallByBallState = GetCurrentBallByBallState();
                var inningsStatus = currentBallByBallState.GetInningsStatus();

                if (inningsStatus.TheirInningsStatus == InningsStatus.Completed)
                    throw new InvalidOperationException("This innings has already ended.");

                if (inningsStatus.TheirInningsStatus != InningsStatus.InProgress)
                    throw new InvalidOperationException("Can't finish an innings before it has started");

                if (inningsEndDetails.WasDeclared && !WasDeclaration)
                    throw new InvalidOperationException("Can't declare in a game that's not a declaration game");

                inningsStatus.TheirInningsStatus = InningsStatus.Completed;
                inningsStatus.TheirInningsWasDeclared = inningsEndDetails.WasDeclared;
                inningsStatus.TheirInningsCommentary = inningsEndDetails.Commentary;
                if (inningsStatus.OurInningsStatus == InningsStatus.NotStarted)
                    inningsStatus.OurInningsStatus = InningsStatus.InProgress;

                myDao.UpdateInningsStatus(inningsStatus);
            }

            var status = myDao.GetInningsStatus(ID);
            if (status.OurInningsStatus == InningsStatus.Completed &&
                status.TheirInningsStatus == InningsStatus.Completed)
            {
                PopulateScorecardFromBallByBallData();

                return NextInnings.GameOver;
            }

            if (status.OurInningsStatus == InningsStatus.InProgress) return NextInnings.Batting;

                if (status.TheirInningsStatus == InningsStatus.InProgress) return NextInnings.Bowling;

                throw new ApplicationException(
                    "After the end of an innings one innings must be in progess or both complete.");
        }

        public void PopulateScorecardFromBallByBallData()
        {
            var liveScorecard = GetLiveScorecard();
            var ourBattingCard = new BattingCard(this.ID, ThemOrUs.Us, dao);
            ourBattingCard.ScorecardData.Clear();
            ourBattingCard.ScorecardData.AddRange(liveScorecard.LiveBattingCard.Players.Select(p => BattingCardLine.From(p, this)));
            var liveExtras = liveScorecard.LiveBattingCard.Extras;
            ourBattingCard.Save(BattingOrBowling.Batting);

            var extras = new Extras(ID, ThemOrUs.Them)
            {
                Byes = liveExtras.Byes,
                LegByes = liveExtras.LegByes,
                NoBalls = liveExtras.NoBalls,
                Penalty = liveExtras.Penalty,
                Wides = liveExtras.Wides
            };
            extras.Save();

            var playerIdToPosition = ourBattingCard.ScorecardData.ToDictionary(b => b.Batsman.Id, b => b.BattingAt);

            var fallOfWicketStats = new FoWStats(ID, ThemOrUs.Us);
            fallOfWicketStats.Data.Clear();
            fallOfWicketStats.Data.AddRange(liveScorecard.FallOfWickets.Select(f => FoWStatsLine.From(f, this, ThemOrUs.Us, playerIdToPosition)));
            fallOfWicketStats.Save();

            var theirBowlingStats = new BowlingStats(ID, ThemOrUs.Them, dao);
            theirBowlingStats.BowlingStatsData.Clear();
            theirBowlingStats.BowlingStatsData.AddRange(liveScorecard.LiveBowlingCard.Select(b => BowlingStatsLine.From(b, this)));
            theirBowlingStats.Save();

        }

        public void DeleteLastBallByBallOver()
        {
            var myDao = dao ?? new Dao();
            myDao.DeleteBallByBallOver(ID, GetCurrentBallByBallState().LastCompletedOver);
        }

        public void ResetBallByBallData()
        {
            var myDao = dao ?? new Dao();
            myDao.ResetBallByBallCoverage(ID);
        }

        public FoWStats GetOurFoWData()
        {
            return new FoWStats(ID, ThemOrUs.Us);
        }

        public FoWStats GetTheirFoWData()
        {
            return new FoWStats(ID, ThemOrUs.Them);
        }
    }
}















