using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;
using CricketClubDAL;
using CricketClubDomain;
using CricketClubMiddle.Stats;

namespace CricketClubMiddle
{
    public class Venue
    {
        private InternalCache venueCache = InternalCache.GetInstance();
        private VenueData _data;
        private IDao myDAO;

        public Venue(int VenueID) : this(VenueID, new Dao())
        {
        }

        public Venue(int VenueID, IDao dao)
        {
            myDAO = dao;
            if (venueCache.Get("venue" + VenueID) == null)
            {
                _data = myDAO.GetVenueData(VenueID);
                venueCache.Insert("venue" + VenueID, _data, new TimeSpan(24, 0, 0));
            }
            else
            {
                _data = (VenueData)venueCache.Get("venue" + VenueID);
            }
        }

        public static Venue CreateNewVenue(string venueName, string mapUrl, string description, decimal? lat,
            decimal? lng)
        {
            return CreateNewVenue(venueName, mapUrl, description, lat, lng, new Dao());
        }

        public static Venue CreateNewVenue(string venueName, string mapUrl, string description, decimal? lat,
            decimal? lng, IDao dao)
        {
            var newVenueId = dao.CreateNewVenue(venueName, mapUrl, description, lat, lng);
            return new Venue(newVenueId, dao);
        }

        public string GoogleMapsLocationURL
        {
            get => _data.MapUrl;
            set => _data.MapUrl = value;
        }

        public string Name
        {
            get => _data.Name;
            set => _data.Name = value;
        }

        public int ID => _data.ID;

        public void Save()
        {
            myDAO.UpdateVenue(_data);
        }

        public void Delete()
        {
            myDAO.DeleteVenue(_data.ID);
            venueCache.Remove("venue" + _data.ID);
        }

        public static List<Venue> GetAll()
        {
            return GetAll(new Dao());
        }

        public static List<Venue> GetAll(IDao dao)
        {
            var data = dao.GetAllVenueData();
            var venues = new List<Venue>();
            foreach (var item in data)
            {
                venues.Add(new Venue(item, dao));

            }

            return venues;
        }

        public static Venue GetByName(string Name)
        {
            return GetByName(Name, new Dao());
        }

        public static Venue GetByName(string Name, IDao dao)
        {
            var venue = (from a in Venue.GetAll(dao) where a.Name == Name select a).FirstOrDefault();
            return venue;
        }

        private Venue(VenueData data) : this(data, null)
        {
        }

        private Venue(VenueData data, IDao dao)
        {
            _data = data;
            myDAO = dao;
        }

        public override string ToString()
        {
            return this.Name;
        }


        public VenueStats GetStats(DateTime fromDate, DateTime toDate, List<MatchType> matchTypes)
        {
            return new VenueStats(this, fromDate, toDate, matchTypes);
        }

        public string Description
        {
            get => _data.Description;
            set => _data.Description = value;
        }

        public Tuple<decimal?, decimal?> Coordinates
        {
            get => _data.Coordinates;
            set => _data.Coordinates = value;
        }
    }
}
