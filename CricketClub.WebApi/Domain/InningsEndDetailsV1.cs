using System.Diagnostics.CodeAnalysis;

namespace CricketClub.WebApi.Domain
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class InningsEndDetailsV1
    {
        public string Commentary { get; set; }
        public string InningsType { get; set; }
        public bool WasDeclared { get; set; }
    }
}

