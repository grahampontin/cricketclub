using CricketClub.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CricketClub.WebApi.Tests.Controllers
{
    public class VersionControllerTests
    {
        private readonly VersionController controller = new VersionController();

        [Fact]
        public void GetVersion_ReturnsOk()
        {
            var result = controller.GetVersion();
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetVersion_WhenEnvVarNotSet_ReturnsUnknown()
        {
            Environment.SetEnvironmentVariable("GIT_HASH", null);
            Environment.SetEnvironmentVariable("ENV_NAME", null);

            var result = controller.GetVersion();

            var ok = Assert.IsType<OkObjectResult>(result);
            var version = Assert.IsType<VersionInfoV1>(ok.Value);
            Assert.Equal("unknown", version.GitHash);
            Assert.Equal("unknown", version.EnvName);
        }

        [Fact]
        public void GetVersion_WhenEnvVarsSet_ReturnsHashAndEnv()
        {
            Environment.SetEnvironmentVariable("GIT_HASH", "abc1234");
            Environment.SetEnvironmentVariable("ENV_NAME", "PROD");
            try
            {
                var result = controller.GetVersion();

                var ok = Assert.IsType<OkObjectResult>(result);
                var version = Assert.IsType<VersionInfoV1>(ok.Value);
                Assert.Equal("abc1234", version.GitHash);
                Assert.Equal("PROD", version.EnvName);
            }
            finally
            {
                Environment.SetEnvironmentVariable("GIT_HASH", null);
                Environment.SetEnvironmentVariable("ENV_NAME", null);
            }
        }
    }
}

