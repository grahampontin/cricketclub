namespace CricketClub.WebApi.Domain
{
    public class OverV1
    {
        public int OverNumber { get; set; }
        public string Bowler { get; set; }
        public int RunsConceded { get; set; }
        public int WicketsTaken { get; set; }
        public BallV1[] Balls { get; set; }
    }
}