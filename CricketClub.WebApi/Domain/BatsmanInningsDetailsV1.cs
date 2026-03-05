using CricketClubMiddle.Stats;

namespace CricketClub.WebApi.Domain
{
    public class BatsmanInningsDetailsV1
    {
        public int CareerHighScore { get; set; }
        public decimal CareerAverage { get; set; }
        public int CareerRuns { get; set; }
        public int Matches { get; set; }
        public int BallsFacedInLastTenOvers { get; set; }
        public int ScoreForLastTenOvers { get; set; }
        public int BallsFacedFromThisBowler { get; set; }
        public int ScoreForThisBowler { get; set; }
        public decimal StrikeRate { get; set; }
        public int Dots { get; set; }
        public int Sixes { get; set; }
        public int Fours { get; set; }
        public int Balls { get; set; }
        public int Score { get; set; }
        public string Name { get; set; }
        public int PlayerId { get; set; }

        public static BatsmanInningsDetailsV1 FromInternal(BatsmanInningsDetails details)
        {
            if (details == null) return null;
            return new BatsmanInningsDetailsV1
            {
                CareerHighScore = details.CareerHighScore,
                CareerAverage = details.CareerAverage,
                CareerRuns = details.CareerRuns,
                Matches = details.Matches,
                BallsFacedInLastTenOvers = details.BallsFacedInLastTenOvers,
                ScoreForLastTenOvers = details.ScoreForLastTenOvers,
                BallsFacedFromThisBowler = details.BallsFacedFromThisBowler,
                ScoreForThisBowler = details.ScoreForThisBowler,
                StrikeRate = details.StrikeRate,
                Dots = details.Dots,
                Sixes = details.Sixes,
                Fours = details.Fours,
                Balls = details.Balls,
                Score = details.Score,
                Name = details.Name,
                PlayerId = details.PlayerId
            };
        }
    }
}
