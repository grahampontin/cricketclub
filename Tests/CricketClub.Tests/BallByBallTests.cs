using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CricketClubMiddle;
using NUnit.Framework;

namespace CricketClub.Tests
{
    [TestFixture]
    class BallByBallTests
    {
        [Test]
        public void CanPopuateScorecards()
        {
            var match = new Match(394);
            match.PopulateScorecardFromBallByBallData();
        }
    }
}
