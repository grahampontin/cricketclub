#nullable disable
using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc;

namespace CricketClub.WebApi.Controllers
{
    public interface IRequestContext
    {
        string HttpMethod { get; }
        Stream InputStream { get; }
        Uri Url { get; }
        NameValueCollection QueryString { get; }
    }

    public interface IResponseContext
    {
        string ContentType { get; set; }
        void Write(string text);
        int StatusCode { get; set; }
        void End();
    }

    public interface IHandlerContext
    {
        IRequestContext Request { get; }
        IResponseContext Response { get; }
    }

    public class AspNetCoreRequestContext : IRequestContext
    {
        private readonly HttpRequest request;
        private readonly MemoryStream bodyStream;

        private AspNetCoreRequestContext(HttpRequest request, MemoryStream bodyStream)
        {
            this.request = request;
            this.bodyStream = bodyStream;
        }

        public static async Task<AspNetCoreRequestContext> CreateAsync(HttpRequest request)
        {
            var bodyStream = new MemoryStream();
            await request.Body.CopyToAsync(bodyStream);
            bodyStream.Position = 0;
            return new AspNetCoreRequestContext(request, bodyStream);
        }

        public string HttpMethod => request.Method;
        public Stream InputStream => bodyStream;
        public Uri Url => new Uri($"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}");
        public NameValueCollection QueryString
        {
            get
            {
                var nvc = new NameValueCollection();
                foreach (var key in request.Query.Keys)
                {
                    nvc.Add(key, request.Query[key]);
                }
                return nvc;
            }
        }
    }

    public class AspNetCoreResponseContext : IResponseContext
    {
        private readonly HttpResponse response;
        private readonly StringWriter writer;

        public AspNetCoreResponseContext(HttpResponse response)
        {
            this.response = response;
            writer = new StringWriter();
        }

        public string ContentType
        {
            get => response.ContentType;
            set => response.ContentType = value;
        }

        public void Write(string text) => writer.Write(text);

        public int StatusCode
        {
            get => response.StatusCode;
            set => response.StatusCode = value;
        }

        public void End()
        {
            var content = writer.ToString();
            if (!string.IsNullOrEmpty(content))
            {
                response.WriteAsync(content).GetAwaiter().GetResult();
            }
        }
    }

    public class AspNetCoreHandlerContext : IHandlerContext
    {
        private AspNetCoreHandlerContext(IRequestContext request, IResponseContext response)
        {
            Request = request;
            Response = response;
        }

        public static async Task<AspNetCoreHandlerContext> CreateAsync(HttpContext context)
        {
            var request = await AspNetCoreRequestContext.CreateAsync(context.Request);
            var response = new AspNetCoreResponseContext(context.Response);
            return new AspNetCoreHandlerContext(request, response);
        }

        public IRequestContext Request { get; }
        public IResponseContext Response { get; }
    }

    [ApiController]
    public abstract class ControllerBase : Controller
    {
        protected async Task<IActionResult> ProcessRequest()
        {
            var wrapped = await AspNetCoreHandlerContext.CreateAsync(HttpContext);
            ProcessRequest(wrapped);
            wrapped.Response.End();
            return new EmptyResult();
        }

        public abstract void ProcessRequest(IHandlerContext context);
    }
}
