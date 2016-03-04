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
        private readonly Dao dao = new Dao();
        private readonly MatchData data;
        private BattingCard ourBatting;
        private BowlingStats ourBowling;
        private BattingCard theirBatting;
        private BowlingStats theirBowling;

        public Match(int MatchID)
        {
            data = dao.GetMatchData(MatchID);
        }

        private Match(MatchData data)
        {
            this.data = data;
        }

        public int ID
        {
            get { return data.ID; }
        }

        public int VenueID
        {
            get { return data.VenueID; }
            set { data.VenueID = value; }
        }

        public Venue Venue
        {
            get
            {
                var v = new Venue(VenueID);
                return v;
            }
        }

        public string VenueName
        {
            get { return Venue.Name; }
        }

        public DateTime MatchDate
        {
            get { return data.Date; }
            set { data.Date = value; }
        }

        public string MatchDateString
        {
            get { return MatchDate.DayOfWeek.ToString().Substring(0, 3) + " " + MatchDate.ToLongDateString(); }
        }

        public MatchType Type
        {
            get { return (MatchType) data.MatchType; }
            set { data.MatchType = (int) value; }
        }

        public int OppositionID
        {
            get { return data.OppositionID; }
            set { data.OppositionID = value; }
        }

        public Team Opposition
        {
            get { return new Team(OppositionID); }
        }

        public Team Us
        {
            get { return new Team(0); }
        }

        public bool Abandoned
        {
            get { return data.Abandoned; }

            set { data.Abandoned = value; }
        }

        public Team TossWinner
        {
            get
            {
                if (data.WonToss)
                    return new Team(0);
                return new Team(OppositionID);
            }
        }

        /// <summary>
        /// Did the special case team (The Village) win the toss - try not to use - prefer TossWinner
        /// </summary>
        public bool WonToss
        {
            get { return data.WonToss; }
            set { data.WonToss = value; }
        }

        /// <summary>
        /// Did the side that won the toss choose to Bat?
        /// </summary>
        public bool TossWinnerBatted
        {
            get { return data.Batted; }
            set { data.Batted = value; }
        }

        /// <summary>
        /// What did the toss winner do? returns "bat" or "field"
        /// </summary>
        public string TossWinnerElectedTo
        {
            get
            {
                if (TossWinnerBatted)
                {
                    return "bat";
                }
                return "field";
            }
        }

        public HomeOrAway HomeOrAway
        {
            get
            {
                if (data.HomeOrAway.ToUpper() == "H")
                {
                    return HomeOrAway.Home;
                }
                return HomeOrAway.Away;
            }

            set { data.HomeOrAway = value.ToString().Substring(0, 1); }
        }

        public Team HomeTeam
        {
            get
            {
                if (HomeOrAway == HomeOrAway.Home)
                {
                    return Us;
                }
                return Opposition;
            }
        }

        public Team AwayTeam
        {
            get
            {
                if (HomeOrAway == HomeOrAway.Away)
                {
                    return Us;
                }
                return Opposition;
            }
        }

        public string HomeTeamName
        {
            get { return HomeTeam.Name; }
        }

        public string AwayTeamName
        {
            get { return AwayTeam.Name; }
        }

        public string HomeTeamScore
        {
            get
            {
                string result = GetTeamScore(HomeTeam) + " for " + GetTeamWicketsDown(HomeTeam);
                result = result.Replace("for 10", "all out");
                if (HomeOrAway == HomeOrAway.Home && WeDeclared)
                {
                    result = result + " dec";
                }
                if (HomeOrAway == HomeOrAway.Away && TheyDeclared)
                {
                    result = result + " dec";
                }

                return result;
            }
        }

        public string AwayTeamScore
        {
            get
            {
                string result = GetTeamScore(AwayTeam) + " for " + GetTeamWicketsDown(AwayTeam);
                result = result.Replace("for 10", "all out");

                if (HomeOrAway == HomeOrAway.Away && WeDeclared)
                {
                    result = result + " dec";
                }
                if (HomeOrAway == HomeOrAway.Home && TheyDeclared)
                {
                    result = result + " dec";
                }
                return result;
            }
        }

        public bool ResultDrawn
        {
            get
            {
                if (Type == MatchType.Declaration)
                {
                    if ((GetTeamScore(TeamBattingSecond()) < GetTeamScore(TeamBattingFirst())) &&
                        GetTeamWicketsDown(TeamBattingSecond()) < 10)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool ResultTied
        {
            get
            {
                if (GetTeamScore(Us) == GetTeamScore(Opposition) && GetTeamScore(Us) > 0 && GetTeamScore(Opposition) > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public Team Winner
        {
            get
            {
                if (GetTeamScore(Us) > GetTeamScore(Opposition) && !ResultDrawn)
                {
                    return Us;
                }
                if (GetTeamScore(Us) < GetTeamScore(Opposition) && !ResultDrawn)
                {
                    return Opposition;
                }
                return null;
            }
        }

        public Team Loser
        {
            get
            {
                if (GetTeamScore(Us) < GetTeamScore(Opposition) && !ResultDrawn)
                {
                    return Us;
                }
                if (GetTeamScore(Us) > GetTeamScore(Opposition) && !ResultDrawn)
                {
                    return Opposition;
                }
                return null;
            }
        }

        public string ResultText
        {
            get
            {
                if (Abandoned)
                {
                    return "abandoned";
                }
                if (Winner != null && Winner.ID == Us.ID)
                {
                    if (HomeOrAway == HomeOrAway.Home)
                    {
                        return "beat";
                    }
                    return "lost to";
                }
                if (Winner != null && Winner.ID == Opposition.ID)
                {
                    switch (HomeOrAway)
                    {
                        case HomeOrAway.Away:
                            return "beat";
                        default:
                            return "lost to";
                    }
                }
                if (ResultDrawn)
                {
                    return "drew with";
                }
                if (ResultTied)
                {
                    return "tied with";
                }
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
                        int margin = GetTeamScore(Winner) - GetTeamScore(Loser);
                        string resultText = "by " + margin + " runs";
                        if (margin == 1)
                        {
                            resultText = resultText.Substring(0, resultText.Length - 1);
                        }
                        return resultText;
                    }
                    else
                    {
                        int margin = 10 - GetTeamWicketsDown(Winner);
                        string resultText = "by " + margin + " wickets";
                        if (margin == 1)
                        {
                            resultText = resultText.Substring(0, resultText.Length - 1);
                        }
                        return resultText;
                    }
                }
                else
                {
                    if (!Abandoned && !ResultDrawn)
                    {
                        return "result not yet in";
                    }
                    if (ResultDrawn)
                    {
                        return "";
                    }
                    return "no result";
                }
            }
        }

        public Player Captain
        {
            get { return new Player(data.CaptainID); }
            set { data.CaptainID = value.ID; }
        }

        public Player WicketKeeper
        {
            get { return new Player(data.WicketKeeperID); }
            set { data.WicketKeeperID = value.ID; }
        }

        public int Overs
        {
            get { return data.Overs; }
            set { data.Overs = value; }
        }

        public bool WasDeclaration
        {
            get { return data.WasDeclarationGame; }
            set { data.WasDeclarationGame = value; }
        }

        public bool WeDeclared
        {
            get { return data.WeDeclared; }
            set { data.WeDeclared = value; }
        }

        public bool TheyDeclared
        {
            get { return data.TheyDeclared; }
            set { data.TheyDeclared = value; }
        }

        public double OurInningsLength
        {
            get { return data.OurInningsLength; }
            set { data.OurInningsLength = value; }
        }

        public double TheirInningsLength
        {
            get { return data.TheirInningsLength; }
            set { data.TheirInningsLength = value; }
        }

        public static Match CreateNewMatch(Team opposition, DateTime matchDay, Venue venue, MatchType type,
                                           HomeOrAway homeAway)
        {
            var myDao = new Dao();
            int id = myDao.CreateNewMatch(opposition.ID, matchDay, venue.ID, (int) type,
                                          homeAway);
            return new Match(id);
        }

        public static Match GetNextMatch()
        {
            var myDao = new Dao();
            int matchID = myDao.GetNextMatch(DateTime.Today);
            if (matchID >= 0)
            {
                return new Match(matchID);
            }
            else
            {
                return null;
            }
        }

        public static Match GetLastMatch()
        {
            var myDao = new Dao();
            int matchID = myDao.GetPreviousMatch(DateTime.Today);
            if (matchID >= 0)
            {
                return new Match(matchID);
            }
            return null;
        }

        public Match GetPreviousMatch()
        {
            var myDao = new Dao();
            int matchID = myDao.GetPreviousMatch(MatchDate);
            if (matchID >= 0)
            {
                return new Match(matchID);
            }
            else
            {
                return null;
            }
        }

        public static IList<Match> GetFixtures()
        {
            return (GetAll().Where(a => a.MatchDate > DateTime.Today)).OrderBy(a => a.MatchDate).ToList();
        }

        public static IList<Match> GetResults()
        {
            return (GetAll().Where(a => a.MatchDate < DateTime.Today)).OrderBy(a => a.MatchDate).ToList();
        }

        public static IList<Match> GetResults(DateTime startDate, DateTime endDate)
        {
            return (GetResults().Where(a => a.MatchDate > startDate && a.MatchDate < endDate)).ToList();
        }

        private static IList<Match> GetAll()
        {
            var dao = new Dao();
            List<MatchData> data = dao.GetAllMatches();
            return (from a in data select new Match(a)).ToList();
        }

        public void Save()
        {
            ClearCache();
            if (data.ID != 0)
            {
                var dao = new Dao();
                dao.UpdateMatch(data);
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
                {
                    teamBattingFirst = Opposition;
                }
                else
                {
                    teamBattingFirst = new Team(0);
                }
            }
            return teamBattingFirst;
        }

        public Team TeamBattingSecond()
        {
            if (TeamBattingFirst().ID == 0)
            {
                return Opposition;
            }
            return new Team(0);
        }

        public BattingCard GetOurBattingScoreCard()
        {
            if (ourBatting == null)
            {
                ourBatting = new BattingCard(ID, ThemOrUs.Us);
            }
            return ourBatting;
        }

        public BattingCard GetTheirBattingScoreCard()
        {
            if (theirBatting == null)
            {
                theirBatting = new BattingCard(ID, ThemOrUs.Them);
            }
            return theirBatting;
        }

        public BowlingStats GetOurBowlingStats()
        {
            if (ourBowling == null)
            {
                ourBowling = new BowlingStats(ID, ThemOrUs.Us);
            }
            return ourBowling;
        }

        public BowlingStats GetThierBowlingStats()
        {
            if (theirBowling == null)
            {
                theirBowling = new BowlingStats(ID, ThemOrUs.Them);
            }
            return theirBowling;
        }


        /// <summary>
        /// Get the score for the home or away team
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public int GetTeamScore(Team team)
        {
            BattingCard sc = GetScoreCardForTeam(team);

            int score = sc.ScorecardData.Select(a => a.Score).Sum();
            score = score + sc.Extras;
            return score;
        }

        public int GetTeamWicketsDown(Team team)
        {
            BattingCard sc = GetScoreCardForTeam(team);

            int wickets = (sc.ScorecardData.Where(a => a.Dismissal != ModesOfDismissal.NotOut).Where(
                a => a.Dismissal != ModesOfDismissal.DidNotBat).Where(a => a.Dismissal != ModesOfDismissal.RetiredHurt).
                Where(a => a.BattingAt != 12)).Count();

            return wickets;
        }

        private BattingCard GetScoreCardForTeam(Team team)
        {
            BattingCard sc;
            if (team.ID == Us.ID)
            {
                sc = GetOurBattingScoreCard();
            }
            else if (team.ID == Opposition.ID)
            {
                sc = GetTheirBattingScoreCard();
            }
            else
            {
                throw new Exception("The team you specified didn't play in this game");
            }
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

        public void StartBallByBallCoverage(BallByBallMatchConditions ballByBallMatchConditions)
        {
            if (!dao.IsBallByBallCoverageInProgress(ID))
            {
                data.CaptainID = ballByBallMatchConditions.Captain;
                data.WicketKeeperID = ballByBallMatchConditions.Keeper;
                data.WonToss = ballByBallMatchConditions.WonToss;
                data.Batted = ballByBallMatchConditions.Batted;
                data.WasDeclarationGame = ballByBallMatchConditions.Declaration;
                data.Overs = ballByBallMatchConditions.Overs;

                dao.StartBallByBallCoverage(ID, ballByBallMatchConditions.PlayerIds, this.data);    
            }
            else
            {
                throw new InvalidOperationException("Cannot start coverage for match " + Description + " game is already in progress.");
            }
            
        }

        public string Description
        {
            get { return HomeTeam + " vs " + AwayTeam + " (" + MatchDateString + ")"; }
        }

        public BallByBallMatch GetCurrentBallByBallState()
        {
            return BallByBallMatch.Load(ID);
        }

        public void UpdateCurrentBallByBallState(MatchState stateFromClient)
        {
            if (stateFromClient.Over.Balls.Any(b => string.IsNullOrEmpty(b.Bowler)))
            {
                throw new InvalidOperationException("Cannot add a ball with a blank bowler");
            }
            dao.UpdateCurrentBallByBallState(stateFromClient, ID);
        }

        public static IEnumerable<Match> GetInProgressGames()
        {
            return GetAll().Where(m => m.GetIsBallByBallInProgress());
        }

        public bool GetIsBallByBallInProgress()
        {
            return dao.IsBallByBallCoverageInProgress(ID);
        }

        // ReSharper disable once UnusedMember.Global
        public int BallByBallOver => GetCurrentBallByBallState().LastCompletedOver;

        public bool OppositionInningsComplete
        {
            get
            {
                return GetCurrentBallByBallState().OppositionInningsComplete;
            }
        }

        public bool OppositionBattedFirst
        {
            get { return (TossWinner == Us && !TossWinnerBatted) || (TossWinner != Us && TossWinnerBatted); } 
        }

        public bool OurInningsComplete
        {
            get
            {
                var currentBallByBallState = GetCurrentBallByBallState();
                return currentBallByBallState.LastCompletedOver == Overs || currentBallByBallState.GetMatchState().Players.Count(p => p.State == PlayerState.Out) == 10;
            }
        }

        public int OppositionBallByBallOver => GetCurrentBallByBallState().OppositionOver;

        public LiveScorecard GetLiveScorecard()
        {
            var currentBallByBallState = GetCurrentBallByBallState();
            var liveScorecard = new LiveScorecard();
            liveScorecard.WonToss = WonToss;
            liveScorecard.TossWinnerBatted = TossWinnerBatted;
            liveScorecard.Overs = Overs;
            liveScorecard.OnStrikeBatsman = currentBallByBallState.GetOnStrikeBatsmanDetails();
            liveScorecard.OtherBatsman = currentBallByBallState.GetOtherBatsmanDetails();
            liveScorecard.LastBatsmanOut = currentBallByBallState.GetLastBatsmanOutDetails();
            liveScorecard.Opposition = Opposition.Name;
            liveScorecard.LastCompletedOver = currentBallByBallState.LastCompletedOver;
            liveScorecard.OversRemaining = WasDeclaration ? 0 : Overs - currentBallByBallState.LastCompletedOver;
            liveScorecard.DeclarationGame = WasDeclaration;
            var matchState = currentBallByBallState.GetMatchState();
            liveScorecard.Score = matchState.Score;
            liveScorecard.Wickets = matchState.Players.Count(p => p.State==PlayerState.Out);
            liveScorecard.RunRate = matchState.LastCompletedOver == 0
                ? 0
                : Math.Round((decimal) matchState.Score/matchState.LastCompletedOver, 2);

            var partnershipsAndFallOfWickets = currentBallByBallState.GetPartnershipsAndFallOfWickets();

            liveScorecard.CurrentPartnership =
                partnershipsAndFallOfWickets.GetPartnershipData(liveScorecard.OnStrikeBatsman.PlayerId,
                    liveScorecard.OtherBatsman.PlayerId);

            var currentPartnershipIndex = partnershipsAndFallOfWickets.Partnerships.IndexOf(liveScorecard.CurrentPartnership);

            liveScorecard.PreviousPartnership = currentPartnershipIndex == 0 ? null : partnershipsAndFallOfWickets.Partnerships[currentPartnershipIndex - 1];
            var fallOfWickets = partnershipsAndFallOfWickets.FallOfWickets;
            liveScorecard.LastManOut = fallOfWickets.Any()? fallOfWickets.Last() : null;

            liveScorecard.FallOfWickets = fallOfWickets;

            liveScorecard.CompletedOvers = currentBallByBallState.GetOverSummaries();

            liveScorecard.BowlerOneDetails = currentBallByBallState.GetBowlerOneDetails();
            liveScorecard.BowlerTwoDetails = currentBallByBallState.GetBowlerTwoDetails();

            var liveBattingCard = new LiveBattingCard();
            var liveBattingCardEntries = currentBallByBallState.GetMatchState().Players.Where(ps=>ps.State!=PlayerState.Waiting).ToDictionary(playerState => playerState.Position.ToString(), playerState => new LiveBattingCardEntry
            {
                BatsmanInningsDetails = currentBallByBallState.GetBatsmanInningsDetails(playerState.PlayerId), Wicket = fallOfWickets.FirstOrDefault(f => f.OutGoingPlayerId == playerState.PlayerId)?.Wicket
            });
            liveBattingCard.Players = liveBattingCardEntries;
            liveBattingCard.Extras = currentBallByBallState.GetExtras();
            liveScorecard.LiveBattingCard = liveBattingCard;

            return liveScorecard;
        }

        public void UpdateOppositionScore(OppositionInningsDetails oppositionInningsDetails)
        {
            if (oppositionInningsDetails.Over > Overs)
            {
                throw new InvalidOperationException(oppositionInningsDetails.Over + " overs is more than the match conditions stipulate ("+Overs+")");
            }
            if (oppositionInningsDetails.Over == Overs && !oppositionInningsDetails.EndOfInnings)
            {
                throw new InvalidOperationException(oppositionInningsDetails.Over + " overs should be the end of the innings according to the match conditions.");
            }
            if (oppositionInningsDetails.Wickets > 10)
            {
                throw new InvalidOperationException("There can't be more than 10 wickets in an innings");
            }
            if (oppositionInningsDetails.Wickets == 10 && !oppositionInningsDetails.EndOfInnings)
            {
                throw new InvalidOperationException("10 wickets should signify the end of the innings don't you think?");
            }
            var currentBallByBallState = GetCurrentBallByBallState();
            var oppositionOver = currentBallByBallState.OppositionOver;
            var oppositionScore = currentBallByBallState.OppositionScore;
            if (oppositionOver > oppositionInningsDetails.Over)
            {
                throw new InvalidOperationException("You are trying to input over " + oppositionInningsDetails.Over + " but over " + oppositionOver + " has already been entered");
            }
            if (oppositionScore > oppositionInningsDetails.Score)
            {
                throw new InvalidOperationException("They were " + oppositionScore + " after " + oppositionOver + " overs, they can't be " + oppositionInningsDetails.Score + " now.");
            }
            dao.CreateOrUpdateOppositionInningsDetails(oppositionInningsDetails, ID);
        }
    }
}
