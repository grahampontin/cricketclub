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
        private Dao myDAO = new Dao();

        public Venue(int VenueID)
        {
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

        public static Venue CreateNewVenue(string venueName, string mapUrl)
        {
            Dao myDAO = new Dao();
            int newVenueId = myDAO.CreateNewVenue(venueName, mapUrl);
            return new Venue(newVenueId);
        }

        public string GoogleMapsLocationURL
        {
            get
            {
                return _data.MapUrl;
            }
            set
            {
                _data.MapUrl = value;
            }
        }

        public string Name
        {
            get
            {
                return _data.Name;
            }
            set
            {
                _data.Name = value;
            }
        }

        public int ID
        {
            get
            {
                return _data.ID;
            }
        }

        public void Save()
        {  
            myDAO.UpdateVenue(_data);
        }

        public static List<Venue> GetAll()
        {
            IEnumerable<VenueData> data = new Dao().GetAllVenueData();
            List<Venue> venues = new List<Venue>();
            foreach (VenueData item in data)
            {
                venues.Add(new Venue(item));

            }
            return venues;
        }

        public static Venue GetByName(string Name)
        {
            Venue venue = (from a in Venue.GetAll() where a.Name == Name select a).FirstOrDefault();
            return venue;
        }

        private Venue(VenueData data)
        {
            _data = data;
        }

        public override string ToString()
        {
            return this.Name;
        }


        public VenueStats GetStats(DateTime fromDate, DateTime toDate, List<MatchType> matchTypes)
        {
            return new VenueStats(this, fromDate, toDate, matchTypes);
        }
    }
}
