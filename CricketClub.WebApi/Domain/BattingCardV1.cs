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
        public List<BattingEntryV1> entries;
        public ExtrasV1 extras;
        public int score;
        public int wickets;

        private readonly List<ModesOfDismissal> notOutThings = new List<ModesOfDismissal>()
            { ModesOfDismissal.RetiredHurt, ModesOfDismissal.NotOut, ModesOfDismissal.DidNotBat };

        public BattingCardV1(BattingCard internalModel, Extras extras)
        {
            this.entries = internalModel.ScorecardData.Select(d => new BattingEntryV1(d)).ToList();
            this.extras = new ExtrasV1(extras);
            this.score = entries.Sum(e => e.runs) + this.extras.total;
            this.wickets = internalModel.ScorecardData.Count(e => !notOutThings.Contains(e.Dismissal));
        }


        // ReSharper disable once UnusedMember.Global
        public BattingCardV1()
        {
        }

        public BattingCard ToInternalBattingCard(Match match, ThemOrUs themOrUs)
        {
            var battingCard = new BattingCard(match.ID, themOrUs);
            battingCard.Extras = extras.GetTotal();
            battingCard.ScorecardData.Clear();
            battingCard.ScorecardData.AddRange(entries.Select(e=>e.ToInternal(match)));

            return battingCard;
        }

        public Extras ToInternalExtras(int matchId, ThemOrUs themOrUs)
        {
            return extras.ToInternal(matchId, themOrUs);
        }
    }
}