using System.Collections.Generic;

namespace CricketClubMiddle
{
    public class BallByBallMatchConditions
    {
        public int Captain { get; set; }
        public int Keeper { get; set; }
        public bool WonToss { get; set; }
        public bool Batted { get; set; }
        public bool Declaration { get; set; }
        public int Overs { get; set; }
        public int[] PlayerIds { get; set; }
    }
}