using System;
using System.IO;
using Masuit.Tools.DigtalWatermarker;
using OpenCvSharp;
using Xunit;

namespace Masuit.Tools.DigitalWatermarker.Test;

public class DigitalWatermarkerTests : IDisposable
{
    private readonly string _testImageDirectory;
    private readonly string _sourceImagePath;
    private readonly string _watermarkImagePath;
    private readonly Mat _sourceImage;
    private readonly Mat _watermarkImage;

    public DigitalWatermarkerTests()
    {
        // 创建测试目录
        _testImageDirectory = Path.Combine(Path.GetTempPath(), "DigitalWatermarkerTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testImageDirectory);

        // 创建测试图像文件路径
        _sourceImagePath = Path.Combine(_testImageDirectory, "source.jpg");
        _watermarkImagePath = Path.Combine(_testImageDirectory, "watermark.png");

        // 创建测试用的源图像 (512x512 彩色图像)
        _sourceImage = CreateTestSourceImage();
        
        // 创建测试用的水印图像 (64x64 二值图像)
        _watermarkImage = CreateTestWatermarkImage();

        // 保存测试图像到文件
        Cv2.ImWrite(_sourceImagePath, _sourceImage);
        Cv2.ImWrite(_watermarkImagePath, _watermarkImage);
    }

    public void Dispose()
    {
        _sourceImage?.Dispose();
        _watermarkImage?.Dispose();
        
        // 清理测试文件
        if (Directory.Exists(_testImageDirectory))
        {
            Directory.Delete(_testImageDirectory, true);
        }
    }

    #region 测试Mat对象方法

    [Fact]
    public void EmbedWatermark_WithValidMatObjects_ShouldReturnWatermarkedImage()
    {
        // Act
        using var result = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(_sourceImage, _watermarkImage);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Empty());
        Assert.Equal(_sourceImage.Size(), result.Size());
        Assert.Equal(_sourceImage.Type(), result.Type());
    }

