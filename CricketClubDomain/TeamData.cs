using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CricketClubDomain
{
    public class TeamData
    {
        public TeamData()
        {

        }

        public int ID { get; set; }

        public string Name { get; set; }


        public string? WebsiteUrl { get; set; }

        public int? HomeVenueId { get; set; }
    }
}
