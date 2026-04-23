using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Detailed team information including match history and difficulty rating.
    /// </summary>
    public class TeamDetailV1
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// URL to the team's logo image (served from /images/teams/{teamId}.png).
        /// Falls back to /images/teams/0.png if no logo file exists.
        /// Drop a PNG named {teamId}.png into Assets/TeamImages/ to add a logo.
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>Link to the team's website, or null if not available.</summary>
        public string? WebsiteUrl { get; set; }

        /// <summary>ID of this team's home venue (for linking to the venues page), or null if unknown.</summary>
        public int? HomeVenueId { get; set; }

        /// <summary>Name of this team's home venue, or null if unknown.</summary>
        public string? HomeVenueName { get; set; }

        /// <summary>Traffic-light difficulty rating: "red" (hardest), "amber", "green" (easiest), or "unknown" (fewer than 3 completed matches).</summary>
        public string DifficultyRating { get; set; } = "green";

        /// <summary>Win percentage against this team as a fraction (0–1).</summary>
        public double WinPercentage { get; set; }

        /// <summary>Past results against this team.</summary>
        public List<ResultV1> Matches { get; set; } = new();
    }
}

