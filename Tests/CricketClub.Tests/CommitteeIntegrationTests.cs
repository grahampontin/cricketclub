﻿// csharp

using System.Linq;
using CricketClubDAL;
using CricketClubDomain;
using NUnit.Framework;

namespace CricketClub.Tests
{
    [TestFixture]
    [Category("RequiresDatabase")]
    public class CommitteeIntegrationTests : IntegrationTestSupport
    {
        private Dao dao;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            dao = new Dao();
        }

        [Test]
        public void CreateGetUpdateDeleteCommittee()
        {
            // Pick values unlikely to collide with real data
            var testYear = 2099;
            var initialPlayerId = 0;
            var updatedPlayerId = 123456;

            var committee = new CommitteeData
            {
                // Use a valid Post enum value from your domain (adjust if necessary)
                Post = Post.Captain,
                Year = testYear,
                PlayerId = initialPlayerId
            };

            int createdId = 0;
            try
            {
                // Create
                createdId = dao.CreateNewCommittee(committee);
                Assert.That(createdId, Is.GreaterThan(0), "CreateNewCommittee should return new id");

                // Get by id
                var fetched = dao.GetCommitteeData(createdId);
                Assert.IsNotNull(fetched, "GetCommitteeData should return the created record");
                Assert.AreEqual(testYear, fetched.Year);
                Assert.AreEqual(initialPlayerId, fetched.PlayerId);
                Assert.AreEqual(committee.Post, fetched.Post);

                // Get all contains created
                var all = dao.GetAllCommitteeData().ToList();
                Assert.IsTrue(all.Any(c => c.Id == createdId), "GetAllCommitteeData should include the created record");

                // Update
                fetched.PlayerId = updatedPlayerId;
                var updatedPost = Post.FixturesSecretary;
                fetched.Post = updatedPost;
                dao.UpdateCommittee(fetched);

                var updated = dao.GetCommitteeData(createdId);
                Assert.IsNotNull(updated);
                Assert.AreEqual(updatedPlayerId, updated.PlayerId, "UpdateCommittee should persist player id change");
                Assert.AreEqual(updatedPost, fetched.Post, "updateCommittee should persist post");    
                
                // Delete
                dao.DeleteCommittee(createdId);
                var afterDelete = dao.GetCommitteeData(createdId);
                Assert.IsNull(afterDelete, "DeleteCommittee should remove the record");
            }
            finally
            {
                // Ensure cleanup if test failed before delete
                if (createdId > 0)
                {
                    try { dao.DeleteCommittee(createdId); } catch { /* ignore cleanup errors */ }
                }
            }
        }
    }
}
