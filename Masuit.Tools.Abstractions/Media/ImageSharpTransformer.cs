using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using static System.Net.WebRequestMethods;
using Image = SixLabors.ImageSharp.Image;
using Size = SixLabors.ImageSharp.Size;

namespace Masuit.Tools.Media;

/// <summary>
/// 使用ImageSharp进行图像变换
/// </summary>
public class ImageSharpTransformer : IImageTransformer
{
    public byte[] TransformImage(Stream stream, int width, int height)
    {
        using var image = Image.Load<Rgba32>(stream);
        return TransformImage(image, width, height);
    }

    public byte[] TransformImage(Image<Rgba32> image, int width, int height)
    {
        image.Mutate(x => x.Resize(new ResizeOptions()
        {
            Size = new Size
            {
                Width = width,
                Height = height
            },
            Mode = ResizeMode.Stretch,
            Sampler = new BicubicResampler()
        }).Grayscale());
        image.DangerousTryGetSinglePixelMemory(out var pixelSpan);
        var pixelArray = pixelSpan.ToArray();
        var pixelCount = width * height;
        var bytes = new byte[pixelCount];
        for (var i = 0; i < pixelCount; i++)
        {
            bytes[i] = pixelArray[i].B;
        }

        return bytes;
    }
}

public static class ImageHashExt
{
    /// <summary>
    /// 使用平均值算法计算图像的64位哈希
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public static ulong AverageHash64(this Image<Rgba32> image)
    {
        var hasher = new ImageHasher();
        return hasher.AverageHash64(image);
    }

    /// <summary>
    /// 使用中值算法计算给定图像的64位哈希
    /// 将图像转换为8x8灰度图像，从中查找中值像素值，然后在结果哈希中将值大于中值的所有像素标记为1。与基于平均值的实现相比，更能抵抗非线性图像编辑。
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public static ulong MedianHash64(this Image<Rgba32> image)
    {
        var hasher = new ImageHasher();
        return hasher.MedianHash64(image);
    }

    /// <summary>
    /// 使用中值算法计算给定图像的256位哈希
    /// 将图像转换为16x16的灰度图像，从中查找中值像素值，然后在结果哈希中将值大于中值的所有像素标记为1。与基于平均值的实现相比，更能抵抗非线性图像编辑。
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>256位hash值，生成一个4长度的数组返回</returns>
    public static ulong[] MedianHash256(this Image<Rgba32> image)
    {
        var hasher = new ImageHasher();
        return hasher.MedianHash256(image);
    }

    /// <summary>
    /// 使用差分哈希算法计算图像的64位哈希。
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public static ulong DifferenceHash64(this Image<Rgba32> image)
    {
        var hasher = new ImageHasher();
        return hasher.DifferenceHash64(image);
    }

    /// <summary>
    /// 使用差分哈希算法计算图像的64位哈希。
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片流</param>
    /// <returns>256位hash值</returns>
    public static ulong[] DifferenceHash256(this Image<Rgba32> image)
    {
        var hasher = new ImageHasher();
        return hasher.DifferenceHash256(image);
    }

    /// <summary>
    /// 使用DCT算法计算图像的64位哈希
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public static ulong DctHash(this Image<Rgba32> image)
    {
        var hasher = new ImageHasher();
        return hasher.DctHash(image);
    }

    /// <summary>
    /// 使用汉明距离比较两幅图像的哈希值。结果1表示图像完全相同，而结果0表示图像完全不同。
    /// </summary>
    /// <param name="image1">图像1</param>
    /// <param name="image2">图像2</param>
    /// <returns>相似度范围：[0,1]</returns>
    public static float Compare(this Image<Rgba32> image1, Image<Rgba32> image2)
    {
        var hasher = new ImageHasher();
        var hash1 = hasher.DifferenceHash256(image1);
        var hash2 = hasher.DifferenceHash256(image2);
        return ImageHasher.Compare(hash1, hash2);
    }

    /// <summary>
    /// 使用汉明距离比较两幅图像的哈希值。结果1表示图像完全相同，而结果0表示图像完全不同。
    /// </summary>
    /// <param name="image1">图像1的hash</param>
    /// <param name="image2path">图像2的路径</param>
    /// <returns>相似度范围：[0,1]</returns>
    public static float Compare(this Image<Rgba32> image1, string image2path)
    {
        var hasher = new ImageHasher();
        var hash1 = hasher.DifferenceHash256(image1);
        var hash2 = hasher.DifferenceHash256(image2path);
        return ImageHasher.Compare(hash1, hash2);
    }
}
