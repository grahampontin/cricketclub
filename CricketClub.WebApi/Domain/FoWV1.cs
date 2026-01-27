using System.Diagnostics.CodeAnalysis;
using CricketClubDomain;
using CricketClubMiddle;
using CricketClubMiddle.Stats;

namespace CricketClub.WebApi.Domain
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public class FoWV1
    {
        public List<FoWEntryV1> Entries { get; set; }

        // ReSharper disable once UnusedMember.Global
        public FoWV1()
        {
        }

        public FoWV1(FoWStats internalModel)
        {
            Entries = internalModel.Data.Select(d => new FoWEntryV1(d)).ToList();
        }

        public FoWStats ToInternal(Match match, ThemOrUs themOrUs)
        {
            var foWStats = new FoWStats(match.ID, themOrUs);
            foWStats.Data.Clear();
            foWStats.Data.AddRange(Entries.Select(e => e.ToInternal(match.ID, themOrUs)));
            return foWStats;

        }
    }
}