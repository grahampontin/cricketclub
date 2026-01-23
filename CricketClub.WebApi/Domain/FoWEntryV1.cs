using System.Diagnostics.CodeAnalysis;
using CricketClubDomain;
using CricketClubMiddle.Stats;

namespace CricketClub.WebApi.Domain
{
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class FoWEntryV1
    {
        // ReSharper disable once UnusedMember.Global
        public FoWEntryV1()
        {
        }

        public FoWPlayerV1 outgoingPlayer;
        public FoWPlayerV1 notOutPlayer;
        public int wicket;
        public int score;
        public decimal overs;
        public int partnership;
        

        public FoWEntryV1(FoWStatsLine foWStatsLine)
        {
            outgoingPlayer = new FoWPlayerV1()
            {
                battingAt = foWStatsLine.OutgoingBatsmanPosition,
                id = foWStatsLine.OutgoingBatsman.Id,
                name = foWStatsLine.OutgoingBatsman.Name,
                score = foWStatsLine.OutgoingBatsmanScore
            };
            notOutPlayer = new FoWPlayerV1()
            {
                battingAt = foWStatsLine.NotOutBatsmanPosition,
                id = foWStatsLine.NotOutBatsman.Id,
                name = foWStatsLine.NotOutBatsman.Name,
                score = foWStatsLine.NotOutBatsmanScore
            };
            wicket = foWStatsLine.Wicket;
            score = foWStatsLine.Score;
            overs = foWStatsLine.Over;
            partnership = foWStatsLine.Partnership;
        }

        public FoWStatsLine ToInternal(int matchId, ThemOrUs themOrUs)
        {
            return new FoWStatsLine(new FoWDataLine()
            {
                OutgoingBatsman = outgoingPlayer.battingAt,
                OutgoingBatsmanScore = outgoingPlayer.score,
                NotOutBatsman = notOutPlayer.battingAt,
                NotOutBatsmanScore = notOutPlayer.score,
                Score = score,
                Partnership = partnership,
                Wicket = wicket,
                MatchID = matchId,
                OverNumber = (int) overs,
                Who = themOrUs
            });
        }
    }
}