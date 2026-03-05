using CricketClubMiddle;
using CricketClubMiddle.Stats;

namespace CricketClub.WebApi.Domain
{
    public class FallOfWicketV1
    {
        public int WicketNumber { get; set; }
        public int TeamScore { get; set; }
        public string OverAsString { get; set; }
        public string Bowler { get; set; }
        public int OutGoingPlayerId { get; set; }
        public string OutgoingPlayerName { get; set; }
        public int OutGoingPlayerScore { get; set; }
        public int NotOutPlayerId { get; set; }
        public int NotOutPlayerScore { get; set; }
        public WicketV1 Wicket { get; set; }
        public BatsmanInningsDetails OutgoingBatsmanInningsDetails { get; set; }
        public Partnership Partnership { get; set; }

        public static FallOfWicketV1 FromInternal(FallOfWicket fow)
        {
            if (fow == null) return null;
            return new FallOfWicketV1
            {
                WicketNumber = fow.WicketNumber,
                TeamScore = fow.TeamScore,
                OverAsString = fow.OverAsString,
                Bowler = fow.Bowler,
                OutGoingPlayerId = fow.OutGoingPlayerId,
                OutgoingPlayerName = fow.OutgoingPlayerName,
                OutGoingPlayerScore = fow.OutGoingPlayerScore,
                NotOutPlayerId = fow.NotOutPlayerId,
                NotOutPlayerScore = fow.NotOutPlayerScore,
                Wicket = fow.Wicket != null
                    ? new WicketV1
                    {
                        Player = fow.Wicket.Player,
                        PlayerName = fow.Wicket.PlayerName,
                        ModeOfDismissal = EnumMappers.ToV1(fow.Wicket.ModeOfDismissalAsEnum),
                        Bowler = fow.Wicket.Bowler,
                        Fielder = fow.Wicket.Fielder,
                        Description = fow.Wicket.Description
                    }
                    : null,
                OutgoingBatsmanInningsDetails = fow.OutgoingBatsmanInningsDetails,
                Partnership = fow.Partnership
            };
        }
    }
}
