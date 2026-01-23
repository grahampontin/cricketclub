#nullable disable
namespace CricketClub.WebApi
{
    public class Utils
    {
        public static Q ParseEnumOrThrow<T, Q>(string enumAsString, Func<T, Q> parsedAction) where T : struct
        {
            if (Enum.TryParse<T>(enumAsString, true, out var award))
            {
                return parsedAction.Invoke(award);
            }
            else
            {
                throw new ArgumentException($"Enum value '{enumAsString}' is not recognized for type {typeof(T).Name}");
            }
        }
    }
}