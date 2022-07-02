using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Masuit.Tools.Media;

/// <summary>
/// 用于在ImageHashes类的哈希函数中实现图像转换操作。
/// </summary>
public interface IImageTransformer
{
    /// <summary>
    /// 将给定图像转换为8bit色深通道的灰度图像，并将其调整为给定的宽度和高度。在调整大小操作期间，应忽略纵横比。
    /// </summary>
    /// <param name="stream">图像</param>
    /// <param name="width">给定宽度</param>
    /// <param name="height">给定高度</param>
    /// <returns>包含转换图像的8位像素值的字节数组。</returns>
    byte[] TransformImage(Stream stream, int width, int height);

    /// <summary>
    /// 将给定图像转换为8bit色深通道的灰度图像，并将其调整为给定的宽度和高度。在调整大小操作期间，应忽略纵横比。
    /// </summary>
    /// <param name="image">图像</param>
    /// <param name="width">给定宽度</param>
    /// <param name="height">给定高度</param>
    /// <returns>包含转换图像的8位像素值的字节数组。</returns>
    byte[] TransformImage(Image<Rgba32> image, int width, int height);
}
