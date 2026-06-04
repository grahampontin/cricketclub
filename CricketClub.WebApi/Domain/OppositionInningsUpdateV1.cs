using System.Diagnostics.CodeAnalysis;

namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// The state update the frontend submits after scoring each opposition over (ball-by-ball mode).
    /// Mirrors MatchStateUpdateV1 but for the opposition innings:
    ///   - batsmen are string names (no player IDs)
    ///   - bowlers are OUR player IDs
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class OppositionInningsUpdateV1
    {
        /// <summary>Over number just completed (1-based).</summary>
        public int LastCompletedOver { get; set; }
        /// <summary>Name of the batter who will be on strike at the start of the next over.</summary>
        public string OnStrikeBatsmanName { get; set; }
        /// <summary>The over that was just bowled.</summary>
        public OppositionOverV1 Over { get; set; }
        /// <summary>Full state snapshot of all opposition batters after this over.</summary>
        public OppositionBatterStateV1[] Players { get; set; }
    }
}

