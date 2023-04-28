using System.Collections.Generic;
using CricketClubDAL;
using CricketClubDomain;

namespace CricketClubMiddle.Stats
{
    public class BallByBallStats
    {
        private static Dao dao;
        
        public static List<Ball> GetAllBalls()
        {
            return InitDao().GetAllBalls();
        }

        private static Dao InitDao()
        {
            return dao ?? (dao = new Dao());
        }
    }
}