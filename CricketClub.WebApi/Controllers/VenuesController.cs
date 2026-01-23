#nullable disable
using System.Collections.Specialized;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubMiddle;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class VenuesController : EntityControllerBase<VenueV1>
    {
        public VenuesController(IDao database) : base(database)
        {
        }

        [HttpGet]
        [HttpGet("{id}")]
        [HttpPost]
        [HttpPut]
        [HttpDelete("{id}")]
        public async Task<IActionResult> HandleRequest()
        {
            return await ProcessRequest();
        }

        protected override VenueV1 UpdateEntity(VenueV1 entity)
        {
            var venue = new Venue(entity.Id, database)
            {
                Name = entity.Name,
                GoogleMapsLocationURL = entity.MapUrl,
                Description = entity.Description,
                Coordinates = new Tuple<decimal?, decimal?>(entity.Latitude, entity.Longitude)
            };
            venue.Save();
            return entity;
        }

        protected override void DeleteEntity(int id)
        {
            var venue = new Venue(id, database);
            if (!string.IsNullOrEmpty(venue.Name))
            {
                venue.Delete();
            }
        }

        protected override VenueV1 CreateEntity(VenueV1 entity)
        {
            Venue.CreateNewVenue(entity.Name, entity.MapUrl, entity.Description, entity.Latitude, entity.Longitude, database);
            var venues = Venue.GetAll(database);
            var createdVenue = venues.OrderByDescending(v => v.ID).FirstOrDefault(v => v.Name == entity.Name);
            return createdVenue == null ? entity : VenueV1.FromInternal(createdVenue);
        }

        protected override List<VenueV1> GetAllEntities(NameValueCollection requestQueryString)
        {
            return Venue.GetAll(database).Select(VenueV1.FromInternal).OrderBy(v => v.Name).ToList();
        }

        protected override VenueV1 GetEntity(int id)
        {
            var venue = new Venue(id, database);
            return string.IsNullOrEmpty(venue.Name) ? null : VenueV1.FromInternal(venue);
        }

        public override string GetTypeName()
        {
            return "venues";
        }
    }
}
