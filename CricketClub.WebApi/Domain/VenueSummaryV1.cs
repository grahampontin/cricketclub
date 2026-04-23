namespace CricketClub.WebApi.Domain
{
    public class VenueSummaryV1
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapUrl { get; set; }
        public VenueStatsV1 Stats { get; set; } = new();
    }
}
