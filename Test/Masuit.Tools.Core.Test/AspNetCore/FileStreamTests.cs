using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Masuit.Tools.Core.Test.AspNetCore
{
    public class FileStreamTests : TestBase
    {
        [Fact]
        public async Task FullFileStreamAttachmentEtagTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/stream/true/true");
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
        public async Task FullFileStreamAttachmentNoEtagTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/stream/true/false");
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
        public async Task FullFileStreamInlineEtagTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/stream/false/true");
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
        public async Task FullFileStreamInlineFileNameTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/stream/false");
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
        public async Task FullFileStreamInlineNoEtagTest()
        {
            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/stream/false/false");
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
        public async Task Partial1FileStreamAttachmentEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=0-0");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/stream/true/true");
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
        public async Task Partial1FileStreamAttachmentNoEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=0-0");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/stream/true/false");
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
        public async Task Partial2FileStreamAttachmentEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/stream/true/true");
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
        public async Task Partial2FileStreamAttachmentNoEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/stream/true/false");
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
        public async Task Partial2FileStreamInlineEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/stream/false/true");
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
        public async Task Partial2FileStreamInlineNoEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/stream/false/false");
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