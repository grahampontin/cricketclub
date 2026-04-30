using CricketClubDomain;
using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Summary description for TeamV1
    /// </summary>
    public class TeamV1
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        /// <summary>URL to the team's logo image (served from /images/teams/{teamId}.png). Falls back to /images/teams/0.png.</summary>
        public string? LogoUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public int? HomeVenueId { get; set; }

        public TeamV1() { }

        /// <summary>Maps from internal Team. LogoUrl must be resolved separately via ResolveLogoUrl.</summary>
        public static TeamV1 FromInternal(Team team)
        {
            return new TeamV1
            {
                Id = team.ID,
                Name = team.Name,
                WebsiteUrl = team.WebsiteUrl,
                HomeVenueId = team.HomeVenueId
            };
        }

        /// <summary>Maps from internal Team and resolves the logo URL using the provided resolver (mirrors player image pattern).</summary>
        public static TeamV1 FromInternal(Team team, Func<int, string> logoUrlResolver)
        {
            var v1 = FromInternal(team);
            v1.LogoUrl = logoUrlResolver(team.ID);
            return v1;
        }

        /// <summary>Maps directly from <see cref="TeamData"/> — no domain object required.</summary>
        public static TeamV1 FromData(TeamData data, Func<int, string>? logoUrlResolver = null)
        {
            return new TeamV1
            {
                Id = data.ID,
                Name = data.Name,
                WebsiteUrl = data.WebsiteUrl,
                HomeVenueId = data.HomeVenueId > 0 ? data.HomeVenueId : null,
                LogoUrl = logoUrlResolver?.Invoke(data.ID)
            };
        }
    }
}