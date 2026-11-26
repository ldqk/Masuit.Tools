using SkiaSharp;
using System.IO;

namespace Masuit.Tools.Media;

/// <summary>
/// 使用SkiaSharp进行图像变换
/// </summary>
public class SkiaSharpTransformer : IImageTransformer
{
    public byte[] TransformImage(Stream stream, int width, int height)
    {
        using var codec = SKCodec.Create(stream);
        using var original = SKBitmap.Decode(codec);
        if (original == null) return Array.Empty<byte>();
        return TransformImage(original, width, height);
    }

    public byte[] TransformImage(SKBitmap image, int width, int height)
    {
        var imageInfo = new SKImageInfo(width, height, SKColorType.Gray8, SKAlphaType.Opaque);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // 源图片完整矩形
        SKRect sourceRect = new SKRect(0, 0, image.Width, image.Height);
        SKRect destRect = new SKRect(0, 0, width, height);

        // 绘制：不做比例适配，直接拉伸填充
        canvas.DrawBitmap(image, sourceRect, destRect,new SKSamplingOptions(SKFilterMode.Linear));
        using var pixels = surface.PeekPixels();
        return pixels.GetPixelSpan().ToArray();
    }

    public byte[,] GetPixelData(SKBitmap image, int width, int height)
    {
        var imageInfo = new SKImageInfo(width, height, SKColorType.Gray8, SKAlphaType.Opaque);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // 源图片完整矩形
        SKRect sourceRect = new SKRect(0, 0, image.Width, image.Height);
        SKRect destRect = new SKRect(0, 0, width, height);

        // 绘制：不做比例适配，直接拉伸填充
        canvas.DrawBitmap(image, sourceRect, destRect, new SKSamplingOptions(SKFilterMode.Linear));
        var grayscalePixels = new byte[width, height];
        unsafe
        {
            var src = (byte*)surface.PeekPixels().GetPixels().ToPointer();
            fixed (byte* dst = grayscalePixels)
                Buffer.MemoryCopy(src, dst, width * height, width * height);
        }
        return grayscalePixels;
    }
}

public static class ImageHashExt
{
    private static readonly ImageHasher Hasher = new();

    public static ulong AverageHash64(this SKBitmap image) => Hasher.AverageHash64(image);
    public static ulong MedianHash64(this SKBitmap image) => Hasher.MedianHash64(image);
    public static ulong[] MedianHash256(this SKBitmap image) => Hasher.MedianHash256(image);
    public static ulong DifferenceHash64(this SKBitmap image) => Hasher.DifferenceHash64(image);
    public static ulong[] DifferenceHash256(this SKBitmap image) => Hasher.DifferenceHash256(image);
    public static ulong DctHash(this SKBitmap image) => Hasher.DctHash(image);
    public static ulong DctHash64(this SKBitmap image) => Hasher.DctHash64(image);
    public static ulong[] DctHash256(this SKBitmap image) => Hasher.DctHash256(image);
    public static ulong[] MHHash256(this SKBitmap image) => Hasher.MHHash256(image);
    /// <summary>
    /// 使用PDQ算法计算图像的256位哈希。性能比其他哈希慢一些，因为它为保持原有预处理需要处理原图，性能大约是其他哈希的1/5，适用于对图像进行高精度相似度比较。
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static ulong[] PDQHash256(this SKBitmap image) => Hasher.PDQHash256(image);
    /// <summary>
    /// 使用PDQ算法计算图像的256位哈希。性能比其他哈希慢一些，因为它为保持原有预处理需要处理原图，性能大约是其他哈希的1/5，适用于对图像进行高精度相似度比较。
    /// </summary>
    /// <param name="image"></param>
    /// <param name="quality"></param>
    /// <returns></returns>
    public static ulong[] PDQHash256(this SKBitmap image, out int quality) => Hasher.PDQHash256(image, out quality);

    public static float Compare(this SKBitmap image1, SKBitmap image2, ImageHashAlgorithm algorithm = ImageHashAlgorithm.Difference)
    {
        return algorithm switch
        {
            ImageHashAlgorithm.Average => ImageHasher.Compare(Hasher.AverageHash64(image1), Hasher.AverageHash64(image2)),
            ImageHashAlgorithm.Medium => ImageHasher.Compare(Hasher.MedianHash256(image1), Hasher.MedianHash256(image2)),
            ImageHashAlgorithm.Difference => ImageHasher.Compare(Hasher.DifferenceHash256(image1), Hasher.DifferenceHash256(image2)),
            ImageHashAlgorithm.DCT32 => ImageHasher.Compare(Hasher.DctHash(image1), Hasher.DctHash(image2)),
            ImageHashAlgorithm.DCT64 => ImageHasher.Compare(Hasher.DctHash64(image1), Hasher.DctHash64(image2)),
            ImageHashAlgorithm.DCT256 => ImageHasher.Compare(Hasher.DctHash256(image1), Hasher.DctHash256(image2)),
            ImageHashAlgorithm.MHHash256 => ImageHasher.Compare(Hasher.MHHash256(image1), Hasher.MHHash256(image2)),
            ImageHashAlgorithm.PDQ256 => ImageHasher.Compare(Hasher.PDQHash256(image1), Hasher.PDQHash256(image2)),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };
    }

    public static float Compare(this SKBitmap image1, string image2path, ImageHashAlgorithm algorithm = ImageHashAlgorithm.Difference)
    {
        return algorithm switch
        {
            ImageHashAlgorithm.Average => ImageHasher.Compare(Hasher.AverageHash64(image1), Hasher.AverageHash64(image2path)),
            ImageHashAlgorithm.Medium => ImageHasher.Compare(Hasher.MedianHash256(image1), Hasher.MedianHash256(image2path)),
            ImageHashAlgorithm.Difference => ImageHasher.Compare(Hasher.DifferenceHash256(image1), Hasher.DifferenceHash256(image2path)),
            ImageHashAlgorithm.DCT32 => ImageHasher.Compare(Hasher.DctHash(image1), Hasher.DctHash(image2path)),
            ImageHashAlgorithm.DCT64 => ImageHasher.Compare(Hasher.DctHash64(image1), Hasher.DctHash64(image2path)),
            ImageHashAlgorithm.DCT256 => ImageHasher.Compare(Hasher.DctHash256(image1), Hasher.DctHash256(image2path)),
            ImageHashAlgorithm.MHHash256 => ImageHasher.Compare(Hasher.MHHash256(image1), Hasher.MHHash256(image2path)),
            ImageHashAlgorithm.PDQ256 => ImageHasher.Compare(Hasher.PDQHash256(image1), Hasher.PDQHash256(image2path)),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };
    }
}

public enum ImageHashAlgorithm
{
    /// <summary>均值算法</summary>
    Average,
    /// <summary>中值算法</summary>
    Medium,
    /// <summary>差异算法</summary>
    Difference,
    /// <summary>32分辨率精度DCT算法</summary>
    DCT32,
    /// <summary>64分辨率精度DCT算法</summary>
    DCT64,
    /// <summary>256位DCT算法</summary>
    DCT256,
    /// <summary>256位MH算法</summary>
    MHHash256,
    /// <summary>256位PDQ算法</summary>
    PDQ256
}
