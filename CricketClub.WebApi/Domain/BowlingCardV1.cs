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
    public class BowlingCardV1
    {
        public List<BowlingEntryV1> entries;

        // ReSharper disable once UnusedMember.Global
        public BowlingCardV1()
        {
        }

        public BowlingCardV1(BowlingStats internalModel)
        {
            entries = internalModel.BowlingStatsData.Select(s => new BowlingEntryV1(s)).ToList();
        }

        public BowlingStats ToInternal(Match match, ThemOrUs themOrUs)
        {
            var bowlingStats = new BowlingStats(match.ID, themOrUs);
            bowlingStats.BowlingStatsData.Clear();
            bowlingStats.BowlingStatsData.AddRange(entries.Select(e=>e.ToInternal(match)));
            return bowlingStats;
        }
    }
}