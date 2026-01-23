namespace CricketClub.WebApi.Domain
{
    public class StatsQueryV1
    {
        public string category { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public string venue { get; set; }
        public List<string> matchTypes { get; set; }
    }
}
