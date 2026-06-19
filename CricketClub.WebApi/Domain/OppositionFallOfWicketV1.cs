namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// A fall-of-wicket entry for the opposition's ball-by-ball innings.
    /// Mirrors <see cref="FallOfWicketV1"/> but uses batter names rather than player IDs.
    /// </summary>
    public class OppositionFallOfWicketV1
    {
        public int WicketNumber { get; set; }
        public int TeamScore { get; set; }
        public string OverAsString { get; set; }
        public int BowlerPlayerId { get; set; }
        public string BowlerName { get; set; }
        public string OutgoingBatsmanName { get; set; }
        public int OutgoingBatsmanScore { get; set; }
        public string NotOutBatsmanName { get; set; }
        public int NotOutBatsmanScore { get; set; }
        public OppositionWicketV1 Wicket { get; set; }
        public OppositionPartnershipV1 Partnership { get; set; }
    }
}

