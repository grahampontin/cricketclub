namespace CricketClub.WebApi.Domain
{
    public class OverSummaryV1
    {
        public OverV1 Over { get; set; }
        public int ScoreAtEndOfOver { get; set; }
        public int WicketsAtEndOfOver { get; set; }
        public int ScoreForThisOver { get; set; }
    }
}
