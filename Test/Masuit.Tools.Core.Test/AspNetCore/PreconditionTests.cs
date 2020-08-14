using Microsoft.Net.Http.Headers;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Masuit.Tools.Core.Test.AspNetCore
{
    public class PreconditionTests : TestBase
    {
        /// <summary>
        /// The precondition if match fail test.
        /// </summary>
        [Fact]
        public async Task PreconditionIfMatchFailTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Match", "\"xyzzy\"");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.Equal(string.Empty, responseString);
            Assert.NotEqual("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfMatchEmptySuccessTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Match", "\"xyzzy\"");

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/false");

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("0123456789abcdefghijklmnopgrstuvwxyzABCDEFGHIJKLMNOPGRSTUVWXYZ", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Null(response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfMatchFailWeakTest()
        {
            // Arrange
            string entityTag = EntityTag.ToString();
            var tmpNewEntityTag = new EntityTagHeaderValue(entityTag, true);
            Client.DefaultRequestHeaders.Add("If-Match", tmpNewEntityTag.ToString());

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.Equal(string.Empty, responseString);
            Assert.NotEqual("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfMatchSuccessAnyTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Match", EntityTagHeaderValue.Any.ToString());

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
        }

        [Fact]
        public async Task PreconditionIfMatchSuccessTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Match", EntityTag.ToString());

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
        }

        [Fact]
        public async Task PreconditionIfModifiedSinceFailEqualTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Modified-Since", HeaderUtilities.FormatDate(LastModified));

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            Assert.Equal(string.Empty, responseString);
            Assert.NotEqual("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfModifiedSinceFailLessTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Modified-Since", HeaderUtilities.FormatDate(LastModified.AddSeconds(1)));

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            Assert.Equal(string.Empty, responseString);
            Assert.NotEqual("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfModifiedSinceIfNoneMatchSuccessTest()
        {
            // Arrange
            var tmpNewEntityTag = new EntityTagHeaderValue("\"xyzzy\"", true);
            Client.DefaultRequestHeaders.Add("If-None-Match", tmpNewEntityTag.ToString());
            Client.DefaultRequestHeaders.Add("If-Modified-Since", HeaderUtilities.FormatDate(LastModified.AddSeconds(1)));

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
        }

        [Fact]
        public async Task PreconditionIfModifiedSinceSuccessLessTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Modified-Since", HeaderUtilities.FormatDate(LastModified.AddSeconds(-1)));

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("0123456789abcdefghijklmnopgrstuvwxyzABCDEFGHIJKLMNOPGRSTUVWXYZ", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfModifiedSinceSuccessPutTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Modified-Since", HeaderUtilities.FormatDate(LastModified.AddSeconds(-1)));

            // Act
            HttpResponseMessage response = await Client.PutAsync("/file/file", new StringContent(string.Empty));
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("0123456789abcdefghijklmnopgrstuvwxyzABCDEFGHIJKLMNOPGRSTUVWXYZ", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfNoneMatchFailAnyGetTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-None-Match", EntityTagHeaderValue.Any.ToString());

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            Assert.Equal(string.Empty, responseString);
            Assert.NotEqual("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfNoneMatchFailAnyPutTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-None-Match", EntityTagHeaderValue.Any.ToString());

            // Act
            HttpResponseMessage response = await Client.PutAsync("/file/file", new StringContent(string.Empty));

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            Assert.Equal(string.Empty, responseString);
            Assert.NotEqual("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfNoneMatchGetFailTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-None-Match", EntityTag.ToString());

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            Assert.Equal(string.Empty, responseString);
            Assert.NotEqual("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfNoneMatchGetWeakSuccessTest()
        {
            // Arrange
            string entityTag = EntityTag.ToString();
            var tmpNewEntityTag = new EntityTagHeaderValue(entityTag, true);
            Client.DefaultRequestHeaders.Add("If-None-Match", tmpNewEntityTag.ToString());

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            Assert.Equal(string.Empty, responseString);
            Assert.NotEqual("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfNoneMatchHeadFailTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-None-Match", EntityTag.ToString());

            // Act
            HttpResponseMessage response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/file/file"));

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            Assert.Equal(string.Empty, responseString);
            Assert.NotEqual("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfNoneMatchPutFailTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-None-Match", EntityTag.ToString());

            // Act
            HttpResponseMessage response = await Client.PutAsync("/file/file", new StringContent(string.Empty));

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
            Assert.Equal(string.Empty, responseString);
            Assert.NotEqual("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfNoneMatchSuccessTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-None-Match", "\"xyzzy\"");

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
        }

        [Fact]
        public async Task PreconditionIfNoneMatchSuccessWeakTest()
        {
            // Arrange
            var tmpNewEntityTag = new EntityTagHeaderValue("\"xyzzy\"", true);
            Client.DefaultRequestHeaders.Add("If-None-Match", tmpNewEntityTag.ToString());

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
        }

        [Fact]
        public async Task PreconditionIfRangeEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");
            Client.DefaultRequestHeaders.Add("If-Range", EntityTag.ToString());

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
        }

        [Fact]
        public async Task PreconditionIfRangeIgnoreEtagEmptyTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");
            Client.DefaultRequestHeaders.Add("If-Range", EntityTag.ToString());

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/false");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal("1", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Null(response.Headers.ETag);
        }

        [Fact]
        public async Task PreconditionIfRangeIgnoreEtagTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");
            Client.DefaultRequestHeaders.Add("If-Range", "\"xyzzy\"");

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
        }

        [Fact]
        public async Task PreconditionIfRangeIgnoreTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Range", "\"xyzzy\"");

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
        }

        [Fact]
        public async Task PreconditionIfRangeIgnoreWeakEtagTest()
        {
            // Arrange
            var tmpNewEntityTag = new EntityTagHeaderValue(EntityTag.ToString(), true);
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");
            Client.DefaultRequestHeaders.Add("If-Range", tmpNewEntityTag.ToString());

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
        }

        [Fact]
        public async Task PreconditionIfRangeLastModifiedEqualTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");
            Client.DefaultRequestHeaders.Add("If-Range", HeaderUtilities.FormatDate(LastModified));

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
        }

        [Fact]
        public async Task PreconditionIfRangeLastModifiedIgnoreTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");
            Client.DefaultRequestHeaders.Add("If-Range", HeaderUtilities.FormatDate(LastModified.AddSeconds(-1)));

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
        }

        [Fact]
        public async Task PreconditionIfRangeLastModifiedLessTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("Range", "bytes=1-1");
            Client.DefaultRequestHeaders.Add("If-Range", HeaderUtilities.FormatDate(LastModified.AddSeconds(1)));

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            // Assert
            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal("1", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.NotNull(response.Content.Headers.ContentRange);
            Assert.Equal("bytes 1-1/62", response.Content.Headers.ContentRange.ToString());
        }

        [Fact]
        public async Task PreconditionIfUnmodifiedSinceFailTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Unmodified-Since", HeaderUtilities.FormatDate(LastModified.AddSeconds(-1)));

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.Equal(string.Empty, responseString);
            Assert.NotEqual("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfUnmodifiedSinceIfMatchFailTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Match", EntityTag.ToString());
            Client.DefaultRequestHeaders.Add("If-Unmodified-Since", HeaderUtilities.FormatDate(LastModified.AddSeconds(-1)));

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.PreconditionFailed, response.StatusCode);
            Assert.Equal(string.Empty, responseString);
            Assert.NotEqual("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfUnmodifiedSinceSuccessEqualTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Unmodified-Since", HeaderUtilities.FormatDate(LastModified));

            // Act
            HttpResponseMessage response = await Client.GetAsync("/file/physical/true/true");

            string responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("0123456789abcdefghijklmnopgrstuvwxyzABCDEFGHIJKLMNOPGRSTUVWXYZ", responseString);
            Assert.Equal("bytes", response.Headers.AcceptRanges.ToString());
            Assert.Equal(EntityTag, response.Headers.ETag);
            Assert.Null(response.Content.Headers.ContentRange);
        }

        [Fact]
        public async Task PreconditionIfUnmodifiedSinceSuccessLessTest()
        {
            // Arrange
            Client.DefaultRequestHeaders.Add("If-Unmodified-Since", HeaderUtilities.FormatDate(LastModified.AddSeconds(1)));

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
        }
    }
}