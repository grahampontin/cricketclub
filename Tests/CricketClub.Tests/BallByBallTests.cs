using CricketClubMiddle;
using NUnit.Framework;

namespace CricketClub.Tests
{
    [TestFixture]
    class BallByBallTests: IntegrationTestSupport
    {
        [Test]
        [Category("RequiresDatabase")]
        public void CanPopuateScorecards()
        {
            var match = new Match(381);
            var liveScorecard = match.GetLiveScorecard();
            Assert.NotNull(liveScorecard.LiveBattingCard);
            match.PopulateScorecardFromBallByBallData();
        }

        [Test]
        [Category("RequiresDatabase")]
        public void CanLoad353()
        {
            var match = new Match(353);
            match.GetLiveScorecard();
        }
    }
}
