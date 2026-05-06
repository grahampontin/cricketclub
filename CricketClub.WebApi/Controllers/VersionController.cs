using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    /// <summary>
    /// Returns build/version metadata for the running API instance.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VersionController : ControllerBase
    {
        /// <summary>
        /// Returns the git commit hash the API was built from.
        /// The value is baked in at Docker build time via the GIT_HASH build arg.
        /// Returns "unknown" when running outside of a container build (e.g. local dev).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(VersionInfoV1), StatusCodes.Status200OK)]
        public IActionResult GetVersion()
        {
            var hash = Environment.GetEnvironmentVariable("GIT_HASH") ?? "unknown";
            var env  = Environment.GetEnvironmentVariable("ENV_NAME") ?? "unknown";
            return Ok(new VersionInfoV1 { GitHash = hash, EnvName = env });
        }
    }

    /// <summary>Response DTO for the version endpoint.</summary>
    public class VersionInfoV1
    {
        public string GitHash { get; set; } = string.Empty;
        public string EnvName { get; set; } = string.Empty;
    }
}

