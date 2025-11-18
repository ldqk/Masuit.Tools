// ReSharper disable InconsistentNaming

using Masuit.Tools.Mime;
using Xunit;

namespace Masuit.Tools.UnitTest.Mvc
{
    public class MimeMapperTests
    {
        private IMimeMapper _mapper;

        public MimeMapperTests()
        {
            _mapper = new MimeMapper();
        }

        [Fact]
        public void GetDefaultExtension()
        {
            Assert.Equal("text/plain", _mapper.GetMimeFromExtension(".txt"));
        }

        [Fact]
        public void It_Returns_Default_Mime_For_Null_In_Extension()
        {
            Assert.Equal("application/octet-stream", _mapper.GetMimeFromExtension(null));
        }

        [Fact]
        public void It_Returns_Default_Mime_For_Not_Found_Extension()
        {
            Assert.Equal("application/octet-stream", _mapper.GetMimeFromExtension("not found"));
        }

        [Fact]
        public void It_Searches_In_Full_Path()
        {
            Assert.Equal("image/gif", _mapper.GetMimeFromPath("C:\\folder1\\folder2\\text.gif"));
        }

        [Fact]
        public void It_Searches_In_Relative_Path()
        {
            Assert.Equal("image/gif", _mapper.GetMimeFromPath("..\\..\\..\\text.gif"));
        }

        [Fact]
        public void Extension_Overrides_Default_Mime()
        {
            _mapper = new MimeMapper(new MimeMappingItem
            {
                Extension = ".txt",
                MimeType = "my own mime type"
            });
            Assert.Equal("my own mime type", _mapper.GetMimeFromExtension(".txt"));
            Assert.Equal("my own mime type", _mapper.GetMimeFromPath("..\\..\\..\\text.txt"));
        }

        [Fact]
        public void Search_Works_For_Files_With_Dots_In_Name()
        {
            Assert.Equal("text/javascript", _mapper.GetMimeFromPath("jquery.min.js"));
            Assert.Equal("text/javascript", _mapper.GetMimeFromPath("http://example.com/jquery.min.js"));
        }

        [Fact]
        public void It_Returns_Default_Mime_For_Files_Without_Extension()
        {
            Assert.Equal("application/octet-stream", _mapper.GetMimeFromPath("testfile"));
            Assert.Equal("application/octet-stream", _mapper.GetMimeFromPath("\\\\network\\share\\testfile"));
        }
    }
}