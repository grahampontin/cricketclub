using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using CricketClubDAL;
using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    public class BallByBallMatch
    {
        private readonly List<Over> overs;
        private readonly List<PlayerState> playerStates;
        private readonly int matchId;
        private readonly OppositionInnings oppositionInnings;
        private readonly BallByBallInningsStatus inningsStatus;
        private readonly Match match;
        // Opposition ball-by-ball data (only populated when TheirInningsIsBallByBall = true)
        private readonly List<OppositionOver> oppositionBallByBallOvers;
        private readonly List<OppositionBatterState> oppositionBatterStates;

        private BallByBallMatch(List<Over> overs, List<PlayerState> playerStates, int matchId,
            OppositionInnings oppositionInnings, BallByBallInningsStatus inningsStatus, Match match,
            List<OppositionOver> oppositionBallByBallOvers, List<OppositionBatterState> oppositionBatterStates)
        {
            this.overs = overs;
            this.playerStates = playerStates;
            this.matchId = matchId;
            this.oppositionInnings = oppositionInnings;
            this.inningsStatus = inningsStatus;
            this.match = match;
            this.oppositionBallByBallOvers = oppositionBallByBallOvers;
            this.oppositionBatterStates = oppositionBatterStates;
        }

        public int LastCompletedOver
        {
            get { return overs.Any() ? overs.Max(o => o.OverNumber) : 0; }
        }

        public List<Over> Overs => overs;

        public bool OppositionInningsComplete => inningsStatus.TheirInningsStatus == InningsStatus.Completed;
        public int OppositionOver => oppositionInnings.Details.Any() ? oppositionInnings.Details.Max(d => d.Over) : 0;
        public int OppositionScore => inningsStatus.TheirInningsIsBallByBall
            ? GetOppositionBallByBallScore()
            : (oppositionInnings.Details.Any() ? LastOppositionOver.Score : 0);

        public List<OppositionInningsDetails> OppositionOvers => oppositionInnings.Details;

        private OppositionInningsDetails LastOppositionOver
        {
            get { return oppositionInnings.Details.OrderBy(d => d.Over).LastOrDefault(); }
        }

        public int OppositionWickets => inningsStatus.TheirInningsIsBallByBall
            ? GetOppositionBallByBallWickets()
            : (oppositionInnings.Details.Any() ? LastOppositionOver.Wickets : 0);

        // ── Opposition ball-by-ball helpers ─────────────────────────────────────

        public bool TheirInningsIsBallByBall => inningsStatus.TheirInningsIsBallByBall;

        public List<OppositionOver> OppositionBallByBallOvers => oppositionBallByBallOvers;

        public List<OppositionBatterState> OppositionBatterStates => oppositionBatterStates;

        public int OppositionBallByBallLastCompletedOver =>
            oppositionBallByBallOvers.Any() ? oppositionBallByBallOvers.Max(o => o.OverNumber) : 0;

        public int GetOppositionBallByBallScore() =>
            oppositionBallByBallOvers.SelectMany(o => o.Balls).Sum(b => b.Amount);

        public int GetOppositionBallByBallWickets() =>
            oppositionBallByBallOvers.SelectMany(o => o.Balls).Count(b => b.Wicket != null);

        public OppositionBatterState GetOppositionOnStrikeBatter()
        {
            var battingStates = oppositionBatterStates
                .Where(s => s.State == OppositionBatterState.Batting)
                .ToList();
            if (!battingStates.Any()) return null;

            var allBalls = OppositionBallsSortedLastToFirst();
            var lastBall = allBalls.FirstOrDefault(b => battingStates.Any(s => s.BatsmanName == b.BatsmanName));
            if (lastBall == null) return battingStates.First();

            // After an over ends the striker rotates; we just return who faced the last ball
            return battingStates.FirstOrDefault(s => s.BatsmanName == lastBall.BatsmanName)
                   ?? battingStates.First();
        }

        public OppositionBatterState GetOppositionOtherBatter()
        {
            var onStrike = GetOppositionOnStrikeBatter();
            return oppositionBatterStates
                .FirstOrDefault(s => s.State == OppositionBatterState.Batting && s.BatsmanName != onStrike?.BatsmanName);
        }

        /// <summary>Returns scoring details for an individual opposition batter.</summary>
        public OppositionBatterScorecardLine GetOppositionBatterDetails(string batsmanName)
        {
            var allBalls = oppositionBallByBallOvers.SelectMany(o => o.Balls)
                .Where(b => b.BatsmanName == batsmanName).ToList();

            var score = allBalls.Sum(b => b.Amount);
            var ballsFaced = allBalls.Count(b => !b.IsWide);
            var fours = allBalls.Count(b => b.IsBoundary());
            var sixes = allBalls.Count(b => b.IsSix());
            var wicket = allBalls.Select(b => b.Wicket).FirstOrDefault(w => w?.BatsmanName == batsmanName);

            return new OppositionBatterScorecardLine
            {
                BatsmanName = batsmanName,
                Score = score,
                BallsFaced = ballsFaced,
                Fours = fours,
                Sixes = sixes,
                StrikeRate = ballsFaced == 0 ? 0 : Math.Round((decimal)score * 100 / ballsFaced, 2),
                Wicket = wicket
            };
        }

        /// <summary>Returns bowling figures for one of OUR players bowling in the opposition innings.</summary>
        public OppositionBowlerDetails GetOppositionInningsBowlerDetails(int bowlerPlayerId, string bowlerName)
        {
            var oversBowled = oppositionBallByBallOvers
                .Where(o => o.Balls.Any(b => b.BowlerPlayerId == bowlerPlayerId)).ToList();
            var allBalls = oversBowled.SelectMany(o => o.Balls).Where(b => b.BowlerPlayerId == bowlerPlayerId).ToList();

            return new OppositionBowlerDetails
            {
                PlayerId = bowlerPlayerId,
                PlayerName = bowlerName,
                Overs = oversBowled.Count,
                Maidens = oversBowled.Count(o => o.IsMaiden()),
                Runs = allBalls.Where(b => !b.IsFieldingExtra()).Sum(b => b.Amount),
                Wickets = allBalls.Count(b => b.IsBowlersWicket()),
                Wides = allBalls.Where(b => b.IsWide).Sum(b => b.Amount),
                NoBalls = allBalls.Count(b => b.IsNoBall),
                Economy = oversBowled.Count == 0
                    ? 0
                    : Math.Round((decimal)allBalls.Where(b => !b.IsFieldingExtra()).Sum(b => b.Amount) / oversBowled.Count, 2)
            };
        }

        private IEnumerable<OppositionBall> OppositionBallsSortedLastToFirst() =>
            oppositionBallByBallOvers
                .OrderByDescending(o => o.OverNumber)
                .SelectMany(o => ((IEnumerable<OppositionBall>)o.Balls).Reverse());

        public static BallByBallMatch Load(int matchId, Match match)
            => Load(matchId, match, new Dao());

        public static BallByBallMatch Load(int matchId, Match match, IDao dao)
        {
            var cacheKey = $"BallByBallState_{matchId}";
            var cache = InternalCache.GetInstance();
            if (cache.Get(cacheKey) is BallByBallMatch cached) return cached;

            var inningsStatus = dao.GetInningsStatus(matchId);

            List<OppositionOver> oppOvers;
            List<OppositionBatterState> oppStates;
            if (inningsStatus.TheirInningsIsBallByBall)
            {
                oppOvers = dao.GetOppositionBallByBallOvers(matchId);
                oppStates = dao.GetOppositionBatterStates(matchId);
            }
            else
            {
                oppOvers = new List<OppositionOver>();
                oppStates = new List<OppositionBatterState>();
            }

            var loaded = new BallByBallMatch(
                dao.GetAllBallsForMatch(matchId),
                dao.GetPlayerStates(matchId),
                matchId,
                dao.GetOppositionInnings(matchId),
                inningsStatus,
                match,
                oppOvers,
                oppStates);

            cache.Insert(cacheKey, loaded, TimeSpan.FromSeconds(30));
            return loaded;
        }

        /// <summary>Removes the cached <see cref="BallByBallMatch"/> for the given match so the next
        /// call to <see cref="Load"/> fetches fresh data from the database.</summary>
        public static void InvalidateCache(int matchId)
        {
            InternalCache.GetInstance().Remove($"BallByBallState_{matchId}");
        }

        public Dictionary<int, int> GetPlayerScores(HashSet<int> playerIds)
        {
            return BallByBallHelpers.GetPlayerScoresFromBalls(playerIds, overs.SelectMany(o => o.Balls));
        }

        public MatchState GetMatchState()
        {
            var sortedOversLastToFirst = overs.OrderBy(o=>o.OverNumber).Reverse().ToList();
            Partnership partnership = null;
            if (sortedOversLastToFirst.Any())
            {
                partnership = GetPartnershipsAndFallOfWickets().GetPartnershipData(GetOnStrikeBatsmanDetails().PlayerId, GetOtherBatsmanDetails().PlayerId);
            }


            var bowlers = overs.SelectMany(o=>o.Balls).Select(b=>b.Bowler).Distinct().ToArray();
            var opposition = match.Opposition;
            return new MatchState
            {
                Bowlers = bowlers.ToArray(),
                BowlerDetails = bowlers.Select(GetBowlerDetails).ToArray(),
                LastCompletedOver = !overs.Any() ? 0 : overs.Max(o=>o.OverNumber),
                MatchId = matchId,
                Score = GetScore(),
                RunRate = !overs.Any() ? 0 :GetScore()/(decimal)overs.Count(),
                Players = GetPlayerStates(),
                PreviousBowler = !overs.Any() ? null : sortedOversLastToFirst.Take(1).Single().Balls[0].Bowler,
                PreviousBowlerButOne = !overs.Any() || overs.Count < 2 ? null : sortedOversLastToFirst.Skip(1).Take(1).Single().Balls[0].Bowler,
                Partnership = new PartnershipStub()
                {
                    Runs = partnership?.Score ?? 0,
                    Balls = partnership?.BallCount ?? 0,
                    Sixes = partnership?.Balls.Count(b => b.IsSix()) ?? 0,
                    Fours = partnership?.Balls.Count(b => b.IsBoundary()) ?? 0
                },
                OnStrikeBatsmanId = !overs.Any() || !GetPlayersInState(PlayerState.Batting).Any() ? -1 : GatBastmanOnStrikeAfter(GetSortedBallsLastToFirst().First()),
                NextState = GetWhatsNext().ToString(),
                OppositionScore = OppositionScore,
                OppositionWickets = OppositionWickets,
                OppositionName = opposition.Name,
                OppositionShortName = CreateShortName(opposition.Name)
            };
        }

        private string CreateShortName(string oppositionName)
        {
            var strings = oppositionName.Split(' ');
            return strings.Select(s => s =="CC" ? "CC" : s.Substring(0, 1).ToUpper()).Take(3).Aggregate((s1, s2) => s1 + s2);
        }

        private NextState GetWhatsNext()
        {
            var ourInningsStatus = GetInningsStatus().OurInningsStatus;
            var theirInningsStatus = GetInningsStatus().TheirInningsStatus;

            if (ourInningsStatus == InningsStatus.InProgress)
            {
                if (overs.Count == match.Overs || GetPlayersInState(PlayerState.Out).Length == 10 || (theirInningsStatus==InningsStatus.Completed && GetScore() > OppositionScore))
                {
                    return NextState.EndOfBattingInnings;
                }

                return NextState.BattingOver;
            }

            if (theirInningsStatus == InningsStatus.InProgress)
            {
                if (inningsStatus.TheirInningsIsBallByBall)
                {
                    // Ball-by-ball opposition innings
                    var oppLastOver = oppositionBallByBallOvers.Any() ? oppositionBallByBallOvers.Max(o => o.OverNumber) : 0;
                    var oppWickets = GetOppositionBallByBallWickets();
                    var oppScore = GetOppositionBallByBallScore();
                    if (oppLastOver >= match.Overs || oppWickets >= 10 ||
                        (ourInningsStatus == InningsStatus.Completed && oppScore > GetScore()))
                    {
                        return NextState.EndOfBowlingInnings;
                    }
                    return NextState.OppositionBattingOver;
                }
                else
                {
                    // Summary (per-over) opposition innings
                    if (LastOppositionOver!=null && (LastOppositionOver.Over == match.Overs || oppositionInnings.Details.Last()?.Wickets>=10 || (ourInningsStatus==InningsStatus.Completed && OppositionScore > GetScore())))
                    {
                        return NextState.EndOfBowlingInnings;
                    }

                    return NextState.BowlingOver;
                }
            }

            if (theirInningsStatus == InningsStatus.NotStarted &&
                ourInningsStatus == InningsStatus.NotStarted)
            {
                if (this.playerStates.IsNullOrEmpty())
                {
                    return NextState.SelectTeam;
                }

                if (match.Overs == 0)
                {
                    return NextState.MatchConditions;
                }

                if (match.OppositionBattedFirst)
                {
                    return NextState.BowlingOver;
                }

                return NextState.BattingOver;
            }
             
            return NextState.EndOfMatch;

        }

        private int GatBastmanOnStrikeAfter(Ball ball)
        {
            var batsmanWhoFaced = ball.Batsman;
            if (ball.Wicket != null)
            {
                if (ball.Wicket.Player == batsmanWhoFaced)
                {
                    batsmanWhoFaced = GetBattingPlayers().AsEnumerable().OrderBy(s => s.Position).Last().PlayerId;
                }
            }
            if (BatsmenChangeEndsAfter(ball))
            {
                return batsmanWhoFaced;
            }

            return GetBattingPlayers().Item1.PlayerId == batsmanWhoFaced ? GetBattingPlayers().Item2.PlayerId : GetBattingPlayers().Item1.PlayerId;

        }

        private bool BatsmenChangeEndsAfter(Ball ball)
        {
            var shouldSwitch = ball.Amount % 2 != 0;
            switch (ball.Thing) {
                case Ball.Wides:
                case Ball.NoBall:
                    shouldSwitch = !shouldSwitch;
                    break;
            }
            return shouldSwitch;
        }

        private Tuple<PlayerState, PlayerState> GetBattingPlayers()
        {
            var battingPlayers = GetPlayersInState(PlayerState.Batting);
            var defaultPlayer = new PlayerState { PlayerId = -1 };

            if (battingPlayers.Length == 0)
            {
                return new Tuple<PlayerState, PlayerState>(defaultPlayer, defaultPlayer);
            }

            var secondPlayer = battingPlayers.Length > 1 ? battingPlayers[1] : new PlayerState { PlayerId = -1 };
            return new Tuple<PlayerState, PlayerState>(battingPlayers[0], secondPlayer);
        }

        private PlayerState[] GetPlayersInState(string state)
        {
            return playerStates.Where(p => p.AsOfOver == LastCompletedOver && p.State == state).ToArray();
        }

        private PlayerState[] GetPlayerStates()
        {
            var batsmenDetails = playerStates.Select(p => GetBatsmanInningsDetails(p.PlayerId)).ToDictionary(d=>d.PlayerId, d=>d);
            playerStates.ForEach(p=>
            {
                var detail = batsmenDetails.GetValueOrInitializeDefault(p.PlayerId, new BatsmanInningsDetails());
                p.CurrentScore = detail.Score;
                p.BallsFaced = detail.Balls;
                p.Fours = detail.Fours;
                p.Sixes = detail.Sixes;
                p.StrikeRate = detail.StrikeRate;
            });
            return playerStates.ToArray();
        }

        private int GetScore()
        {
            return GetScoreForBalls(overs.SelectMany(o => o.Balls));
        }

        private int GetScoreForBalls(IEnumerable<Ball> balls)
        {
            return balls.Sum(b => b.Amount);
        }

        public BatsmanInningsDetails GetOnStrikeBatsmanDetails()
        {
            return GetBatsmanInningsDetails(GetOnStrikeBatsman());
        }

        private int GetOnStrikeBatsman()
        {
            var battingPlayers = playerStates.Where(p => p.State == PlayerState.Batting).Select(p => p.PlayerId).ToList();
            var lastBall = GetSortedBallsLastToFirst().Where(b => battingPlayers.Contains(b.Batsman)).FirstOrDefault();
            if (lastBall == null)
            {
                return battingPlayers.First();
            }
            var onStrikeBatsman = lastBall.Batsman;
            return onStrikeBatsman;
        }

        public BatsmanInningsDetails GetBatsmanInningsDetails(int playerId)
        {
            if (!overs.Any())
            {
                return new BatsmanInningsDetails()
                {
                    PlayerId = playerId
                };
            }
            var lastBall = overs.Last().Balls.Last();
            var bowler = lastBall.Bowler;
            var allBalls = overs.SelectMany(o => o.Balls);

            var player = new Player(playerId);
            var allBallsFacedByThisBatsman = allBalls.Where(b => b.Batsman == playerId).ToList();

            var batsmanInningsDetails = new BatsmanInningsDetails();
            batsmanInningsDetails.PlayerId = playerId;
            batsmanInningsDetails.Name = player.Name;
            var playerScore = GetPlayerScores(new HashSet<int> {playerId})[playerId];
            batsmanInningsDetails.Score = playerScore;
            var legitimateBallsFaced = allBallsFacedByThisBatsman.Count(b => !b.IsNoBall && !b.IsWide);
            batsmanInningsDetails.Balls = legitimateBallsFaced;
            batsmanInningsDetails.Fours = allBallsFacedByThisBatsman.Count(b => b.Amount == 4);
            batsmanInningsDetails.Sixes = allBallsFacedByThisBatsman.Count(b => b.Amount == 6);
            batsmanInningsDetails.Dots = allBallsFacedByThisBatsman.Count(b => b.Amount == 0);
            batsmanInningsDetails.StrikeRate = legitimateBallsFaced == 0 ? 0 : Math.Round((decimal) playerScore*100/legitimateBallsFaced, 2);

            var ballsForThisBowler = allBallsFacedByThisBatsman.Where(b => b.Bowler == bowler).ToList();
            batsmanInningsDetails.ScoreForThisBowler = BallByBallHelpers.ScoreFromBalls(ballsForThisBowler);
            batsmanInningsDetails.BallsFacedFromThisBowler = ballsForThisBowler.Count;

            var ballsFacedInLastTenOvers =
                overs.Where(o => o.OverNumber > LastCompletedOver - 10).SelectMany(o => o.Balls.Where(b=>b.Batsman==playerId)).ToList();
            batsmanInningsDetails.ScoreForLastTenOvers = BallByBallHelpers.ScoreFromBalls(ballsFacedInLastTenOvers);
            batsmanInningsDetails.BallsFacedInLastTenOvers = ballsFacedInLastTenOvers.Count;

            batsmanInningsDetails.Matches = player.GetMatchesPlayed();
            batsmanInningsDetails.CareerRuns = player.GetRunsScored();
            batsmanInningsDetails.CareerAverage = player.GetBattingAverage();
            batsmanInningsDetails.CareerHighScore = player.GetHighScore();

            return batsmanInningsDetails;
        }

        public BatsmanInningsDetails GetOtherBatsmanDetails()
        {
            var otherBatsman = playerStates.FirstOrDefault(p => p.State == PlayerState.Batting && p.PlayerId!=GetOnStrikeBatsman());
            if (otherBatsman == null)
            {
                return new BatsmanInningsDetails();
            }
            return GetBatsmanInningsDetails(otherBatsman.PlayerId);

        }

        public BatsmanInningsDetails GetLastBatsmanOutDetails()
        {
            var outBatsmen = playerStates.Where(p => p.State == PlayerState.Out).ToList();
            if (!outBatsmen.Any())
            {
                return new BatsmanInningsDetails();
            }
            if (outBatsmen.Count == 1)
            {
                return GetBatsmanInningsDetails(outBatsmen.Single().PlayerId);
            }
            return GetBatsmanInningsDetails(GetLastBatsmanOut(outBatsmen));
        }

        private int GetLastBatsmanOut(IEnumerable<PlayerState> outBatsmen)
        {
            var outPlayerIds = new HashSet<int>(outBatsmen.Select(p => p.PlayerId));
            return GetSortedBallsLastToFirst().First(b => outPlayerIds.Contains(b.Batsman)).Batsman;
        }

        private IEnumerable<Ball> GetSortedBallsLastToFirst()
        {
            return overs.OrderByDescending(o => o.OverNumber).SelectMany(over => over.Balls.Reverse());
        }

        private IEnumerable<Ball> GetSortedBallsFirstToLast()
        {
            return overs.OrderBy(o => o.OverNumber).SelectMany(over => over.Balls);
        }

        public PartnershipsAndFallOfWickets GetPartnershipsAndFallOfWickets()
        {
            var partnerships = new List<Partnership>();
            var fallOfWickets = new List<FallOfWicket>();
            var openingPair = playerStates.Where(ps => ps.Position <= 2 && ps.Position >0).ToList();
            if (openingPair.Count != 2)
            {
                throw new Exception("There should always be 2 batsmen to open an innings", new Exception());
            }
            var balls = GetSortedBallsFirstToLast();
            var partnership = new Partnership(openingPair[0].PlayerId, openingPair[1].PlayerId);
            try
            {
                foreach (var ball in balls)
                {
                    partnership.Balls.Add(ball);
                    if (ball.Wicket != null)
                    {
                        partnerships.Add(partnership);

                        var ballsToThisPointInTime = partnerships.SelectMany(p => p.Balls).ToList();
                        var playerScores = BallByBallHelpers.GetPlayerScoresFromBalls(new HashSet<int>(partnership.PlayerIds), ballsToThisPointInTime);
                        var outGoingPlayer = ball.Wicket.Player;
                        var notOutPlayer = partnership.PlayerIds.Single(p => p != outGoingPlayer);
                        var fallOfWicket = new FallOfWicket(partnerships.Count,
                            GetScoreForBalls(ballsToThisPointInTime),
                            outGoingPlayer,
                            playerScores[outGoingPlayer],
                            notOutPlayer,
                            playerScores[notOutPlayer],
                            GetBatsmanInningsDetails(outGoingPlayer),
                            BallByBallHelpers.GetOversAsString(ballsToThisPointInTime),
                            partnership, ball.Wicket, ball.Bowler);

                        fallOfWickets.Add(
                            fallOfWicket);

                        var lastBatsman = playerStates.Where(
                            ps => ps.PlayerId == partnership.PlayerId1 || ps.PlayerId == partnership.PlayerId2)
                            .Max(ps => ps.Position);
                        if (lastBatsman == 11)
                        {
                            break;
                        }
                        var nextManInBattingAt = lastBatsman + 1;
                        var nextBatsman = playerStates.Single(ps => ps.Position == nextManInBattingAt).PlayerId;

                        partnership = new Partnership(notOutPlayer, nextBatsman);
                    }
                }
            }
            catch (Exception)
            {
                //
            }
            partnerships.Add(partnership);
            return new PartnershipsAndFallOfWickets(partnerships, fallOfWickets);
        }

        public List<OverSummary> GetOverSummaries()
        {
            var overSummaries = new List<OverSummary>();
            var oversToThisPointInTime = new List<Over>();
            foreach (var over in Overs.OrderBy(o=>o.OverNumber))
            {
                oversToThisPointInTime.Add(over);
                overSummaries.Add(new OverSummary(over, 
                    GetScoreForBalls(oversToThisPointInTime.SelectMany(o=>o.Balls)), 
                    oversToThisPointInTime.SelectMany(o=>o.Balls).Count(b=>b.Wicket!=null), 
                    GetScoreForBalls(over.Balls)));
            }
            return overSummaries;
        }

        public BowlerInningsDetails GetBowlerOneDetails()
        {
            var bowlerOne = GetBowlerOne();
            return GetBowlerDetails(bowlerOne);
        }
        public BowlerInningsDetails GetBowlerTwoDetails()
        {
            var bowlerTwo = GetBowlerTwo();
            if (string.IsNullOrEmpty(bowlerTwo))
            {
                return new BowlerInningsDetails();
            }
            return GetBowlerDetails(bowlerTwo);
        }

        private string GetBowlerTwo()
        {
            return GetSortedBallsLastToFirst().Select(b => b.Bowler).FirstOrDefault(b => b != GetBowlerOne());
        }

        public BowlerInningsDetails GetBowlerDetails(string bowlerName)
        {
            var bowlerInningsDetails = new BowlerInningsDetails();
            bowlerInningsDetails.Name = bowlerName;

            var oversBowledByThisBowler = overs.Where(o => o.Balls.Select(b => b.Bowler).Contains(bowlerName)).ToList();

            var bowlingDetails = GetBowlingInningsDetailsForOvers(oversBowledByThisBowler, bowlerName);

            bowlerInningsDetails.Details = bowlingDetails;

            var oversInThisSpell = GetOversInLastSpell(bowlerName);
            bowlerInningsDetails.JustThisSpell = GetBowlingInningsDetailsForOvers(oversInThisSpell, bowlerName);

            return bowlerInningsDetails;
        }

        private List<Over> GetOversInLastSpell(string bowlerOne)
        {
            var oversSinceThisBowlerLastBowled = 0;
            var spell = new List<Over>();
            foreach (var over in overs.Select(o=>o).Reverse())
            {
                if (over.WasBowledBy(bowlerOne))
                {
                    spell.Add(over);
                    oversSinceThisBowlerLastBowled = 0;
                }
                else
                {
                    oversSinceThisBowlerLastBowled ++;
                    if (!spell.Any())
                    {
                        continue;
                    }
                    if (oversSinceThisBowlerLastBowled > 1)
                    {
                        break;
                    }
                }
            }
            return spell;
        }

        private BowlingDetails GetBowlingInningsDetailsForOvers(List<Over> oversBowledByThisBowler, string bowlerOne)
        {
            var ballsForThisBowler = oversBowledByThisBowler.SelectMany(o => o.Balls).Where(b => b.Bowler == bowlerOne).ToList();

            var bowlingDetails = new BowlingDetails();
            
            bowlingDetails.Dots = ballsForThisBowler.Count(b => b.Amount == 0 || b.IsFieldingExtra());
            bowlingDetails.Fours = ballsForThisBowler.Count(b => b.Amount == 4 && !b.IsFieldingExtra());
            bowlingDetails.Sixes = ballsForThisBowler.Count(b => b.Amount == 6 && !b.IsFieldingExtra());
            bowlingDetails.Overs = oversBowledByThisBowler.Count;
            bowlingDetails.Maidens = oversBowledByThisBowler.Count(o => o.IsMaiden());
            bowlingDetails.Wides = ballsForThisBowler.Where(b => b.IsWide).Sum(b => b.Amount);
            bowlingDetails.NoBalls = ballsForThisBowler.Count(b => b.IsNoBall);
            bowlingDetails.Runs = ballsForThisBowler.Where(b => !b.IsFieldingExtra()).Sum(b => b.Amount);
            bowlingDetails.Wickets = ballsForThisBowler.Count(b => b.IsBowlersWicket());
            bowlingDetails.Economy = Math.Round(((decimal) bowlingDetails.Runs)/bowlingDetails.Overs,2);

            return bowlingDetails;
        }


        private string GetBowlerOne()
        {
            return GetSortedBallsLastToFirst().First().Bowler;
        }

        public LiveExtras GetExtras()
        {
            var liveExtras = new LiveExtras();
            foreach (var ball in overs.SelectMany(o=>o.Balls))
            {
                switch (ball.Thing)
                {
                    case Ball.Wides:
                        liveExtras.Wides += ball.Amount;
                        break;
                    case Ball.Byes:
                        liveExtras.Byes += ball.Amount;
                        break;
                    case Ball.LegByes:
                        liveExtras.LegByes += ball.Amount;
                        break;
                    case Ball.NoBall:
                        liveExtras.NoBalls += 1;
                        break;
                    case Ball.Penalty:
                        liveExtras.Penalty += ball.Amount;
                        break;
                }
            }
            return liveExtras;
        }

        public BallByBallInningsStatus GetInningsStatus()
        {
            return inningsStatus;
        }

        public string GetResultText(Team teamBattingFirst, Team teamBattingSecond)
        {
            var oppositionScore = OppositionScore;
            var ourScore = GetScore();
            if (ourScore == oppositionScore)
            {
                return " drew with ";
            }

            int teamBattingFirstScore;
            int teamBattingSecondScore;
            if (teamBattingFirst.IsUs)
            {
                teamBattingFirstScore = ourScore;
                teamBattingSecondScore = oppositionScore;
            }
            else
            {
                teamBattingFirstScore = oppositionScore;
                teamBattingSecondScore = ourScore;
            }

            if (teamBattingFirstScore > teamBattingSecondScore)
            {
                var margin = teamBattingFirstScore - teamBattingSecondScore;
                return teamBattingFirst.Name + " beat " + teamBattingSecond.Name + " by " + margin + " runs";
            }
            else
            {
                return teamBattingSecond.Name + " beat " + teamBattingFirst.Name + " by " + GetWicketsDown(teamBattingSecond) + " wickets";
            }

        }

        private int GetWicketsDown(Team teamBattingSecond)
        {
            if (teamBattingSecond.IsUs)
            {
                return GetMatchState().Players.Count(p => p.State == PlayerState.Out);
            }
            else
            {
                return OppositionWickets;
            }
        }

        public bool IsMatchComplete()
        {
            var status = GetInningsStatus();
            return status.TheirInningsStatus == InningsStatus.Completed &&
                   status.OurInningsStatus == InningsStatus.Completed;
        }
    }

   
}
