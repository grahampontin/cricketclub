namespace CricketClubDomain
{
    /// <summary>
    /// State of a single opposition batter during ball-by-ball coverage of their innings.
    /// Mirrors <see cref="PlayerState"/> but uses a string name instead of a player ID
    /// because opposition players have no records in the player table.
    /// </summary>
    public class OppositionBatterState
    {
        public string BatsmanName;
        public int Position;
        public string State;
        public int CurrentScore;
        public int Fours;
        public int BallsFaced;
        public int Sixes;
        public decimal StrikeRate;
        public int AsOfOver;

        public const string Batting = "Batting";
        public const string Waiting = "Waiting";
        public const string Out = "Out";

        public override string ToString() =>
            $"BatsmanName: {BatsmanName}, Position: {Position}, State: {State}, Score: {CurrentScore}";
    }
}

