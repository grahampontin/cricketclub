namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Identifies a VCC player who has not yet come to the crease in the current innings.
    /// </summary>
    public class YetToBatEntryV1
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
    }
}

