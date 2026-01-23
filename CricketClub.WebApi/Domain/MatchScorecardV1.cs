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
    public class MatchScorecardV1
    {
        public InningsScoreCardV1 ourInnings;
        public InningsScoreCardV1 theirInnings;
        public MatchConditionsV1 matchConditions;

        public MatchScorecardV1(BattingCard ourBatting, BowlingStats theirBowling, FoWStats ourFoW, BattingCard theirBatting, BowlingStats ourBowling, FoWStats theirFoW, Extras ourExtras, Extras theirExtras, Match match)
        {
            ourInnings = new InningsScoreCardV1(ourBatting, theirBowling, ourFoW, ourExtras,  match.OurInningsLength);
            theirInnings = new InningsScoreCardV1(theirBatting, ourBowling, theirFoW, theirExtras, match.TheirInningsLength);
            matchConditions = new MatchConditionsV1(match);
        }

        // Deserialize
        // ReSharper disable once UnusedMember.Global
        public MatchScorecardV1()
        {

        }

        public static MatchScorecardV1 GetExternalScorecard(Match match)
        {
            return new MatchScorecardV1(match.GetOurBattingScoreCard(), match.GetThierBowlingStats(),
                new FoWStats(match.ID, ThemOrUs.Us), match.GetTheirBattingScoreCard(), match.GetOurBowlingStats(),
                new FoWStats(match.ID, ThemOrUs.Them), new Extras(match.ID, ThemOrUs.Them),
                new Extras(match.ID, ThemOrUs.Us), match);
        }
    }
}