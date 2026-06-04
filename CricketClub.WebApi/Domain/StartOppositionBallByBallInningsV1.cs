namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Request body for POST /start-opposition-ball-by-ball.
    /// Provides the opposition batting lineup (11 string names) to enable ball-by-ball scoring.
    /// </summary>
    public class StartOppositionBallByBallInningsV1
    {
        /// <summary>Opposition batsmen in batting order (positions 1–11).</summary>
        public string[] BatsmanNames { get; set; }
    }
}

