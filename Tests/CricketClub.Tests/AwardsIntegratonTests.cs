using System;
using System.Linq;
using CricketClubDAL;
using CricketClubDomain;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace CricketClub.Tests
{
    public class AwardsIntegrationTests : IntegrationTestSupport
    {
    
        private readonly Dao dao = new Dao();
        private readonly Random random = new Random();


        [OneTimeTearDown]
        public void TearDown()
        {
            foreach (var awardData in dao.GetAllAwardsData())
            {
                dao.DeleteAward(awardData.Id);
            }
        }
        
        [Test]
        public void CanCreateAnAward()
        {
            var randomYear = random.Next();
            var awardData = new AwardData
            {
                Year = randomYear,
                Award = Award.PlayerOfTheYear,
                PlayerId = 1,
                Data = "Test Data"
            };
            
            var awardId = dao.CreateNewAward(awardData.Award, awardData.Year, awardData
                .PlayerId, awardData.Data);

            var saved = dao.GetAwardData(awardId);

            Assert.True(awardId > 0);
            Assert.AreEqual(awardData.Year, saved.Year);
            Assert.AreEqual(awardData.Award, saved.Award);
            Assert.AreEqual(awardData.PlayerId, saved.PlayerId);
            Assert.AreEqual(awardData.Data, saved.Data);
            
            
        }

        [Test]
        public void CanUpdateAnAward()
        {
            
            // arrange - create initial award
            var awardData = new AwardData
            {
                Year = random.Next(),
                Award = Award.PlayerOfTheYear,
                PlayerId = 1,
                Data = "Initial Data"
            };

            var awardId = dao.CreateNewAward(awardData.Award, awardData.Year, awardData.PlayerId, awardData.Data);
            Assert.True(awardId > 0);

            var saved = dao.GetAwardData(awardId);
            Assert.NotNull(saved);

            // act - modify and persist
            saved.Data = "Updated Test Data";
            saved.PlayerId = 2;
            saved.Award = Award.CaptainsPlayerOfTheYear;
            saved.Year = random.Next();

            dao.UpdateAward(saved);

            // assert - fetch again and verify changes
            var afterUpdate = dao.GetAwardData(awardId);
            Assert.NotNull(afterUpdate);
            Assert.AreEqual(saved.Data, afterUpdate.Data);
            Assert.AreEqual(saved.PlayerId, afterUpdate.PlayerId);
            Assert.AreEqual(saved.Award, afterUpdate.Award);
            Assert.AreEqual(saved.Year, afterUpdate.Year);
            

        }

        [Test]
        public void CanGetAllAwards()
        {
            var awardData = new AwardData
            {
                Year = random.Next(),
                Award = Award.PlayerOfTheYear,
                PlayerId = 1,
                Data = "Initial Data"
            };

            var awardId = dao.CreateNewAward(awardData.Award, awardData.Year, awardData.PlayerId, awardData.Data);
            Assert.True(awardId > 0);

            var awardDatas = dao.GetAllAwardsData() as AwardData[] ?? dao.GetAllAwardsData().ToArray();
            Assert.IsTrue(awardDatas.Any());
            Assert.IsTrue(awardDatas.Any(a => a.Id == awardId));
        }
    }
}