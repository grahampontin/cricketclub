namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Full venue detail including match history and batting-difficulty rating.
    /// </summary>
    public class VenueDetailV1
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapUrl { get; set; }
        /// <summary>Batting-friendliness stats for this venue.</summary>
        public VenueStatsV1 Stats { get; set; } = new();
        /// <summary>Past results at this venue, ordered most-recent first.</summary>
        public List<ResultV1> Matches { get; set; } = new();
    }
}
