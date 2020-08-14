using System.IO;
using System.Web;

namespace Mvc.Stream.Tests.Mocks
{
    public class MockHttpPostedFileBase : HttpPostedFileBase
    {
        public MockHttpPostedFileBase(int contentLen, string fileName, string contentType, System.IO.Stream stream = null)
        {
            ContentLength = contentLen;
            FileName = fileName;
            ContentType = contentType;
            InputStream = stream;
        }

        public override int ContentLength { get; }

        public override string FileName { get; }

        public override System.IO.Stream InputStream { get; }

        public override string ContentType { get; }

        public override void SaveAs(string filename)
        {
            var fileInfo = new FileInfo(filename);
            var directory = new DirectoryInfo(Path.GetDirectoryName(fileInfo.FullName));

            if (!directory.Exists)
            {
                directory.Create();
            }

            using (var file = fileInfo.CreateText())
            {
                file.Write("test");
            }
        }
    }
}
