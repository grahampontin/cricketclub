using CricketClubDomain;
using CricketClubMiddle;

namespace CricketClub.WebApi.Domain
{
    /// <summary>
    /// Summary description for VenueV1
    /// </summary>
    public class VenueV1
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MapUrl { get; set; }
        public string Description { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public VenueV1()
        {
        }

        public static VenueV1 FromInternal(Venue venue)
        {
            return new VenueV1()
            {
                Id = venue.ID,
                Name = venue.Name,
                MapUrl = venue.GoogleMapsLocationURL,
                Description = venue.Description,
                Latitude = venue.Coordinates.Item1,
                Longitude = venue.Coordinates.Item2
            };
        }

        /// <summary>Maps directly from <see cref="VenueData"/> — no domain object required.</summary>
        public static VenueV1 FromData(VenueData data)
        {
            return new VenueV1
            {
                Id = data.ID,
                Name = data.Name,
                MapUrl = data.MapUrl,
                Description = data.Description,
                Latitude = data.Coordinates?.Item1,
                Longitude = data.Coordinates?.Item2
            };
        }
    }

}