    [Fact]
    public void EmbedWatermark_WithEmptySource_ShouldThrowArgumentException()
    {
        // Arrange
        using var emptyMat = new Mat();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(emptyMat, _watermarkImage));
        Assert.Contains("source is empty", exception.Message);
    }

    [Fact]
    public void EmbedWatermark_WithEmptyWatermark_ShouldThrowArgumentException()
    {
        // Arrange
        using var emptyMat = new Mat();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(_sourceImage, emptyMat));
        Assert.Contains("watermark is empty", exception.Message);
    }

    [Fact]
    public void ExtractWatermark_WithValidImage_ShouldReturnWatermark()
    {
        // Arrange
        using var watermarkedImage = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(_sourceImage, _watermarkImage);

        // Act
        using var extractedWatermark = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.ExtractWatermark(watermarkedImage);

        // Assert
        Assert.NotNull(extractedWatermark);
        Assert.False(extractedWatermark.Empty());
        Assert.Equal(MatType.CV_8U, extractedWatermark.Type());
    }

    [Fact]
    public void ExtractWatermark_WithEmptyImage_ShouldThrowArgumentException()
    {
        // Arrange
        using var emptyMat = new Mat();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.ExtractWatermark(emptyMat));
        Assert.Contains("image is empty", exception.Message);
    }

    #endregion

    #region 测试文件路径方法

    [Fact]
    public void EmbedWatermark_WithValidFilePaths_ShouldReturnWatermarkedImage()
    {
        // Act
        using var result = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(_sourceImagePath, _watermarkImagePath);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Empty());
        Assert.Equal(_sourceImage.Size(), result.Size());
        Assert.Equal(_sourceImage.Type(), result.Type());
    }

    [Fact]
    public void EmbedWatermark_WithNullSourcePath_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(null, _watermarkImagePath));
        Assert.Contains("图片路径不能为空", exception.Message);
    }

    [Fact]
    public void EmbedWatermark_WithEmptySourcePath_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark("", _watermarkImagePath));
        Assert.Contains("图片路径不能为空", exception.Message);
    }

    [Fact]
    public void EmbedWatermark_WithNullWatermarkPath_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(_sourceImagePath, null));
        Assert.Contains("水印图片路径不能为空", exception.Message);
    }

    [Fact]
    public void EmbedWatermark_WithNonExistentSourceFile_ShouldThrowFileNotFoundException()
    {
        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark("nonexistent.jpg", _watermarkImagePath));
        Assert.Contains("文件不存在", exception.Message);
    }

    [Fact]
    public void EmbedWatermark_WithNonExistentWatermarkFile_ShouldThrowFileNotFoundException()
    {
        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(_sourceImagePath, "nonexistent.png"));
        Assert.Contains("文件不存在", exception.Message);
    }

    [Fact]
    public void ExtractWatermark_WithValidFilePath_ShouldReturnWatermark()
    {
        // Arrange
        using var watermarkedImage = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(_sourceImagePath, _watermarkImagePath);
        var watermarkedImagePath = Path.Combine(_testImageDirectory, "watermarked.jpg");
        Cv2.ImWrite(watermarkedImagePath, watermarkedImage);

        // Act
        using var extractedWatermark = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.ExtractWatermark(watermarkedImagePath);

        // Assert
        Assert.NotNull(extractedWatermark);
        Assert.False(extractedWatermark.Empty());
    }

    [Fact]
    public void ExtractWatermark_WithNullPath_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.ExtractWatermark((string)null!));
        Assert.Contains("路径不能为空", exception.Message);
    }

    [Fact]
    public void ExtractWatermark_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.ExtractWatermark("nonexistent.jpg"));
        Assert.Contains("文件不存在", exception.Message);
    }

    #endregion

    #region 测试Stream方法

    [Fact]
    public void EmbedWatermark_WithValidStreams_ShouldReturnWatermarkedImage()
    {
        // Arrange
        using var sourceStream = new MemoryStream();
        using var watermarkStream = new MemoryStream();
        
        Cv2.ImEncode(".jpg", _sourceImage, out var sourceBytes);
        Cv2.ImEncode(".png", _watermarkImage, out var watermarkBytes);
        
        sourceStream.Write(sourceBytes);
        watermarkStream.Write(watermarkBytes);
        
        sourceStream.Position = 0;
        watermarkStream.Position = 0;

        // Act
        using var result = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(sourceStream, watermarkStream);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Empty());
        Assert.Equal(_sourceImage.Size(), result.Size());
        Assert.Equal(_sourceImage.Type(), result.Type());
    }

    [Fact]
    public void EmbedWatermark_WithNullSourceStream_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var watermarkStream = new MemoryStream();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(null, watermarkStream));
    }

    [Fact]
    public void EmbedWatermark_WithNullWatermarkStream_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var sourceStream = new MemoryStream();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(sourceStream, null));
    }

    [Fact]
    public void ExtractWatermark_WithValidStream_ShouldReturnWatermark()
    {
        // Arrange
        using var watermarkedImage = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(_sourceImage, _watermarkImage);
        using var imageStream = new MemoryStream();
        
        Cv2.ImEncode(".jpg", watermarkedImage, out var imageBytes);
        imageStream.Write(imageBytes);
        imageStream.Position = 0;

        // Act
        using var extractedWatermark = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.ExtractWatermark(imageStream);

        // Assert
        Assert.NotNull(extractedWatermark);
        Assert.False(extractedWatermark.Empty());
    }

    [Fact]
    public void ExtractWatermark_WithNullStream_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.ExtractWatermark((Stream)null!));
    }

    [Fact]
    public void ExtractWatermark_WithInvalidImageStream_ShouldThrowArgumentException()
    {
        // Arrange
        using var invalidStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Masuit.Tools.DigtalWatermarker.DigitalWatermarker.ExtractWatermark(invalidStream));
        Assert.Contains("stream不能解析为图像", exception.Message);
    }

    #endregion

    #region 集成测试

    [Fact]
    public void WatermarkRoundTrip_ShouldPreserveWatermarkPattern()
    {
        // Arrange - 创建一个有明确模式的水印
        using var patternWatermark = CreatePatternWatermark();

        // Act - 嵌入并提取水印
        using var watermarkedImage = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(_sourceImage, patternWatermark);
        using var extractedWatermark = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.ExtractWatermark(watermarkedImage);

        // Assert - 验证提取的水印不为空且有合理的尺寸
        Assert.NotNull(extractedWatermark);
        Assert.False(extractedWatermark.Empty());
        Assert.True(extractedWatermark.Rows > 0);
        Assert.True(extractedWatermark.Cols > 0);
    }

    [Fact]
    public void WatermarkShouldBeRobustToJpegCompression()
    {
        // Arrange
        using var watermarkedImage = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.EmbedWatermark(_sourceImage, _watermarkImage);
        
        // 模拟JPEG压缩
        var compressionParams = new int[] { (int)ImwriteFlags.JpegQuality, 75 };
        Cv2.ImEncode(".jpg", watermarkedImage, out var compressedBytes, compressionParams);
        using var compressedImage = Cv2.ImDecode(compressedBytes, ImreadModes.Color);

        // Act
        using var extractedWatermark = Masuit.Tools.DigtalWatermarker.DigitalWatermarker.ExtractWatermark(compressedImage);

        // Assert
        Assert.NotNull(extractedWatermark);
        Assert.False(extractedWatermark.Empty());
    }

    #endregion

    #region 辅助方法

    private static Mat CreateTestSourceImage()
    {
        var image = new Mat(512, 512, MatType.CV_8UC3);
        
        // 创建渐变背景
        for (int y = 0; y < image.Rows; y++)
        {
            for (int x = 0; x < image.Cols; x++)
            {
                byte intensity = (byte)(128 + (x + y) % 128);
                image.Set(y, x, new Vec3b(intensity, (byte)(255 - intensity), (byte)(intensity / 2)));
            }
        }
        
        return image;
    }

    private static Mat CreateTestWatermarkImage()
    {
        var watermark = new Mat(64, 64, MatType.CV_8UC1, Scalar.Black);
        
        // 创建简单的棋盘格模式
        for (int y = 0; y < watermark.Rows; y++)
        {
            for (int x = 0; x < watermark.Cols; x++)
            {
                if ((x / 8 + y / 8) % 2 == 0)
                {
                    watermark.Set(y, x, (byte)255);
                }
            }
        }
        
        return watermark;
    }

    private static Mat CreatePatternWatermark()
    {
        var watermark = new Mat(32, 32, MatType.CV_8UC1, Scalar.Black);
        
        // 创建十字形模式
        int center = 16;
        for (int i = 0; i < 32; i++)
        {
            watermark.Set(center, i, (byte)255); // 水平线
            watermark.Set(i, center, (byte)255); // 垂直线
        }
        
        return watermark;
    }

    #endregion
}
