using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.IO;

namespace Masuit.Tools.Media;

/// <summary>
/// 使用ImageSharp进行图像变换
/// </summary>
public class ImageSharpTransformer : IImageTransformer
{
#if NET6_0_OR_GREATER
    public byte[] TransformImage(Stream stream, int width, int height)
    {
        var decoderOptions = new DecoderOptions
        {
            TargetSize = new Size(144),
            SkipMetadata = true,
        };
        using var image = Image.Load<L8>(decoderOptions, stream);
        return TransformImage(image, width, height);
    }
#else

    public byte[] TransformImage(Stream stream, int width, int height)
    {
        using var image = Image.Load<L8>(stream);
        return TransformImage(image, width, height);
    }

#endif

    public byte[] TransformImage(Image<L8> image, int width, int height)
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
        }));
        image.DangerousTryGetSinglePixelMemory(out var pixelSpan);
        var pixelArray = pixelSpan.ToArray();
        var pixelCount = width * height;
        var bytes = new byte[pixelCount];
        for (var i = 0; i < pixelCount; i++)
        {
            bytes[i] = pixelArray[i].PackedValue;
        }

        return bytes;
    }

    public byte[,] GetPixelData(Image<L8> image, int width, int height)
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
        }));
        var grayscalePixels = new byte[width, height];
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < width; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < height; x++)
                {
                    var pixel = row[x];
                    grayscalePixels[y, x] = pixel.PackedValue;
                }
            }
        });

        return grayscalePixels;
    }
}

public static class ImageHashExt
{
    /// <summary>
    /// 使用平均值算法计算图像的64位哈希
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public static ulong AverageHash64(this Image image)
    {
        var hasher = new ImageHasher();
        return hasher.AverageHash64(image);
    }

    /// <summary>
    /// 使用平均值算法计算图像的64位哈希
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public static ulong AverageHash64(this Image<L8> image)
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
    public static ulong MedianHash64(this Image image)
    {
        var hasher = new ImageHasher();
        return hasher.MedianHash64(image);
    }

    /// <summary>
    /// 使用中值算法计算给定图像的64位哈希
    /// 将图像转换为8x8灰度图像，从中查找中值像素值，然后在结果哈希中将值大于中值的所有像素标记为1。与基于平均值的实现相比，更能抵抗非线性图像编辑。
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public static ulong MedianHash64(this Image<L8> image)
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
    public static ulong[] MedianHash256(this Image image)
    {
        var hasher = new ImageHasher();
        return hasher.MedianHash256(image);
    }

    /// <summary>
    /// 使用中值算法计算给定图像的256位哈希
    /// 将图像转换为16x16的灰度图像，从中查找中值像素值，然后在结果哈希中将值大于中值的所有像素标记为1。与基于平均值的实现相比，更能抵抗非线性图像编辑。
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>256位hash值，生成一个4长度的数组返回</returns>
    public static ulong[] MedianHash256(this Image<L8> image)
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
    public static ulong DifferenceHash64(this Image image)
    {
        var hasher = new ImageHasher();
        return hasher.DifferenceHash64(image);
    }

    /// <summary>
    /// 使用差分哈希算法计算图像的64位哈希。
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public static ulong DifferenceHash64(this Image<L8> image)
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
    public static ulong[] DifferenceHash256(this Image image)
    {
        var hasher = new ImageHasher();
        return hasher.DifferenceHash256(image);
    }

    /// <summary>
    /// 使用差分哈希算法计算图像的64位哈希。
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片流</param>
    /// <returns>256位hash值</returns>
    public static ulong[] DifferenceHash256(this Image<L8> image)
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
    public static ulong DctHash(this Image image)
    {
        var hasher = new ImageHasher();
        return hasher.DctHash(image);
    }

    /// <summary>
    /// 使用DCT算法计算图像的64位哈希
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public static ulong DctHash(this Image<L8> image)
    {
        var hasher = new ImageHasher();
        return hasher.DctHash(image);
    }

