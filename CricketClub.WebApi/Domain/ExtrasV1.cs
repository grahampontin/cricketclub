using System.Diagnostics.CodeAnalysis;
using CricketClubDomain;
using CricketClubMiddle.Stats;

namespace CricketClub.WebApi.Domain
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class ExtrasV1
    {
        public int wides;
        public int noBalls;
        public int penalties;
        public int byes;
        public int legByes;
        public int total;

        // ReSharper disable once UnusedMember.Global
        public ExtrasV1()
        {
        }

        public ExtrasV1(Extras internalModelExtras)
        {
            this.wides = internalModelExtras.Wides;
            this.noBalls = internalModelExtras.NoBalls;
            this.penalties = internalModelExtras.Penalty;
            this.byes = internalModelExtras.Byes;
            this.legByes = internalModelExtras.LegByes;
            this.total = GetTotal();
        }

        public Extras ToInternal(int matchId, ThemOrUs themOrUs)
        {
            return new Extras(matchId, themOrUs)
            {
                Byes = byes,
                LegByes = legByes,
                NoBalls = noBalls,
                Penalty = penalties,
                Wides = wides
            };
        }

        public int GetTotal()
        {
            return byes + legByes + noBalls + penalties + wides;
        }
    }
}