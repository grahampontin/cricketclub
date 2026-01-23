using System.Diagnostics.CodeAnalysis;
using CricketClub.WebApi.AGGrid;

namespace CricketClub.WebApi.Domain
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class StatsDataV1
    {
        public string statsType { get; set; }
        public AGGridOptions gridOptions { get; set; }
    }
}