#nullable disable
namespace CricketClub.WebApi.Domain
{
    public class LeadingPlayerCategoryV1
    {
        public string Category { get; set; }
        public List<LeadingPlayerEntryV1> Players { get; set; }
    }

    public class LeadingPlayerEntryV1
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int Value { get; set; }
    }
}
