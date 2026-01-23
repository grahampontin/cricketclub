namespace CricketClub.WebApi.Domain
{
    public class BallV1
    {
        public int BallNumber { get; set; }
        public int Amount { get; set; }
        public int Batsman { get; set; }
        public string BatsmanName { get; set; }
        public string Bowler { get; set; }
        public string Thing { get; set; }
        public WicketV1 Wicket { get; set; }
        public decimal? Angle { get; set; }
        public int MatchId { get; set; }
        public int OverNumber { get; set; }
        public bool IsWide { get; set; }
        public bool IsNoBall { get; set; }
        public bool IsBoundary { get; set; }
        public bool IsSix { get; set; }
        public bool IsBowlersWicket { get; set; }
        public bool IsFieldingExtra { get; set; }
    }
}