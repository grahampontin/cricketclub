namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Request body for the abandon-match endpoint.
    /// </summary>
    public class AbandonMatchV1
    {
        /// <summary>
        /// Optional human-readable reason for the abandonment (e.g. "rain", "bad light").
        /// Stored as innings commentary.
        /// </summary>
        public string? Reason { get; set; }
    }
}

