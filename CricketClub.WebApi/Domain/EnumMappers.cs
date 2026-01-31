using CricketClubDomain;
using MatchType = CricketClubDomain.MatchType;

namespace CricketClub.WebApi.Domain
{
    public static class EnumMappers
    {
        public static ModesOfDismissalV1 ToV1(ModesOfDismissal value)
        {
            return value switch
            {
                ModesOfDismissal.NotOut => ModesOfDismissalV1.NotOut,
                ModesOfDismissal.Bowled => ModesOfDismissalV1.Bowled,
                ModesOfDismissal.Stumped => ModesOfDismissalV1.Stumped,
                ModesOfDismissal.RunOut => ModesOfDismissalV1.RunOut,
                ModesOfDismissal.Caught => ModesOfDismissalV1.Caught,
                ModesOfDismissal.CaughtAndBowled => ModesOfDismissalV1.CaughtAndBowled,
                ModesOfDismissal.LBW => ModesOfDismissalV1.LBW,
                ModesOfDismissal.HitWicket => ModesOfDismissalV1.HitWicket,
                ModesOfDismissal.DidNotBat => ModesOfDismissalV1.DidNotBat,
                ModesOfDismissal.Retired => ModesOfDismissalV1.Retired,
                ModesOfDismissal.RetiredHurt => ModesOfDismissalV1.RetiredHurt,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unhandled dismissal mode")
            };
        }

        public static ModesOfDismissal ToInternal(ModesOfDismissalV1 value)
        {
            return value switch
            {
                ModesOfDismissalV1.NotOut => ModesOfDismissal.NotOut,
                ModesOfDismissalV1.Bowled => ModesOfDismissal.Bowled,
                ModesOfDismissalV1.Stumped => ModesOfDismissal.Stumped,
                ModesOfDismissalV1.RunOut => ModesOfDismissal.RunOut,
                ModesOfDismissalV1.Caught => ModesOfDismissal.Caught,
                ModesOfDismissalV1.CaughtAndBowled => ModesOfDismissal.CaughtAndBowled,
                ModesOfDismissalV1.LBW => ModesOfDismissal.LBW,
                ModesOfDismissalV1.HitWicket => ModesOfDismissal.HitWicket,
                ModesOfDismissalV1.DidNotBat => ModesOfDismissal.DidNotBat,
                ModesOfDismissalV1.Retired => ModesOfDismissal.Retired,
                ModesOfDismissalV1.RetiredHurt => ModesOfDismissal.RetiredHurt,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unhandled dismissal mode")
            };
        }

        public static string ToWire(MatchType value)
        {
            // Keep the current wire format (Enum name) but centralize it.
            return value.ToString();
        }

        public static MatchType ParseMatchType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Match type is required", nameof(value));
            }

            if (Enum.TryParse<MatchType>(value, true, out var parsed))
            {
                return parsed;
            }

            throw new ArgumentException($"Unknown match type '{value}'", nameof(value));
        }

        public static ModesOfDismissal ParseModesOfDismissal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Mode of dismissal is required", nameof(value));
            }

            if (Enum.TryParse<ModesOfDismissal>(value, true, out var parsed))
            {
                return parsed;
            }

            throw new ArgumentException($"Unknown mode of dismissal '{value}'", nameof(value));
        }
    }
}
