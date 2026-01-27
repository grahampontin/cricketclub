using System.Diagnostics.CodeAnalysis;
using CricketClubMiddle.Stats;

namespace CricketClub.WebApi.Domain
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public class InningsScoreCardV1
    {
        public BattingCardV1 Batting { get; set; }
        public BowlingCardV1 Bowling { get; set; }
        public FoWV1 Fow { get; set; }
        public double InningsLength { get; set; }

        public InningsScoreCardV1(BattingCard batting, BowlingStats bowling, FoWStats fow, Extras extras, double inningsLength)
        {
            Batting = new BattingCardV1(batting, extras);
            Bowling = new BowlingCardV1(bowling);
            Fow = new FoWV1(fow);
            InningsLength = inningsLength;
        }

        // ReSharper disable once UnusedMember.Global
        public InningsScoreCardV1()
        {
        }
    }
}