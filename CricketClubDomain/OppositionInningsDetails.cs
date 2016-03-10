namespace CricketClubDomain
{
    public class OppositionInningsDetails
    {
        //Deserializer
        public OppositionInningsDetails()
        {
        }

        public OppositionInningsDetails(int over, int score, int wickets, string commentary)
        {
            Over = over;
            Score = score;
            Wickets = wickets;
            Commentary = commentary;
        }

        // Setters for deserializer
        // ReSharper disable MemberCanBePrivate.Global
        public int Over { get; set; }
        
        public int Score { get; set; }

        public int Wickets { get; set; }

        public string Commentary { get; set; }

        // ReSharper restore MemberCanBePrivate.Global

    }
}