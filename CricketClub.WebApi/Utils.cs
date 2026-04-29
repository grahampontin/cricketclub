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

        /// <summary>
        /// Returns the URL for a team's logo image.
        /// Looks for Assets/TeamImages/{teamId}.png; falls back to 0.png when no file exists.
        /// Mirrors the player image pattern used in TeamsController and StatsProvider.
        /// </summary>
        public static string ResolveTeamLogoUrl(int teamId, string contentRootPath, string baseUrl)
        {
            var imageRoot = Path.Combine(contentRootPath, "Assets", "TeamImages");
            var imagePath = Path.Combine(imageRoot, $"{teamId}.png");
            var resolvedId = File.Exists(imagePath) ? teamId : 0;
            return new Uri(new Uri(baseUrl), $"/images/teams/{resolvedId}.png").ToString();
        }
    }
}