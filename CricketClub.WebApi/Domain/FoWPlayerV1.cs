using System.Diagnostics.CodeAnalysis;

namespace CricketClub.WebApi.Domain
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public class FoWPlayerV1
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int BattingAt { get; set; }
        public int Score { get; set; }
    }
}