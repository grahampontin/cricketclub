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
        public BattingCardV1 batting;
        public BowlingCardV1 bowling;
        public FoWV1 fow;
        public double inningsLength;

        public InningsScoreCardV1(BattingCard batting, BowlingStats bowling, FoWStats fow, Extras extras, double inningsLength)
        {
            this.batting = new BattingCardV1(batting, extras);
            this.bowling = new BowlingCardV1(bowling);
            this.fow = new FoWV1(fow);
            this.inningsLength = inningsLength;
        }

        // ReSharper disable once UnusedMember.Global
        public InningsScoreCardV1()
        {
        }
    }
}