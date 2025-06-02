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
            var match = new Match(381);
            var liveScorecard = match.GetLiveScorecard();
            Assert.NotNull(liveScorecard.LiveBattingCard);
            match.PopulateScorecardFromBallByBallData();
        }

        [Test]
        public void CanLoad353()
        {
            var match = new Match(353);
            match.GetLiveScorecard();
        }
    }
}
