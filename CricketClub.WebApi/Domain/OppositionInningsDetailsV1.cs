using System.Diagnostics.CodeAnalysis;

namespace CricketClub.WebApi.Domain
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class OppositionInningsDetailsV1
    {
        public int Over { get; set; }
        public int Score { get; set; }
        public int Wickets { get; set; }
        public string Commentary { get; set; }
    }
}
