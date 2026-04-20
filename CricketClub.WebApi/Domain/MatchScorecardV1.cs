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
        public InningsScoreCardV1 OurInnings { get; set; }
        public InningsScoreCardV1 TheirInnings { get; set; }
        public MatchConditionsV1 MatchConditions { get; set; }
        public MatchReportV1 MatchReport { get; set; }

        public MatchScorecardV1(BattingCard ourBatting, BowlingStats theirBowling, FoWStats ourFoW, BattingCard theirBatting, BowlingStats ourBowling, FoWStats theirFoW, Extras ourExtras, Extras theirExtras, Match match)
        {
            OurInnings = new InningsScoreCardV1(ourBatting, theirBowling, ourFoW, ourExtras,  match.OurInningsLength);
            TheirInnings = new InningsScoreCardV1(theirBatting, ourBowling, theirFoW, theirExtras, match.TheirInningsLength);
            MatchConditions = new MatchConditionsV1(match);
            var report = match.GetMatchReport();
            MatchReport = new MatchReportV1(report.Conditions, report.Report, report.ReportImage);
        }

        // Deserialize
        // ReSharper disable once UnusedMember.Global
        public MatchScorecardV1()
        {

        }

        public static MatchScorecardV1 GetExternalScorecard(Match match)
        {
            // Use the same DAO as the Match instance to avoid creating new Dao() instances.
            var dao = match.Dao;

            return new MatchScorecardV1(
                match.GetOurBattingScoreCard(),
                match.GetThierBowlingStats(),
                new FoWStats(match.ID, ThemOrUs.Us, dao),
                match.GetTheirBattingScoreCard(),
                match.GetOurBowlingStats(),
                new FoWStats(match.ID, ThemOrUs.Them, dao),
                new Extras(match.ID, ThemOrUs.Them, dao),
                new Extras(match.ID, ThemOrUs.Us, dao),
                match);
        }
    }
}