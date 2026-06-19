using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    /// <summary>
    /// Per-over summary for an opposition ball-by-ball innings over.
    /// Mirrors <see cref="CricketClubDomain.OverSummary"/> but wraps an <see cref="OppositionOver"/>.
    /// </summary>
    public class OppositionOverSummary
    {
        public OppositionOverSummary(OppositionOver over, int scoreAtEndOfOver, int wicketsAtEndOfOver, int scoreForThisOver)
        {
            Over = over;
            ScoreAtEndOfOver = scoreAtEndOfOver;
            WicketsAtEndOfOver = wicketsAtEndOfOver;
            ScoreForThisOver = scoreForThisOver;
        }

        public OppositionOver Over { get; }
        public int ScoreAtEndOfOver { get; }
        public int WicketsAtEndOfOver { get; }
        public int ScoreForThisOver { get; }
    }
}

