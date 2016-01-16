using System.Collections.Generic;
using System.Linq;
using CricketClubDomain;

static internal class BallByBallHelpers
{
    public static Dictionary<int, int> GetPlayerScoresFromBalls(HashSet<int> playerIds, IEnumerable<Ball> balls)
    {
        var playerScoresFromBalls = balls.Where(b => playerIds.Contains(b.Batsman))
            .GroupBy(b => b.Batsman)
            .ToDictionary(g => g.Key, ScoreFromBalls);
        foreach (var playerId in playerIds.Where(playerId => !playerScoresFromBalls.ContainsKey(playerId)))
        {
            playerScoresFromBalls.Add(playerId, 0);
        }
        return playerScoresFromBalls;
    }

    public static int ScoreFromBalls(IEnumerable<Ball> balls)
    {
        return balls.Aggregate(0, (score, ball) => score + RunsFromBall(ball));
    }

    public static int RunsFromBall(Ball ball)
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

    public static string GetOversAsString(IList<Ball> balls)
    {
        var ballCountExcludingExtras = GetBallCountExcludingExtras(balls);
        var wholeOvers = (int) (ballCountExcludingExtras/6);
        var extraBalls = ballCountExcludingExtras%6;
        return wholeOvers + "." + extraBalls;
    }

    public static decimal GetBallCountExcludingExtras(IList<Ball> balls)
    {
        return balls.Count(b => b.Thing != Ball.NoBall && b.Thing != Ball.Wides);
    }
}