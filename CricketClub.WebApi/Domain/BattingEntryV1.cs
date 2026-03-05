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
    public class BattingEntryV1
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int Runs { get; set; }
        public ModesOfDismissalV1 ModeOfDismissal { get; set; }
        public int BowlerId { get; set; }
        public string BowlerName { get; set; }
        public int FielderId { get; set; }
        public string FielderName { get; set; }
        public int Fours { get; set; }
        public int Sixes { get; set; }
        public int BattingAt { get; set; }
        public int BallsFaced { get; set; }
        public int DotBalls { get; set; }
        public WicketV1 Wicket { get; set; }

        // ReSharper disable once UnusedMember.Global
        public BattingEntryV1()
        {
        }

        public BattingEntryV1(BattingCardLine battingCardLine)
        {
            PlayerId = battingCardLine.Batsman.Id;
            PlayerName = battingCardLine.Batsman.Name;
            Runs = battingCardLine.Score;
            ModeOfDismissal = EnumMappers.ToV1(battingCardLine.Dismissal);
            BowlerId = battingCardLine.Bowler.Id;
            BowlerName = battingCardLine.Bowler.Name;
            FielderId = battingCardLine.Fielder.Id;
            FielderName = battingCardLine.Fielder.Name;
            Fours = battingCardLine.Fours;
            Sixes = battingCardLine.Sixes;
            BattingAt = battingCardLine.BattingAt;
            BallsFaced = battingCardLine.BallsFaced;
            DotBalls = battingCardLine.DotBalls;
            Wicket = new WicketV1(BowlerName, FielderName, EnumMappers.ToV1(battingCardLine.Dismissal));
        }

        public BattingCardLine ToInternal(Match match)
        {
            var dismissal = EnumMappers.ToInternal(ModeOfDismissal);

            return new BattingCardLine(new BattingCardLineData()
            {
                BattingAt = BattingAt,
                BowlerID = BowlerId,
                BowlerName = BowlerName,
                FielderID = FielderId,
                FielderName = FielderName,
                Fours = Fours,
                MatchDate = match.MatchDate,
                MatchID = match.ID,
                MatchTypeID = (int)match.Type,
                ModeOfDismissal = (int)dismissal,
                PlayerID = PlayerId,
                PlayerName = PlayerName,
                Runs = Runs - (Fours * 4 + Sixes * 6),
                Score = Runs,
                Sixes = Sixes,
                VenueID = match.VenueID,
                BallsFaced = BallsFaced,
                DotBalls = DotBalls
            });
        }
    }
}