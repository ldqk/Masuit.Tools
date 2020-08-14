// ReSharper disable InconsistentNaming

using Masuit.Tools.Mvc.Mime;
using NUnit.Framework;

namespace Masuit.Tools.UnitTest.Mvc
{
    [TestFixture]
    public class MimeMapperTests
    {
        private IMimeMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _mapper = new MimeMapper();
        }

        [Test]
        public void GetDefaultExtension()
        {
            Assert.AreEqual("text/plain", _mapper.GetMimeFromExtension("txt"));
        }

        [Test]
        public void Search_Works_For_Extensions_With_Dot_As_Well()
        {
            Assert.IsNotNull(_mapper.GetMimeFromExtension("css"));
            Assert.AreEqual(_mapper.GetMimeFromExtension("css"), _mapper.GetMimeFromExtension(".css"));
        }

        [Test]
        public void It_Returns_Default_Mime_For_Null_In_Extension()
        {
            Assert.AreEqual("application/octet-stream", _mapper.GetMimeFromExtension(null));
        }

        [Test]
        public void It_Returns_Default_Mime_For_Not_Found_Extension()
        {
            Assert.AreEqual("application/octet-stream", _mapper.GetMimeFromExtension("not found"));
        }

        [Test]
        public void It_Searches_In_Full_Path()
        {
            Assert.AreEqual("image/gif", _mapper.GetMimeFromPath("C:\\folder1\\folder2\\text.gif"));
        }

        [Test]
        public void It_Searches_In_Relative_Path()
        {
            Assert.AreEqual("image/gif", _mapper.GetMimeFromPath("..\\..\\..\\text.gif"));
        }

        [Test]
        public void Extension_Overrides_Default_Mime()
        {
            _mapper = new MimeMapper(new MimeMappingItem
            {
                Extension = "txt",
                MimeType = "my own mime type"
            });
            Assert.AreEqual("my own mime type", _mapper.GetMimeFromPath(".txt"));
            Assert.AreEqual("my own mime type", _mapper.GetMimeFromPath("..\\..\\..\\text.txt"));
        }

        [Test]
        public void Search_Works_For_Files_With_Dots_In_Name()
        {
            Assert.AreEqual("text/javascript", _mapper.GetMimeFromPath("jquery.min.js"));
            Assert.AreEqual("text/javascript", _mapper.GetMimeFromPath("http://example.com/jquery.min.js"));
        }

        [Test]
        public void It_Returns_Default_Mime_For_Files_Without_Extension()
        {
            Assert.AreEqual("application/octet-stream", _mapper.GetMimeFromPath("testfile"));
            Assert.AreEqual("application/octet-stream", _mapper.GetMimeFromPath("\\\\network\\share\\testfile"));
        }
    }
}