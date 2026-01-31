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
        public int Wides { get; set; }
        public int NoBalls { get; set; }
        public int Penalties { get; set; }
        public int Byes { get; set; }
        public int LegByes { get; set; }
        public int Total { get; set; }

        // ReSharper disable once UnusedMember.Global
        public ExtrasV1()
        {
        }

        public ExtrasV1(Extras internalModelExtras)
        {
            Wides = internalModelExtras.Wides;
            NoBalls = internalModelExtras.NoBalls;
            Penalties = internalModelExtras.Penalty;
            Byes = internalModelExtras.Byes;
            LegByes = internalModelExtras.LegByes;
            Total = GetTotal();
        }

        public Extras ToInternal(int matchId, ThemOrUs themOrUs)
        {
            return new Extras(matchId, themOrUs)
            {
                Byes = Byes,
                LegByes = LegByes,
                NoBalls = NoBalls,
                Penalty = Penalties,
                Wides = Wides
            };
        }

        public int GetTotal()
        {
            return Byes + LegByes + NoBalls + Penalties + Wides;
        }
    }
}