#nullable disable
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CricketClub.WebApi.Domain;
using CricketClubDAL;
using CricketClubDomain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CricketClub.WebApi.Tests.Integration
{
    public class CommitteeControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IDao> _mockDao;

        public CommitteeControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _mockDao = new Mock<IDao>();
            
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Remove the existing IDao registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDao));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add mock IDao
                    services.AddScoped<IDao>(_ => _mockDao.Object);
                });
            });
        }

        [Fact]
        public async Task PostCommittee_WithRequestBody_ShouldNotThrowSynchronousIOException()
        {
            // Arrange
            var newCommittee = new CommitteePostV1 
            { 
                Year = 2023, 
                Post = "Captain", 
                PlayerId = 1 
            };
            
            var createdCommitteeData = new CommitteeData 
            { 
                Id = 1, 
                Year = 2023, 
                Post = Post.Captain, 
                PlayerId = 1 
            };

            _mockDao.Setup(d => d.CreateNewCommittee(It.IsAny<CommitteeData>())).Returns(1);
            _mockDao.Setup(d => d.GetCommitteeData(1)).Returns(createdCommitteeData);

            var client = _factory.CreateClient();
            var content = new StringContent(
                JsonSerializer.Serialize(newCommittee),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await client.PostAsync("/api/committee", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _mockDao.Verify(d => d.CreateNewCommittee(It.IsAny<CommitteeData>()), Times.Once);
        }

        [Fact]
        public async Task PutCommittee_WithRequestBody_ShouldNotThrowSynchronousIOException()
        {
            // Arrange
            var updateCommittee = new CommitteePostV1 
            { 
                Id = 1, 
                Year = 2023, 
                Post = "Captain", 
                PlayerId = 1 
            };

            _mockDao.Setup(d => d.UpdateCommittee(It.IsAny<CommitteeData>()));

            var client = _factory.CreateClient();
            var content = new StringContent(
                JsonSerializer.Serialize(updateCommittee),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await client.PutAsync("/api/committee", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockDao.Verify(d => d.UpdateCommittee(It.IsAny<CommitteeData>()), Times.Once);
        }

        [Fact]
        public async Task GetAllCommittee_ShouldWorkWithoutRequestBody()
        {
            // Arrange
            var committeeData = new CommitteeData 
            { 
                Id = 1, 
                Year = 2023, 
                Post = Post.Captain, 
                PlayerId = 1 
            };
            
            _mockDao.Setup(d => d.GetAllCommitteeData()).Returns(new List<CommitteeData> { committeeData });

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/committee");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockDao.Verify(d => d.GetAllCommitteeData(), Times.Once);
        }
    }
}
