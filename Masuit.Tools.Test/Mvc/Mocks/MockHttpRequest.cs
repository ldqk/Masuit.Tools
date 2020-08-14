using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.Routing;

namespace Masuit.Tools.Test.Mvc.Mocks
{
    public class MockHttpRequest : HttpRequestBase
    {
        public override NameValueCollection Headers => _headers;

        public override HttpFileCollectionBase Files { get; }

        public override RequestContext RequestContext => _context;
        public override string ApplicationPath => _applicationPath;

        public override System.IO.Stream InputStream
        {
            get
            {
                if (TestInput != null)
                {
                    var stream = new MemoryStream();
                    var chars = TestInput.ToCharArray();
                    foreach (var c in chars)
                    {
                        stream.WriteByte(Convert.ToByte(c));
                    }
                    return stream;
                }
                return new MemoryStream();
            }
        }

        public override string HttpMethod => TestHttpMethod;
        private readonly NameValueCollection _headers = new NameValueCollection();
        private readonly MockRequestContext _context = new MockRequestContext();
        private string _applicationPath;
        public string TestInput;

        public string TestHttpMethod;

        public MockHttpRequest(MockHttpFilesCollection filesMock)
        {
            Files = filesMock;
        }

        public MockHttpRequest SetHeader(string header, string val)
        {
            _headers[header] = val;
            return this;
        }

        public MockHttpRequest SetApplicationPath(string path)
        {
            _applicationPath = path;
            return this;
        }
    }
}