using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CricketClubDomain
{
    public class AwardData
    {
        public AwardData()
        {
        }

        public int Year { get; set; }
        public Award Award { get; set; }
        public int PlayerId { get; set; }
        public string Data { get; set; }
        public int ID { get; set; }
    }
}
