using Masuit.Tools.Mime;
using Xunit;

namespace Masuit.Tools.Abstractions.Tests
{
    public class MimeMapperTests
    {
        private MimeMapper _mimeMapper = new();

        [Fact]
        public void GetMimeFromExtension_ShouldReturnCorrectMimeType()
        {
            // Arrange
            var extension = ".txt";

            // Act
            var mimeType = _mimeMapper.GetMimeFromExtension(extension);

            // Assert
            Assert.Equal("text/plain", mimeType);
        }

        [Fact]
        public void GetMimeFromExtension_ShouldReturnDefaultMimeType_WhenExtensionNotFound()
        {
            // Arrange
            var extension = ".unknown";

            // Act
            var mimeType = _mimeMapper.GetMimeFromExtension(extension);

            // Assert
            Assert.Equal(MimeMapper.DefaultMime, mimeType);
        }

        [Fact]
        public void GetExtensionFromMime_ShouldReturnCorrectExtension()
        {
            // Arrange
            var mimeType = "text/plain";

            // Act
            var extensions = _mimeMapper.GetExtensionFromMime(mimeType);

            // Assert
            Assert.True(extensions.Contains(".txt"));
        }

        [Fact]
        public void GetExtensionFromMime_ShouldReturnEmptyString_WhenMimeTypeNotFound()
        {
            // Arrange
            var mimeType = "unknown/type";

            // Act
            var extension = _mimeMapper.GetExtensionFromMime(mimeType);

            // Assert
            Assert.Equal(extension.Count, 0);
        }

        [Fact]
        public void GetMimeFromPath_ShouldReturnCorrectMimeType()
        {
            // Arrange
            var path = "file.txt";

            // Act
            var mimeType = _mimeMapper.GetMimeFromPath(path);

            // Assert
            Assert.Equal("text/plain", mimeType);
        }

        [Fact]
        public void GetMimeFromPath_ShouldReturnDefaultMimeType_WhenExtensionNotFound()
        {
            // Arrange
            var path = "file.unknown";

            // Act
            var mimeType = _mimeMapper.GetMimeFromPath(path);

            // Assert
            Assert.Equal(MimeMapper.DefaultMime, mimeType);
        }

        [Fact]
        public void Extend_ShouldOverrideDefaultMimeType()
        {
            // Arrange
            var customMapping = new MimeMappingItem { Extension = ".txt", MimeType = "custom/type" };

            // Act
            _mimeMapper.Extend(customMapping);
            var mimeType = _mimeMapper.GetMimeFromExtension(".txt");

            // Assert
            Assert.Equal("custom/type", mimeType);
        }
    }
}