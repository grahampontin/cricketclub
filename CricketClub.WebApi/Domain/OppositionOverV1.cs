namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// An over in the opposition's ball-by-ball innings.
    /// </summary>
    public class OppositionOverV1
    {
        public int OverNumber { get; set; }
        public OppositionBallV1[] Balls { get; set; }
        public string Commentary { get; set; }
    }
}

