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
    public class BattingCardV1
    {
        public List<BattingEntryV1> Entries { get; set; }
        public ExtrasV1 Extras { get; set; }
        public int Score { get; set; }
        public int Wickets { get; set; }

        private readonly List<ModesOfDismissal> notOutThings = new List<ModesOfDismissal>()
            { ModesOfDismissal.RetiredHurt, ModesOfDismissal.NotOut, ModesOfDismissal.DidNotBat };

        public BattingCardV1(BattingCard internalModel, Extras extras)
        {
            Entries = internalModel.ScorecardData.Select(d => new BattingEntryV1(d)).ToList();
            Extras = new ExtrasV1(extras);
            Score = Entries.Sum(e => e.Runs) + Extras.total;
            Wickets = internalModel.ScorecardData.Count(e => !notOutThings.Contains(e.Dismissal));
        }


        // ReSharper disable once UnusedMember.Global
        public BattingCardV1()
        {
        }

        public BattingCard ToInternalBattingCard(Match match, ThemOrUs themOrUs)
        {
            var battingCard = new BattingCard(match.ID, themOrUs);
            battingCard.Extras = Extras.GetTotal();
            battingCard.ScorecardData.Clear();
            battingCard.ScorecardData.AddRange(Entries.Select(e => e.ToInternal(match)));

            return battingCard;
        }

        public Extras ToInternalExtras(int matchId, ThemOrUs themOrUs)
        {
            return Extras.ToInternal(matchId, themOrUs);
        }
    }
}