    /// <summary>
    /// 使用汉明距离比较两幅图像的哈希值。结果1表示图像完全相同，而结果0表示图像完全不同。
    /// </summary>
    /// <param name="image1">图像1</param>
    /// <param name="image2">图像2</param>
    /// <param name="algorithm">对比算法</param>
    /// <returns>相似度范围：[0,1]</returns>
    public static float Compare(this Image image1, Image image2, ImageHashAlgorithm algorithm = ImageHashAlgorithm.Difference)
    {
        var hasher = new ImageHasher();
        return algorithm switch
        {
            ImageHashAlgorithm.Average => ImageHasher.Compare(hasher.AverageHash64(image1), hasher.AverageHash64(image2)),
            ImageHashAlgorithm.Medium => ImageHasher.Compare(hasher.MedianHash256(image1), hasher.MedianHash256(image2)),
            ImageHashAlgorithm.Difference => ImageHasher.Compare(hasher.DifferenceHash256(image1), hasher.DifferenceHash256(image2)),
            ImageHashAlgorithm.DCT => ImageHasher.Compare(hasher.DctHash(image1), hasher.DctHash(image2)),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };
    }

    /// <summary>
    /// 使用汉明距离比较两幅图像的哈希值。结果1表示图像完全相同，而结果0表示图像完全不同。
    /// </summary>
    /// <param name="image1">图像1</param>
    /// <param name="image2">图像2</param>
    /// <param name="algorithm">对比算法</param>
    /// <returns>相似度范围：[0,1]</returns>
    public static float Compare(this Image<L8> image1, Image<L8> image2, ImageHashAlgorithm algorithm = ImageHashAlgorithm.Difference)
    {
        var hasher = new ImageHasher();
        return algorithm switch
        {
            ImageHashAlgorithm.Average => ImageHasher.Compare(hasher.AverageHash64(image1), hasher.AverageHash64(image2)),
            ImageHashAlgorithm.Medium => ImageHasher.Compare(hasher.MedianHash256(image1), hasher.MedianHash256(image2)),
            ImageHashAlgorithm.Difference => ImageHasher.Compare(hasher.DifferenceHash256(image1), hasher.DifferenceHash256(image2)),
            ImageHashAlgorithm.DCT => ImageHasher.Compare(hasher.DctHash(image1), hasher.DctHash(image2)),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };
    }

    /// <summary>
    /// 使用汉明距离比较两幅图像的哈希值。结果1表示图像完全相同，而结果0表示图像完全不同。
    /// </summary>
    /// <param name="image1">图像1的hash</param>
    /// <param name="image2path">图像2的路径</param>
    /// <param name="algorithm">对比算法</param>
    /// <returns>相似度范围：[0,1]</returns>
    public static float Compare(this Image image1, string image2path, ImageHashAlgorithm algorithm = ImageHashAlgorithm.Difference)
    {
        var hasher = new ImageHasher();
        return algorithm switch
        {
            ImageHashAlgorithm.Average => ImageHasher.Compare(hasher.AverageHash64(image1), hasher.AverageHash64(image2path)),
            ImageHashAlgorithm.Medium => ImageHasher.Compare(hasher.MedianHash256(image1), hasher.MedianHash256(image2path)),
            ImageHashAlgorithm.Difference => ImageHasher.Compare(hasher.DifferenceHash256(image1), hasher.DifferenceHash256(image2path)),
            ImageHashAlgorithm.DCT => ImageHasher.Compare(hasher.DctHash(image1), hasher.DctHash(image2path)),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };
    }

    /// <summary>
    /// 使用汉明距离比较两幅图像的哈希值。结果1表示图像完全相同，而结果0表示图像完全不同。
    /// </summary>
    /// <param name="image1">图像1的hash</param>
    /// <param name="image2path">图像2的路径</param>
    /// <param name="algorithm">对比算法</param>
    /// <returns>相似度范围：[0,1]</returns>
    public static float Compare(this Image<L8> image1, string image2path, ImageHashAlgorithm algorithm = ImageHashAlgorithm.Difference)
    {
        var hasher = new ImageHasher();
        return algorithm switch
        {
            ImageHashAlgorithm.Average => ImageHasher.Compare(hasher.AverageHash64(image1), hasher.AverageHash64(image2path)),
            ImageHashAlgorithm.Medium => ImageHasher.Compare(hasher.MedianHash256(image1), hasher.MedianHash256(image2path)),
            ImageHashAlgorithm.Difference => ImageHasher.Compare(hasher.DifferenceHash256(image1), hasher.DifferenceHash256(image2path)),
            ImageHashAlgorithm.DCT => ImageHasher.Compare(hasher.DctHash(image1), hasher.DctHash(image2path)),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };
    }
}

public enum ImageHashAlgorithm
{
    /// <summary>
    /// 均值算法
    /// </summary>
    Average,

    /// <summary>
    /// 中值算法
    /// </summary>
    Medium,

    /// <summary>
    /// 差异算法
    /// </summary>
    Difference,

    /// <summary>
    /// DCT算法
    /// </summary>
    DCT
}