using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Masuit.Tools.Core.Test.AspNetCore
{
    public class FileContentsTests : TestBase
    {
        [Fact]
        public async Task FullFileContentsAttachmentEtagTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/content/true/true");
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
        public async Task FullFileContentsAttachmentNoEtagTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/content/true/false");
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
        public async Task FullFileContentsInlineEtagTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/content/false/true");
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
        public async Task FullFileContentsInlineNoEtagTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/content/false/false");
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
        public async Task FullFileContentsInlineFileNameTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/content/false");
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
        public async Task Partial1FileContentsAttachmentEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=0-0");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/content/true/true");
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
        public async Task Partial1FileContentsAttachmentNoEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=0-0");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/content/true/false");
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
        public async Task Partial2FileContentsAttachmentEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/content/true/true");
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
        public async Task Partial2FileContentsAttachmentNoEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/content/true/false");
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
        public async Task Partial2FileContentsInlineEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/content/false/true");
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
        public async Task Partial2FileContentsInlineNoEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/content/false/false");
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