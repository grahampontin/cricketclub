using CricketClubDomain;
using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Summary description for MatchV1
    /// </summary>
    public class MatchV1
    {
        public MatchV1()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static MatchV1 FromInternal(Match match)
        {
            return new MatchV1()
            {
                Id = match.ID,
                Venue = VenueV1.FromInternal(match.Venue),
                Opposition = TeamV1.FromInternal(match.Opposition),
                Date = match.MatchDate.ToString("yyyy-MM-ddTHH:mm:ssK"),
                Type = match.Type.ToString(),
                IsHome = match.HomeOrAway == HomeOrAway.Home
            };
        }

        /// <summary>Maps from internal Match and resolves the opposition team's logo URL.</summary>
        public static MatchV1 FromInternal(Match match, Func<int, string> logoUrlResolver)
        {
            var v1 = FromInternal(match);
            v1.Opposition.LogoUrl = logoUrlResolver(match.Opposition.ID);
            return v1;
        }

        public bool IsHome { get; set; }

        public string Type { get; set; }

        public string Date { get; set; }

        public TeamV1 Opposition { get; set; }

        public VenueV1 Venue { get; set; }

        public int Id { get; set; }
    }
}