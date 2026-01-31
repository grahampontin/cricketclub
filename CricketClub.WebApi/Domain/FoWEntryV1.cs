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

        public FoWPlayerV1 OutgoingPlayer { get; set; }
        public FoWPlayerV1 NotOutPlayer { get; set; }
        public int Wicket { get; set; }
        public int Score { get; set; }
        public decimal Overs { get; set; }
        public int Partnership { get; set; }

        public FoWEntryV1(FoWStatsLine foWStatsLine)
        {
            OutgoingPlayer = new FoWPlayerV1()
            {
                BattingAt = foWStatsLine.OutgoingBatsmanPosition,
                Id = foWStatsLine.OutgoingBatsman.Id,
                Name = foWStatsLine.OutgoingBatsman.Name,
                Score = foWStatsLine.OutgoingBatsmanScore
            };
            NotOutPlayer = new FoWPlayerV1()
            {
                BattingAt = foWStatsLine.NotOutBatsmanPosition,
                Id = foWStatsLine.NotOutBatsman.Id,
                Name = foWStatsLine.NotOutBatsman.Name,
                Score = foWStatsLine.NotOutBatsmanScore
            };
            Wicket = foWStatsLine.Wicket;
            Score = foWStatsLine.Score;
            Overs = foWStatsLine.Over;
            Partnership = foWStatsLine.Partnership;
        }

        public FoWStatsLine ToInternal(int matchId, ThemOrUs themOrUs)
        {
            return new FoWStatsLine(new FoWDataLine()
            {
                OutgoingBatsman = OutgoingPlayer.BattingAt,
                OutgoingBatsmanScore = OutgoingPlayer.Score,
                NotOutBatsman = NotOutPlayer.BattingAt,
                NotOutBatsmanScore = NotOutPlayer.Score,
                Score = Score,
                Partnership = Partnership,
                Wicket = Wicket,
                MatchID = matchId,
                OverNumber = (int)Overs,
                Who = themOrUs
            });
        }
    }
}