using System.Collections.Generic;
using System.Linq;
using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    public class LiveBattingCardV1
    {
        public Dictionary<string, LiveBattingCardEntryV1> Players { get; set; }
        public LiveExtrasV1 Extras { get; set; }

        public static LiveBattingCardV1 FromInternal(LiveBattingCard card)
        {
            if (card == null) return null;
            return new LiveBattingCardV1
            {
                Players = card.Players?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => LiveBattingCardEntryV1.FromInternal(kvp.Value)),
                Extras = LiveExtrasV1.FromInternal(card.Extras)
            };
        }
    }

    public class LiveBattingCardEntryV1
    {
        public BatsmanInningsDetailsV1 BatsmanInningsDetails { get; set; }
        public WicketV1 Wicket { get; set; }

        public static LiveBattingCardEntryV1 FromInternal(LiveBattingCardEntry entry)
        {
            if (entry == null) return null;
            return new LiveBattingCardEntryV1
            {
                BatsmanInningsDetails = BatsmanInningsDetailsV1.FromInternal(entry.BatsmanInningsDetails),
                Wicket = entry.Wicket != null
                    ? new WicketV1
                    {
                        Player = entry.Wicket.Player,
                        PlayerName = entry.Wicket.PlayerName,
                        ModeOfDismissal = EnumMappers.ToV1(entry.Wicket.ModeOfDismissalAsEnum),
                        Bowler = entry.Wicket.Bowler,
                        Fielder = entry.Wicket.Fielder,
                        Description = entry.Wicket.Description
                    }
                    : null
            };
        }
    }

    public class LiveExtrasV1
    {
        public int Byes { get; set; }
        public int LegByes { get; set; }
        public int Wides { get; set; }
        public int NoBalls { get; set; }
        public int Penalty { get; set; }
        public int Total { get; set; }
        public string DetailString { get; set; }

        public static LiveExtrasV1 FromInternal(LiveExtras extras)
        {
            if (extras == null) return null;
            return new LiveExtrasV1
            {
                Byes = extras.Byes,
                LegByes = extras.LegByes,
                Wides = extras.Wides,
                NoBalls = extras.NoBalls,
                Penalty = extras.Penalty,
                Total = extras.Total,
                DetailString = extras.DetailString
            };
        }
    }
}
