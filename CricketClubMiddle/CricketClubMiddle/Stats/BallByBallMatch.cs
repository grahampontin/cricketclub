using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CricketClubDAL;
using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    public class BallByBallMatch
    {
        private readonly IList<Over> overs;
        private readonly List<PlayerState> playerStates;
        private readonly int matchId;

        private BallByBallMatch(IList<Over> overs, List<PlayerState> playerStates, int matchId)
        {
            this.overs = overs;
            this.playerStates = playerStates;
            this.matchId = matchId;
        }

        public int LastCompletedOver
        {
            get { return overs.Any() ? overs.Max(o => o.OverNumber) : 0; }
        }

        public static BallByBallMatch Load(int matchId)
        {
            var dao = new Dao();
            return new BallByBallMatch(dao.GetAllBallsForMatch(matchId), dao.GetPlayerStates(matchId), matchId);
        }

        public Dictionary<int, int> GetPlayerScores(HashSet<int> playerIds)
        {
            IEnumerable<Ball> balls = overs.SelectMany(o => o.Balls);
            return balls.Where(b => playerIds.Contains(b.Batsman))
                .GroupBy(b => b.Batsman)
                .ToDictionary(g => g.Key, ScoreFromBalls);

        }

        private int ScoreFromBalls(IEnumerable<Ball> balls)
        {
            return balls.Aggregate(0, (score, ball) => score + RunsFromBall(ball));
        }

        private int RunsFromBall(Ball ball)
        {
            switch (ball.Thing)
            {
                case Ball.Runs:
                    return ball.Amount;
                case Ball.Byes:
                case Ball.LegByes:
                case Ball.Wides:
                case Ball.Penalty:
                    return 0;
                case Ball.NoBall:
                    return ball.Amount - 1;
                default:
                    return 0;
            }
        }

        public MatchState GetMatchState()
        {
            return new MatchState()
            {
                Bowlers = overs.SelectMany(o=>o.Balls).Select(b=>b.Bowler).Distinct().ToArray(),
                LastCompletedOver = !overs.Any() ? 0 : overs.Max(o=>o.OverNumber),
                MatchId = matchId,
                Score = GetScore(),
                RunRate = !overs.Any() ? 0 :GetScore()/(decimal)overs.Count(),
                Players = GetPlayerStates()
                
            };
        }

        private PlayerState[] GetPlayerStates()
        {
            var playerScores = GetPlayerScores(new HashSet<int>(playerStates.Select(p => p.PlayerId)));
            playerStates.ForEach(p=>p.CurrentScore=playerScores.GetValueOrInitializeDefault(p.PlayerId, 0));
            return playerStates.ToArray();
        }

        private int GetScore()
        {
            return overs.SelectMany(o => o.Balls).Sum(b => b.Amount);
        }

        public BatsmanInningsDetails GetOnStrikeBatsmanDetails()
        {
            return GetBatsmanInningsDetails(GetOnStrikeBatsman());
        }

        private int GetOnStrikeBatsman()
        {
            var lastBall = overs.Last().Balls.Last();
            var onStrikeBatsman = lastBall.Batsman;
            return onStrikeBatsman;
        }

        private BatsmanInningsDetails GetBatsmanInningsDetails(int playerId)
        {
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
            var ballsFaced = allBallsFacedByThisBatsman.Count;
            batsmanInningsDetails.Balls = ballsFaced;
            batsmanInningsDetails.Fours = allBallsFacedByThisBatsman.Count(b => b.Amount == 4);
            batsmanInningsDetails.Sixes = allBallsFacedByThisBatsman.Count(b => b.Amount == 6);
            batsmanInningsDetails.Dots = allBallsFacedByThisBatsman.Count(b => b.Amount == 0);
            batsmanInningsDetails.StrikeRate = ballsFaced == 0 ? 0 : Math.Round((decimal) playerScore*100/ballsFaced, 2);

            var ballsForThisBowler = allBallsFacedByThisBatsman.Where(b => b.Bowler == bowler).ToList();
            batsmanInningsDetails.ScoreForThisBowler = ScoreFromBalls(ballsForThisBowler);
            batsmanInningsDetails.BallsFacedFromThisBowler = ballsForThisBowler.Count;

            var ballsFacedInLastTenOvers =
                overs.Where(o => o.OverNumber > LastCompletedOver - 10).SelectMany(o => o.Balls.Where(b=>b.Batsman==playerId)).ToList();
            batsmanInningsDetails.ScoreForLastTenOvers = ScoreFromBalls(ballsFacedInLastTenOvers);
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

        public Partnership GetPartnership(int playerId1, int playerId2)
        {
            List<Partnership> partnerships = new List<Partnership>();
            List<FallOfWicket> fallOfWickets = new List<FallOfWicket>();
            playerStates.Where(ps => ps.Position <= 2); 
            var balls = GetSortedBallsFirstToLast();
            Partnership partnership = null;
            foreach (var ball in balls)
            {
                if (partnership == null)
                {
                    partnership = new Partnership();
                }
                if (ball.Wicket != null)
                {
                    ball.Wicket.
                }
            }
        }
    }
}
