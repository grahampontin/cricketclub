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
    public class AwardsControllerTests
    {
        private readonly Mock<IDao> _mockDao;
        private readonly AwardsController _controller;

        public AwardsControllerTests()
        {
            _mockDao = new Mock<IDao>();
            _controller = new AwardsController(_mockDao.Object);
        }

        [Fact]
        public void GetAllAwards_ReturnsAllAwards()
        {
            // Arrange
            var awardData = new AwardData 
            { 
                Id = 1, 
                Year = 2023, 
                Award = Award.BatsmanOfTheYear, 
                PlayerId = 1, 
                PlayerName = "Test Player",
                Data = "Test" 
            };
            _mockDao.Setup(d => d.GetAllAwardsData()).Returns(new List<AwardData> { awardData });

            // Act
            var result = _controller.GetAllAwards(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var awards = Assert.IsAssignableFrom<List<AwardV1>>(okResult.Value);
            Assert.Single(awards);
            Assert.Equal(1, awards[0].Id);
            _mockDao.Verify(d => d.GetAllAwardsData(), Times.Once);
        }

        [Fact]
        public void GetAllAwards_WithSeasonFilter_ReturnsFilteredAwards()
        {
            // Arrange
            var awards = new List<AwardData>
            {
                new AwardData { Id = 1, Year = 2023, Award = Award.BatsmanOfTheYear, PlayerId = 1, PlayerName = "Player 1", Data = "Test" },
                new AwardData { Id = 2, Year = 2024, Award = Award.BowlerOfTheYear, PlayerId = 2, PlayerName = "Player 2", Data = "Test" }
            };
            _mockDao.Setup(d => d.GetAllAwardsData()).Returns(awards);

            // Act
            var result = _controller.GetAllAwards(2023);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var filteredAwards = Assert.IsAssignableFrom<List<AwardV1>>(okResult.Value);
            Assert.Single(filteredAwards);
            Assert.Equal(2023, filteredAwards[0].Year);
        }

        [Fact]
        public void GetAward_WithValidId_ReturnsAward()
        {
            // Arrange
            var awardData = new AwardData 
            { 
                Id = 123, 
                Year = 2023, 
                Award = Award.BowlerOfTheYear, 
                PlayerId = 1, 
                PlayerName = "Test Player",
                Data = "Test" 
            };
            _mockDao.Setup(d => d.GetAwardData(123)).Returns(awardData);

            // Act
            var result = _controller.GetAward(123);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var award = Assert.IsType<AwardV1>(okResult.Value);
            Assert.Equal(123, award.Id);
            Assert.Equal("Test Player", award.PlayerName);
            _mockDao.Verify(d => d.GetAwardData(123), Times.Once);
        }

        [Fact]
        public void GetAward_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockDao.Setup(d => d.GetAwardData(999)).Returns((AwardData)null);

            // Act
            var result = _controller.GetAward(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void CreateAward_WithValidData_ReturnsCreatedAward()
        {
            // Arrange
            var newAward = new AwardV1 
            { 
                Year = 2023, 
                Award = "BatsmanOfTheYear", 
                PlayerId = 1, 
                Data = "Test" 
            };
            var createdAwardData = new AwardData 
            { 
                Id = 1, 
                Year = 2023, 
                Award = Award.BatsmanOfTheYear, 
                PlayerId = 1, 
                PlayerName = "Test Player",
                Data = "Test" 
            };
            
            _mockDao.Setup(d => d.CreateNewAward(Award.BatsmanOfTheYear, 2023, 1, "Test")).Returns(1);
            _mockDao.Setup(d => d.GetAwardData(1)).Returns(createdAwardData);

            // Act
            var result = _controller.CreateAward(newAward);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(AwardsController.GetAward), createdResult.ActionName);
            Assert.Equal(1, createdResult.RouteValues["id"]);
            var award = Assert.IsType<AwardV1>(createdResult.Value);
            Assert.Equal(1, award.Id);
            Assert.Equal("Test Player", award.PlayerName);
            _mockDao.Verify(d => d.CreateNewAward(Award.BatsmanOfTheYear, 2023, 1, "Test"), Times.Once);
        }

        [Fact]
        public void UpdateAward_WithValidData_ReturnsUpdatedAward()
        {
            // Arrange
            var updateAward = new AwardV1 
            { 
                Id = 1, 
                Year = 2023, 
                Award = "BatsmanOfTheYear", 
                PlayerId = 1, 
                Data = "Updated" 
            };
            
            _mockDao.Setup(d => d.UpdateAward(It.IsAny<AwardData>()));

            // Act
            var result = _controller.UpdateAward(updateAward);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var award = Assert.IsType<AwardV1>(okResult.Value);
            Assert.Equal(1, award.Id);
            Assert.Equal("Updated", award.Data);
            _mockDao.Verify(d => d.UpdateAward(It.Is<AwardData>(a =>
                a.Id == 1 && 
                a.Year == 2023 && 
                a.Award == Award.BatsmanOfTheYear && 
                a.PlayerId == 1 && 
                a.Data == "Updated")), Times.Once);
        }

        [Fact]
        public void DeleteAward_WithValidId_ReturnsNoContent()
        {
            // Arrange
            _mockDao.Setup(d => d.DeleteAward(1));

            // Act
            var result = _controller.DeleteAward(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockDao.Verify(d => d.DeleteAward(1), Times.Once);
        }

        [Fact]
        public void GetAllAwards_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _mockDao.Setup(d => d.GetAllAwardsData()).Returns(new List<AwardData>());

            // Act
            var result = _controller.GetAllAwards(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var awards = Assert.IsAssignableFrom<List<AwardV1>>(okResult.Value);
            Assert.Empty(awards);
        }

        [Fact]
        public void CreateAward_WithInvalidAwardType_ThrowsException()
        {
            // Arrange
            var newAward = new AwardV1 
            { 
                Year = 2023, 
                Award = "InvalidAwardType", 
                PlayerId = 1, 
                Data = "Test" 
            };

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => _controller.CreateAward(newAward));
        }
    }
}

