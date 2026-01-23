#nullable disable
using System.Text.Json;
using CricketClub.WebApi.Controllers;
using CricketClub.WebApi.Domain;
using CricketClub.WebApi.Tests.Utils;
using CricketClubDAL;
using CricketClubDomain;
using Moq;
using Xunit;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class CommitteeControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly CommitteeController controller;

        public CommitteeControllerTests()
        {
            mockDao = new Mock<IDao>();
            controller = new CommitteeController(mockDao.Object);
        }

        [Fact]
        public void ProcessRequest_GetAll_CallsDaoGetAllCommitteeData()
        {
            // Arrange
            var committeeData = new CommitteeData { Id = 1, Year = 2023, Post = Post.Captain, PlayerId = 1 };
            mockDao.Setup(d => d.GetAllCommitteeData()).Returns(new List<CommitteeData> { committeeData });

            var context = TestControllerContextFactory.CreateHttpContext("GET", "http://test.com/api/committee");

            // Act
            controller.ProcessRequest(context);

            // Assert
            mockDao.Verify(d => d.GetAllCommitteeData(), Times.Once);
        }

        [Fact]
        public void ProcessRequest_GetSingle_CallsDaoGetCommitteeDataWithCorrectId()
        {
            // Arrange
            var committeeData = new CommitteeData { Id = 123, Year = 2023, Post = Post.Captain, PlayerId = 1 };
            mockDao.Setup(d => d.GetCommitteeData(123)).Returns(committeeData);

            var context = TestControllerContextFactory.CreateHttpContext("GET", "http://test.com/api/committee/123");

            // Act
            controller.ProcessRequest(context);

            // Assert
            mockDao.Verify(d => d.GetCommitteeData(123), Times.Once);
        }

        [Fact]
        public void ProcessRequest_Post_CallsDaoCreateNewCommitteeWithCorrectEntity()
        {
            // Arrange
            var newCommittee = new CommitteePostV1 { Year = 2023, Post = "Captain", PlayerId = 1 };
            var createdCommitteeData = new CommitteeData { Id = 1, Year = 2023, Post = Post.Captain, PlayerId = 1 };

            mockDao.Setup(d => d.CreateNewCommittee(It.IsAny<CommitteeData>())).Returns(1);
            mockDao.Setup(d => d.GetCommitteeData(1)).Returns(createdCommitteeData);

            var body = JsonSerializer.Serialize(newCommittee);
            var context = TestControllerContextFactory.CreateHttpContext("POST", "http://test.com/api/committee", body);

            // Act
            controller.ProcessRequest(context);

            // Assert
            mockDao.Verify(d => d.CreateNewCommittee(It.Is<CommitteeData>(c =>
                c.Year == 2023 &&
                c.Post == Post.Captain &&
                c.PlayerId == 1)), Times.Once);
            mockDao.Verify(d => d.GetCommitteeData(1), Times.Once);
        }

        [Fact]
        public void ProcessRequest_Put_CallsDaoUpdateCommitteeWithCorrectEntity()
        {
            // Arrange
            var updateCommittee = new CommitteePostV1 { Id = 1, Year = 2023, Post = "Captain", PlayerId = 1 };
            mockDao.Setup(d => d.UpdateCommittee(It.IsAny<CommitteeData>()));

            var body = JsonSerializer.Serialize(updateCommittee);
            var context = TestControllerContextFactory.CreateHttpContext("PUT", "http://test.com/api/committee", body);

            // Act
            controller.ProcessRequest(context);

            // Assert
            mockDao.Verify(d => d.UpdateCommittee(It.Is<CommitteeData>(c =>
                c.Id == 1 &&
                c.Year == 2023 &&
                c.Post == Post.Captain &&
                c.PlayerId == 1)), Times.Once);
        }

        [Fact]
        public void ProcessRequest_Delete_CallsDaoDeleteCommitteeWithCorrectId()
        {
            // Arrange
            mockDao.Setup(d => d.DeleteCommittee(123));

            var context = TestControllerContextFactory.CreateHttpContext("DELETE", "http://test.com/api/committee/123");

            // Act
            controller.ProcessRequest(context);

            // Assert
            mockDao.Verify(d => d.DeleteCommittee(123), Times.Once);
        }
    }
}
