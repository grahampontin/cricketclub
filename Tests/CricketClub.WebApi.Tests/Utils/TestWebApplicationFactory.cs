using CricketClubDAL;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace CricketClub.WebApi.Tests.Utils
{
    public static class TestWebApplicationFactory
    {
        public static WebApplicationFactory<Program> WithDao(this WebApplicationFactory<Program> factory, IDao dao)
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDao));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddScoped(_ => dao);
                });
            });
        }
    }
}
