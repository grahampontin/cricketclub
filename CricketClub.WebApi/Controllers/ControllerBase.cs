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

        public AspNetCoreRequestContext(HttpRequest request)
        {
            this.request = request;
            bodyStream = new MemoryStream();
            request.Body.CopyTo(bodyStream);
            bodyStream.Position = 0;
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
                response.WriteAsync(content).Wait();
            }
        }
    }

    public class AspNetCoreHandlerContext : IHandlerContext
    {
        public AspNetCoreHandlerContext(HttpContext context)
        {
            Request = new AspNetCoreRequestContext(context.Request);
            Response = new AspNetCoreResponseContext(context.Response);
        }

        public IRequestContext Request { get; }
        public IResponseContext Response { get; }
    }

    [ApiController]
    public abstract class ControllerBase : Controller
    {
        protected IActionResult ProcessRequest()
        {
            var wrapped = new AspNetCoreHandlerContext(HttpContext);
            ProcessRequest(wrapped);
            wrapped.Response.End();
            return new EmptyResult();
        }

        public abstract void ProcessRequest(IHandlerContext context);
    }
}
