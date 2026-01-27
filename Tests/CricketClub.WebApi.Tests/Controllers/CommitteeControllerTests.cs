#nullable disable
using CricketClub.WebApi.Controllers;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class CommitteeControllerTests
    {
        private readonly Mock<IDao> _mockDao;
        private readonly CommitteeController _controller;

        public CommitteeControllerTests()
        {
            _mockDao = new Mock<IDao>();
            _controller = new CommitteeController(_mockDao.Object);
        }

        [Fact]
        public void GetAllCommitteeMembers_ReturnsAllMembers()
        {
            // Arrange
            var committeeData = new CommitteeData { Id = 1, Year = 2023, Post = Post.Captain, PlayerId = 1 };
            _mockDao.Setup(d => d.GetAllCommitteeData()).Returns(new List<CommitteeData> { committeeData });

            // Act
            var result = _controller.GetAllCommitteeMembers(null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var members = Assert.IsAssignableFrom<List<CommitteePostV1>>(okResult.Value);
            Assert.Single(members);
            _mockDao.Verify(d => d.GetAllCommitteeData(), Times.Once);
        }

        [Fact]
        public void GetAllCommitteeMembers_WithSeasonFilter_ReturnsFilteredMembers()
        {
            // Arrange
            var members = new List<CommitteeData>
            {
                new CommitteeData { Id = 1, Year = 2023, Post = Post.Captain, PlayerId = 1 },
                new CommitteeData { Id = 2, Year = 2024, Post = Post.ViceCaptain, PlayerId = 2 }
            };
            _mockDao.Setup(d => d.GetAllCommitteeData()).Returns(members);

            // Act
            var result = _controller.GetAllCommitteeMembers(2023, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var filteredMembers = Assert.IsAssignableFrom<List<CommitteePostV1>>(okResult.Value);
            Assert.Single(filteredMembers);
            Assert.Equal(2023, filteredMembers[0].Year);
        }

        [Fact]
        public void GetAllCommitteeMembers_WithYearFilter_ReturnsFilteredMembers()
        {
            // Arrange
            var members = new List<CommitteeData>
            {
                new CommitteeData { Id = 1, Year = 2023, Post = Post.Captain, PlayerId = 1 },
                new CommitteeData { Id = 2, Year = 2024, Post = Post.ViceCaptain, PlayerId = 2 }
            };
            _mockDao.Setup(d => d.GetAllCommitteeData()).Returns(members);

            // Act
            var result = _controller.GetAllCommitteeMembers(null, 2024);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var filteredMembers = Assert.IsAssignableFrom<List<CommitteePostV1>>(okResult.Value);
            Assert.Single(filteredMembers);
            Assert.Equal(2024, filteredMembers[0].Year);
        }

        [Fact]
        public void GetCommitteeMember_WithValidId_ReturnsMember()
        {
            // Arrange
            var committeeData = new CommitteeData { Id = 123, Year = 2023, Post = Post.Captain, PlayerId = 1 };
            _mockDao.Setup(d => d.GetCommitteeData(123)).Returns(committeeData);

            // Act
            var result = _controller.GetCommitteeMember(123);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var member = Assert.IsType<CommitteePostV1>(okResult.Value);
            Assert.Equal(123, member.Id);
            _mockDao.Verify(d => d.GetCommitteeData(123), Times.Once);
        }

        [Fact]
        public void GetCommitteeMember_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockDao.Setup(d => d.GetCommitteeData(999)).Returns((CommitteeData)null);

            // Act
            var result = _controller.GetCommitteeMember(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void CreateCommitteeMember_WithValidData_ReturnsCreatedMember()
        {
            // Arrange
            var newCommittee = new CommitteePostV1 { Year = 2023, Post = "Captain", PlayerId = 1 };
            var createdCommitteeData = new CommitteeData { Id = 1, Year = 2023, Post = Post.Captain, PlayerId = 1 };

            _mockDao.Setup(d => d.CreateNewCommittee(It.IsAny<CommitteeData>())).Returns(1);
            _mockDao.Setup(d => d.GetCommitteeData(1)).Returns(createdCommitteeData);

            // Act
            var result = _controller.CreateCommitteeMember(newCommittee);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(CommitteeController.GetCommitteeMember), createdResult.ActionName);
            Assert.Equal(1, createdResult.RouteValues["id"]);
            var member = Assert.IsType<CommitteePostV1>(createdResult.Value);
            Assert.Equal(1, member.Id);
            _mockDao.Verify(d => d.CreateNewCommittee(It.Is<CommitteeData>(c =>
                c.Year == 2023 &&
                c.Post == Post.Captain &&
                c.PlayerId == 1)), Times.Once);
        }

        [Fact]
        public void UpdateCommitteeMember_WithValidData_ReturnsUpdatedMember()
        {
            // Arrange
            var updateCommittee = new CommitteePostV1 { Id = 1, Year = 2023, Post = "Captain", PlayerId = 1 };
            _mockDao.Setup(d => d.UpdateCommittee(It.IsAny<CommitteeData>()));

            // Act
            var result = _controller.UpdateCommitteeMember(updateCommittee);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var member = Assert.IsType<CommitteePostV1>(okResult.Value);
            Assert.Equal(1, member.Id);
            _mockDao.Verify(d => d.UpdateCommittee(It.Is<CommitteeData>(c =>
                c.Id == 1 &&
                c.Year == 2023 &&
                c.Post == Post.Captain &&
                c.PlayerId == 1)), Times.Once);
        }

        [Fact]
        public void DeleteCommitteeMember_WithValidId_ReturnsNoContent()
        {
            // Arrange
            _mockDao.Setup(d => d.DeleteCommittee(1));

            // Act
            var result = _controller.DeleteCommitteeMember(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockDao.Verify(d => d.DeleteCommittee(1), Times.Once);
        }

        [Fact]
        public void GetAllCommitteeMembers_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _mockDao.Setup(d => d.GetAllCommitteeData()).Returns(new List<CommitteeData>());

            // Act
            var result = _controller.GetAllCommitteeMembers(null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var members = Assert.IsAssignableFrom<List<CommitteePostV1>>(okResult.Value);
            Assert.Empty(members);
        }
    }
}

