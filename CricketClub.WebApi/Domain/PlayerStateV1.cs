namespace CricketClub.WebApi.Domain
{
    public class PlayerStateV1
    {
        public int PlayerId;
        public string PlayerName;
        public int Position;
        public string State;
        public int CurrentScore;
        public int Fours;
        public int BallsFaced;
        public int Sixes;
        public Decimal StrikeRate;
        public const string Batting = "Batting";
        public const string Waiting = "Waiting";
        public const string Out = "Out";
        public int AsOfOver;
    }
}