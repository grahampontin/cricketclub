using System;
using System.Collections.Generic;
using CricketClubDAL;
using CricketClubDomain;
using NUnit.Framework;

namespace CricketClub.Tests
{
    [TestFixture]
    public class BallByBallDaoTests
    {
        [Test]
        public void CanStartNewMatch()
        {
            Dao da = new Dao();
            int matchId = da.CreateNewMatch(1, DateTime.Now, 1, 1, HomeOrAway.Home);
            da.StartBallByBallCoverage(matchId, new List<int>{1,2,3,4,5,6,7,8,9,10,11});
        }
    }
}
