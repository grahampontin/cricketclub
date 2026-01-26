#nullable disable
using CricketClub.WebApi.Controllers;
using CricketClub.WebApi.Tests.Utils;
using CricketClubDAL;
using Moq;
using Xunit;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class ResultsControllerTests
    {
        private readonly Mock<IDao> mockDao;
        private readonly ResultsController controller;

        public ResultsControllerTests()
        {
            mockDao = new Mock<IDao>();
            controller = new ResultsController(mockDao.Object);
        }

        [Fact]
        public void ProcessRequest_Get_ReturnsResults()
        {
            // Arrange
            mockDao.Setup(d => d.GetAllMatches()).Returns(new List<CricketClubDomain.MatchData>());
            mockDao.Setup(d => d.GetAllMatchReports()).Returns(new Dictionary<int, CricketClubDAL.MatchReportAndConditions>());
            var context = TestControllerContextFactory.CreateHttpContext("GET", "http://test.com/api/results");

            // Act
            controller.ProcessRequest(context);

            // Assert
            Assert.Equal<string>("application/json", context.Response.ContentType);
            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public void ProcessRequest_GetWithSeasonFilter_ReturnsFilteredResults()
        {
            // Arrange
            mockDao.Setup(d => d.GetAllMatches()).Returns(new List<CricketClubDomain.MatchData>());
            mockDao.Setup(d => d.GetAllMatchReports()).Returns(new Dictionary<int, CricketClubDAL.MatchReportAndConditions>());
            var context = TestControllerContextFactory.CreateHttpContext("GET", "http://test.com/api/results?season=2023");

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
            var context = TestControllerContextFactory.CreateHttpContext("POST", "http://test.com/api/results");

            // Act
            controller.ProcessRequest(context);

            // Assert
            Assert.Equal(405, context.Response.StatusCode);
        }
    }
}
