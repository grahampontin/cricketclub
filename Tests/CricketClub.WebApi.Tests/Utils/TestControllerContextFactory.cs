#nullable disable
using System.Collections.Specialized;
using System.Text;
using CricketClub.WebApi.Controllers;

namespace CricketClub.WebApi.Tests.Utils
{
    public static class TestControllerContextFactory
    {
        public static IHandlerContext CreateHttpContext(string httpMethod, string url, string requestBody = null)
        {
            return new TestHandlerContext(httpMethod, url, requestBody);
        }

        private class TestHandlerContext : IHandlerContext
        {
            public TestHandlerContext(string httpMethod, string url, string requestBody)
            {
                Request = new TestRequestContext(httpMethod, url, requestBody);
                Response = new TestResponseContext();
            }

            public IRequestContext Request { get; }
            public IResponseContext Response { get; }

            private class TestRequestContext : IRequestContext
            {
                private readonly MemoryStream bodyStream;
                private readonly Uri url;
                private readonly NameValueCollection qs;

                public TestRequestContext(string httpMethod, string url, string requestBody)
                {
                    HttpMethod = httpMethod;
                    this.url = new Uri(url);
                    qs = ParseQueryString(this.url.Query);
                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        var bytes = Encoding.UTF8.GetBytes(requestBody);
                        bodyStream = new MemoryStream(bytes);
                    }
                    else
                    {
                        bodyStream = new MemoryStream();
                    }
                }

                private NameValueCollection ParseQueryString(string query)
                {
                    var nvc = new NameValueCollection();
                    if (string.IsNullOrEmpty(query))
                        return nvc;

                    query = query.TrimStart('?');
                    var pairs = query.Split('&');
                    foreach (var pair in pairs)
                    {
                        var parts = pair.Split('=');
                        if (parts.Length == 2)
                        {
                            nvc.Add(Uri.UnescapeDataString(parts[0]), Uri.UnescapeDataString(parts[1]));
                        }
                    }
                    return nvc;
                }

                public string HttpMethod { get; }
                public Stream InputStream => bodyStream;
                public Uri Url => url;
                public NameValueCollection QueryString => qs;
            }

            private class TestResponseContext : IResponseContext
            {
                private readonly StringWriter writer = new StringWriter();
                public string ContentType { get; set; }
                public void Write(string text) => writer.Write(text);
                public int StatusCode { get; set; }
                public void End() { }
                public override string ToString() => writer.ToString();
            }
        }
    }
}
