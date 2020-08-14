using System.Collections.Specialized;
using System.IO;
using System.Web;

namespace Masuit.Tools.Test.Mvc.Mocks
{
    public class MockHttpResponse : HttpResponseBase
    {
        public override NameValueCollection Headers => _headers;

        public bool FileTransmitted { get; set; }
        public override bool BufferOutput { get; set; }

        public override System.IO.Stream OutputStream => _stream ?? (_stream = new MemoryStream());

        public override bool IsClientConnected => true;

        public override string ContentType { get; set; }

        public override int StatusCode { get; set; }

        private readonly NameValueCollection _headers = new NameValueCollection();

        private MemoryStream _stream;

        public bool IsClosed;

        public override void AppendHeader(string name, string value)
        {
            AddHeader(name, value);
        }

        public override void AddHeader(string name, string value)
        {
            _headers.Add(name, value);
        }

        public void ClearTestResponse()
        {
            _stream = new MemoryStream();
            Headers.Clear();
            StatusCode = 0;
        }

        public override void Flush()
        {
        }

        public override void Write(string s)
        {
        }

        public override void Close()
        {
            IsClosed = true;
        }

        public override void TransmitFile(string filename)
        {
            FileTransmitted = true;
            var fi = new FileInfo(filename);
            using (var read = fi.OpenRead())
            {
                for (var i = 0; i < fi.Length; i++)
                {
                    OutputStream.WriteByte((byte)read.ReadByte());
                }
            }
        }

        public override void TransmitFile(string filename, long offset, long length)
        {
            var fi = new FileInfo(filename);
            using (var read = fi.OpenRead())
            {
                read.Seek(offset, SeekOrigin.Begin);
                for (var i = 0; i < length; i++)
                {
                    OutputStream.WriteByte((byte)read.ReadByte());
                }
            }
        }
    }
}