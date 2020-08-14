using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Masuit.Tools.Core.Test.AspNetCore
{
    public class PhysicalFileTests : TestBase
    {
        [Fact]
        public async Task FullPhysicalFileAttachmentEtagTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("0123456789abcdefghijklmnopgrstuvwxyzABCDEFGHIJKLMNOPGRSTUVWXYZ", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
            Assert.Equal(62, response.Content.Headers.ContentLength);
            Assert.Equal("attachment", response.Content.Headers.ContentDisposition.DispositionType);
        }

        [Fact]
        public async Task FullPhysicalFileAttachmentNoEtagTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/false");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("0123456789abcdefghijklmnopgrstuvwxyzABCDEFGHIJKLMNOPGRSTUVWXYZ", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Null(response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
            Assert.Equal(62, response.Content.Headers.ContentLength);
            Assert.Equal("attachment", response.Content.Headers.ContentDisposition.DispositionType);
        }

        [Fact]
        public async Task FullPhysicalFileInlineEtagTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/false/true");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("0123456789abcdefghijklmnopgrstuvwxyzABCDEFGHIJKLMNOPGRSTUVWXYZ", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
            Assert.Equal(62, response.Content.Headers.ContentLength);
            Assert.Equal("inline", response.Content.Headers.ContentDisposition.DispositionType);
        }

        [Fact]
        public async Task FullPhysicalFileInlineFileNameTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/false");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("0123456789abcdefghijklmnopgrstuvwxyzABCDEFGHIJKLMNOPGRSTUVWXYZ", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Null(response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
            Assert.Equal(62, response.Content.Headers.ContentLength);
            Assert.Equal("inline", response.Content.Headers.ContentDisposition.DispositionType);
            Assert.Equal("TestFile.txt", response.Content.Headers.ContentDisposition.FileName);
        }

        [Fact]
        public async Task FullPhysicalFileInlineNoEtagTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/false/false");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("0123456789abcdefghijklmnopgrstuvwxyzABCDEFGHIJKLMNOPGRSTUVWXYZ", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Null(response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
            Assert.Equal(62, response.Content.Headers.ContentLength);
            Assert.Equal("inline", response.Content.Headers.ContentDisposition.DispositionType);
        }

        [Fact]
        public async Task Partial1PhysicalFileAttachmentEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=0-0");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal("0", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.NotNull(response.Content.Headers.ContentRange);
            Assert.Equal("bytes 0-0/62", response.Content.Headers.ContentRange.ToString());
            Assert.Equal(1, response.Content.Headers.ContentLength);
            Assert.Equal("attachment", response.Content.Headers.ContentDisposition.DispositionType);
        }

        [Fact]
        public async Task Partial1PhysicalFileAttachmentNoEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=0-0");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/false");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal("0", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Null(response.Headers.ETag);
            Assert.NotNull(response.Content.Headers.ContentRange);
            Assert.Equal("bytes 0-0/62", response.Content.Headers.ContentRange.ToString());
            Assert.Equal(1, response.Content.Headers.ContentLength);
            Assert.Equal("attachment", response.Content.Headers.ContentDisposition.DispositionType);
        }

        [Fact]
        public async Task Partial2PhysicalFileAttachmentEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal("1", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.NotNull(response.Content.Headers.ContentRange);
            Assert.Equal("bytes 1-1/62", response.Content.Headers.ContentRange.ToString());
            Assert.Equal(1, response.Content.Headers.ContentLength);
            Assert.Equal("attachment", response.Content.Headers.ContentDisposition.DispositionType);
        }

        [Fact]
        public async Task Partial2PhysicalFileAttachmentNoEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/false");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal("1", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Null(response.Headers.ETag);
            Assert.NotNull(response.Content.Headers.ContentRange);
            Assert.Equal("bytes 1-1/62", response.Content.Headers.ContentRange.ToString());
            Assert.Equal(1, response.Content.Headers.ContentLength);
            Assert.Equal("attachment", response.Content.Headers.ContentDisposition.DispositionType);
        }

        [Fact]
        public async Task Partial2PhysicalFileInlineEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/false/true");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal("1", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.NotNull(response.Content.Headers.ContentRange);
            Assert.Equal("bytes 1-1/62", response.Content.Headers.ContentRange.ToString());
            Assert.Equal(1, response.Content.Headers.ContentLength);
            Assert.Equal("inline", response.Content.Headers.ContentDisposition.DispositionType);
        }

        [Fact]
        public async Task Partial2PhysicalFileInlineNoEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/false/false");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal("1", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Null(response.Headers.ETag);
            Assert.NotNull(response.Content.Headers.ContentRange);
            Assert.Equal("bytes 1-1/62", response.Content.Headers.ContentRange.ToString());
            Assert.Equal(1, response.Content.Headers.ContentLength);
            Assert.Equal("inline", response.Content.Headers.ContentDisposition.DispositionType);
        }
    }
}