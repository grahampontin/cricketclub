using System.Text.Json;
using log4net;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi
{
    /// <summary>
    /// Centralized exception handler for the API.
    /// Logs the full exception (including stack trace) and returns RFC7807 ProblemDetails.
    /// </summary>
    public sealed class ExceptionHandlingMiddleware
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ExceptionHandlingMiddleware));
        private readonly RequestDelegate next;
        private readonly IWebHostEnvironment environment;

        public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment environment)
        {
            this.next = next;
            this.environment = environment;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                // Never log secrets; log4net will include exception stack trace.
                Log.Error($"Unhandled exception processing {context.Request.Method} {context.Request.Path}", ex);

                if (context.Response.HasStarted)
                {
                    throw;
                }

                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                    Detail = environment.IsDevelopment() ? ex.Message : null,
                    Instance = context.Request.Path
                };

                problem.Extensions["traceId"] = context.TraceIdentifier;

                var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(json);
            }
        }
    }
}

