using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CricketClubDomain
{
    public class VenueData
    {
        public VenueData()
        {
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string MapUrl { get; set; }
        
        public Tuple<decimal?, decimal?> Coordinates { get; set; }
        
        public string Description { get; set; }
        
        
    }
}
