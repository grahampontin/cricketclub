namespace CricketClub.WebApi.Domain
{
    public class PlayerStateV1
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int Position { get; set; }
        public string State { get; set; }
        public int CurrentScore { get; set; }
        public int Fours { get; set; }
        public int BallsFaced { get; set; }
        public int Sixes { get; set; }
        public decimal StrikeRate { get; set; }

        public const string Batting = "Batting";
        public const string Waiting = "Waiting";
        public const string Out = "Out";

        public int AsOfOver { get; set; }
    }
}