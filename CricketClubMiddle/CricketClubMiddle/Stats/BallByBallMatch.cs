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
        private readonly IEnumerable<Over> overs;
        private readonly List<PlayerState> playerStates;
        private readonly int matchId;

        private BallByBallMatch(IEnumerable<Over> overs, List<PlayerState> playerStates, int matchId)
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
                LastCompletedOver = overs.Max(o=>o.OverNumber),
                MatchId = matchId,
                Score = GetScore(),
                RunRate = GetScore()/(decimal)overs.Count(),
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
    }
}
