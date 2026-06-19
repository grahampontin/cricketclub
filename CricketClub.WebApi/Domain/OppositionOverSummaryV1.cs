using System.Collections.Generic;

namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Per-over summary for the opposition's ball-by-ball innings.
    /// Mirrors <see cref="OverSummaryV1"/> but wraps an <see cref="OppositionOverV1"/>.
    /// </summary>
    public class OppositionOverSummaryV1
    {
        public OppositionOverV1 Over { get; set; }
        public int ScoreAtEndOfOver { get; set; }
        public int WicketsAtEndOfOver { get; set; }
        public int ScoreForThisOver { get; set; }
    }
}

