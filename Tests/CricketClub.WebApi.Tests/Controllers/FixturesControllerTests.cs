#nullable disable
using CricketClub.WebApi.Controllers;
using CricketClub.WebApi.Tests.Utils;
using CricketClubDAL;
using Moq;
using Xunit;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class FixturesControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly FixturesController controller;

        public FixturesControllerTests()
        {
            mockDao = new Mock<IDao>();
            controller = new FixturesController(mockDao.Object);
        }

        [Fact]
        public void ProcessRequest_Get_ReturnsFixtures()
        {
            // Arrange
            var context = TestControllerContextFactory.CreateHttpContext("GET", "http://test.com/api/fixtures");

            // Act
            controller.ProcessRequest(context);

            // Assert
            Assert.Equal<string>("application/json", context.Response.ContentType);
            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public void ProcessRequest_GetWithSeasonFilter_ReturnsFilteredFixtures()
        {
            // Arrange
            var context = TestControllerContextFactory.CreateHttpContext("GET", "http://test.com/api/fixtures?season=2023");

            // Act
            controller.ProcessRequest(context);

            // Assert
            Assert.Equal<string>("application/json", context.Response.ContentType);
            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public void ProcessRequest_Post_ReturnsMethodNotAllowed()
        {
            // Arrange
            var context = TestControllerContextFactory.CreateHttpContext("POST", "http://test.com/api/fixtures");

            // Act
            controller.ProcessRequest(context);

            // Assert
            Assert.Equal(405, context.Response.StatusCode);
        }
    }
}
