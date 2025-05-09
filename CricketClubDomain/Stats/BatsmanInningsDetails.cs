namespace CricketClubDomain.Stats
{
    public class BatsmanInningsDetails
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
    }
}