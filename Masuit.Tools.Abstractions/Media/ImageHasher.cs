using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Masuit.Tools.Media;

/// <summary>
/// 图像hash机算器
/// </summary>
public class ImageHasher
{
    private readonly IImageTransformer _transformer;
    private float[][] _dctMatrix;
    private bool _isDctMatrixInitialized;
    private readonly object _dctMatrixLockObject = new();

    /// <summary>
    /// 默认使用ImageSharpTransformer初始化实例
    /// </summary>
    public ImageHasher()
    {
        _transformer = new ImageSharpTransformer();
    }

    /// <summary>
    /// 使用给定的IImageTransformer初始化实例
    /// </summary>
    /// <param name="transformer">用于图像变换的IImageTransformer的实现类</param>
    public ImageHasher(IImageTransformer transformer)
    {
        _transformer = transformer;
    }

    /// <summary>
    /// 使用平均值算法计算图像的64位哈希
    /// </summary>
    /// <param name="pathToImage">图片的文件路径</param>
    /// <returns>64位hash值</returns>
    public ulong AverageHash64(string pathToImage)
    {
#if NET6_0_OR_GREATER

        var decoderOptions = new DecoderOptions
        {
            TargetSize = new Size(144),
            SkipMetadata = true
        };
        using var image = Image.Load<L8>(decoderOptions, pathToImage);
#else
        using var image = Image.Load<L8>(pathToImage);
#endif
        return AverageHash64(image);
    }

    /// <summary>
    /// 使用平均值算法计算图像的64位哈希
    /// </summary>
    /// <param name="sourceStream">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong AverageHash64(Stream sourceStream)
    {
        var pixels = _transformer.TransformImage(sourceStream, 8, 8);
        var average = pixels.Sum(b => b) / 64;

        // 遍历所有像素，如果超过平均值，则将其设置为1，如果低于平均值，则将其设置为0。
        var hash = 0UL;
        for (var i = 0; i < 64; i++)
        {
            if (pixels[i] > average)
            {
                hash |= 1UL << i;
            }
        }

        return hash;
    }

    /// <summary>
    /// 使用平均值算法计算图像的64位哈希
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong AverageHash64(Image image)
    {
        using var source = image.CloneAs<L8>();
        return AverageHash64(source);
    }

    /// <summary>
    /// 使用平均值算法计算图像的64位哈希
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong AverageHash64(Image<L8> image)
    {
        var pixels = _transformer.TransformImage(image, 8, 8);
        var average = pixels.Sum(b => b) / 64;

        // 遍历所有像素，如果超过平均值，则将其设置为1，如果低于平均值，则将其设置为0。
        var hash = 0UL;
        for (var i = 0; i < 64; i++)
        {
            if (pixels[i] > average)
            {
                hash |= 1UL << i;
            }
        }

        return hash;
    }

    /// <summary>
    /// 使用中值算法计算给定图像的64位哈希
    /// 将图像转换为8x8灰度图像，从中查找中值像素值，然后在结果哈希中将值大于中值的所有像素标记为1。与基于平均值的实现相比，更能抵抗非线性图像编辑。
    /// </summary>
    /// <param name="pathToImage">图片的文件路径</param>
    /// <returns>64位hash值</returns>
    public ulong MedianHash64(string pathToImage)
    {
#if NET6_0_OR_GREATER

        var decoderOptions = new DecoderOptions
        {
            TargetSize = new Size(144),
            SkipMetadata = true
        };
        using var image = Image.Load<L8>(decoderOptions, pathToImage);
#else
        using var image = Image.Load<L8>(pathToImage);
#endif
        return MedianHash64(image);
    }

    /// <summary>
    /// 使用中值算法计算给定图像的64位哈希
    /// 将图像转换为8x8灰度图像，从中查找中值像素值，然后在结果哈希中将值大于中值的所有像素标记为1。与基于平均值的实现相比，更能抵抗非线性图像编辑。
    /// </summary>
    /// <param name="sourceStream">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong MedianHash64(Stream sourceStream)
    {
        var pixels = _transformer.TransformImage(sourceStream, 8, 8);

        // 计算中值
        var pixelList = new List<byte>(pixels);
        pixelList.Sort();

        // 中间像素中值
        var median = (byte)((pixelList[31] + pixelList[32]) / 2);

        // 遍历所有像素，如果超过中值，则将其设置为1，如果低于中值，则将其设置为0。
        var hash = 0UL;
        for (var i = 0; i < 64; i++)
        {
            if (pixels[i] > median)
            {
                hash |= 1UL << i;
            }
        }

        return hash;
    }

    /// <summary>
    /// 使用中值算法计算给定图像的64位哈希
    /// 将图像转换为8x8灰度图像，从中查找中值像素值，然后在结果哈希中将值大于中值的所有像素标记为1。与基于平均值的实现相比，更能抵抗非线性图像编辑。
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong MedianHash64(Image image)
    {
        using var source = image.CloneAs<L8>();
        return MedianHash64(source);
    }

    /// <summary>
    /// 使用中值算法计算给定图像的64位哈希
    /// 将图像转换为8x8灰度图像，从中查找中值像素值，然后在结果哈希中将值大于中值的所有像素标记为1。与基于平均值的实现相比，更能抵抗非线性图像编辑。
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong MedianHash64(Image<L8> image)
    {
        var pixels = _transformer.TransformImage(image, 8, 8);

        // 计算中值
        var pixelList = new List<byte>(pixels);
        pixelList.Sort();

        // 中间像素中值
        var median = (byte)((pixelList[31] + pixelList[32]) / 2);

        // 遍历所有像素，如果超过中值，则将其设置为1，如果低于中值，则将其设置为0。
        var hash = 0UL;
        for (var i = 0; i < 64; i++)
        {
            if (pixels[i] > median)
            {
                hash |= 1UL << i;
            }
        }

        return hash;
    }

    /// <summary>
    /// 使用中值算法计算给定图像的256位哈希
    /// 将图像转换为16x16的灰度图像，从中查找中值像素值，然后在结果哈希中将值大于中值的所有像素标记为1。与基于平均值的实现相比，更能抵抗非线性图像编辑。
    /// </summary>
    /// <param name="pathToImage">图片的文件路径</param>
    /// <returns>256位hash值，生成一个4长度的数组返回</returns>
    public ulong[] MedianHash256(string pathToImage)
    {
#if NET6_0_OR_GREATER

        var decoderOptions = new DecoderOptions
        {
            TargetSize = new Size(144),
            SkipMetadata = true
        };
        using var image = Image.Load<L8>(decoderOptions, pathToImage);
#else
        using var image = Image.Load<L8>(pathToImage);
#endif
        return MedianHash256(image);
    }

    /// <summary>
    /// 使用中值算法计算给定图像的256位哈希
    /// 将图像转换为16x16的灰度图像，从中查找中值像素值，然后在结果哈希中将值大于中值的所有像素标记为1。与基于平均值的实现相比，更能抵抗非线性图像编辑。
    /// </summary>
    /// <param name="sourceStream">读取到的图片流</param>
    /// <returns>256位hash值，生成一个4长度的数组返回</returns>
    public ulong[] MedianHash256(Stream sourceStream)
    {
        var pixels = _transformer.TransformImage(sourceStream, 16, 16);

        // 计算中值
        var pixelList = new List<byte>(pixels);
        pixelList.Sort();

        // 中间像素中值
        var median = (byte)((pixelList[127] + pixelList[128]) / 2);

        // 遍历所有像素，如果超过中值，则将其设置为1，如果低于中值，则将其设置为0。
        var hash64 = 0UL;
        var hash = new ulong[4];
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 64; j++)
            {
                if (pixels[64 * i + j] > median)
                {
                    hash64 |= 1UL << j;
                }
            }
            hash[i] = hash64;
            hash64 = 0UL;
        }

        return hash;
    }

    /// <summary>
    /// 使用中值算法计算给定图像的256位哈希
    /// 将图像转换为16x16的灰度图像，从中查找中值像素值，然后在结果哈希中将值大于中值的所有像素标记为1。与基于平均值的实现相比，更能抵抗非线性图像编辑。
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>256位hash值，生成一个4长度的数组返回</returns>
    public ulong[] MedianHash256(Image image)
    {
        using var source = image.CloneAs<L8>();
        return MedianHash256(source);
    }

    /// <summary>
    /// 使用中值算法计算给定图像的256位哈希
    /// 将图像转换为16x16的灰度图像，从中查找中值像素值，然后在结果哈希中将值大于中值的所有像素标记为1。与基于平均值的实现相比，更能抵抗非线性图像编辑。
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>256位hash值，生成一个4长度的数组返回</returns>
    public ulong[] MedianHash256(Image<L8> image)
    {
        var pixels = _transformer.TransformImage(image, 16, 16);

        // 计算中值
        var pixelList = new List<byte>(pixels);
        pixelList.Sort();

        // 中间像素中值
        var median = (byte)((pixelList[127] + pixelList[128]) / 2);

        // 遍历所有像素，如果超过中值，则将其设置为1，如果低于中值，则将其设置为0。
        var hash64 = 0UL;
        var hash = new ulong[4];
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 64; j++)
            {
                if (pixels[64 * i + j] > median)
                {
                    hash64 |= 1UL << j;
                }
            }
            hash[i] = hash64;
            hash64 = 0UL;
        }

        return hash;
    }

    /// <summary>
    /// 使用差分哈希算法计算图像的64位哈希。
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="pathToImage">图片的文件路径</param>
    /// <returns>64位hash值</returns>
    public ulong DifferenceHash64(string pathToImage)
    {
#if NET6_0_OR_GREATER

        var decoderOptions = new DecoderOptions
        {
            TargetSize = new Size(144),
            SkipMetadata = true
        };
        using var image = Image.Load<L8>(decoderOptions, pathToImage);
#else
        using var image = Image.Load<L8>(pathToImage);
#endif
        return DifferenceHash64(image);
    }

    /// <summary>
    /// 使用差分哈希算法计算图像的64位哈希。
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="sourceStream">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong DifferenceHash64(Stream sourceStream)
    {
        var pixels = _transformer.TransformImage(sourceStream, 9, 8);

        // 遍历像素，如果左侧像素比右侧像素亮，则将哈希设置为1。
        var hash = 0UL;
        var hashPos = 0;
        for (var i = 0; i < 8; i++)
        {
            var rowStart = i * 9;
            for (var j = 0; j < 8; j++)
            {
                if (pixels[rowStart + j] > pixels[rowStart + j + 1])
                {
                    hash |= 1UL << hashPos;
                }

                hashPos++;
            }
        }

        return hash;
    }

    /// <summary>
    /// 使用差分哈希算法计算图像的64位哈希。
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong DifferenceHash64(Image image)
    {
        using var source = image.CloneAs<L8>();
        return DifferenceHash64(source);
    }

    /// <summary>
    /// 使用差分哈希算法计算图像的64位哈希。
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong DifferenceHash64(Image<L8> image)
    {
        var pixels = _transformer.TransformImage(image, 9, 8);

        // 遍历像素，如果左侧像素比右侧像素亮，则将哈希设置为1。
        var hash = 0UL;
        var hashPos = 0;
        for (var i = 0; i < 8; i++)
        {
            var rowStart = i * 9;
            for (var j = 0; j < 8; j++)
            {
                if (pixels[rowStart + j] > pixels[rowStart + j + 1])
                {
                    hash |= 1UL << hashPos;
                }

                hashPos++;
            }
        }

        return hash;
    }

    /// <summary>
    /// 使用差分哈希算法计算图像的256位哈希。
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="pathToImage">图片的文件路径</param>
    /// <returns>256位hash值</returns>
    public ulong[] DifferenceHash256(string pathToImage)
    {
#if NET6_0_OR_GREATER

        var decoderOptions = new DecoderOptions
        {
            TargetSize = new Size(144),
            SkipMetadata = true,
        };
        using var image = Image.Load<L8>(decoderOptions, pathToImage);
#else
        using var image = Image.Load<L8>(pathToImage);
#endif
        return DifferenceHash256(image);
    }

    /// <summary>
    /// 使用差分哈希算法计算图像的64位哈希。
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="sourceStream">读取到的图片流</param>
    /// <returns>256位hash值</returns>
    public ulong[] DifferenceHash256(Stream sourceStream)
    {
        var pixels = _transformer.TransformImage(sourceStream, 17, 16);

        // 遍历像素，如果左侧像素比右侧像素亮，则将哈希设置为1。
        var hash = new ulong[4];
        var hashPos = 0;
        var hashPart = 0;
        for (var i = 0; i < 16; i++)
        {
            var rowStart = i * 17;
            for (var j = 0; j < 16; j++)
            {
                if (pixels[rowStart + j] > pixels[rowStart + j + 1])
                {
                    hash[hashPart] |= 1UL << hashPos;
                }

                if (hashPos == 63)
                {
                    hashPos = 0;
                    hashPart++;
                }
                else
                {
                    hashPos++;
                }
            }
        }

        return hash;
    }

    /// <summary>
    /// 使用差分哈希算法计算图像的64位哈希。
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片流</param>
    /// <returns>256位hash值</returns>
    public ulong[] DifferenceHash256(Image image)
    {
        using var source = image.CloneAs<L8>();
        return DifferenceHash256(source);
    }

    /// <summary>
    /// 使用差分哈希算法计算图像的64位哈希。
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片流</param>
    /// <returns>256位hash值</returns>
    public ulong[] DifferenceHash256(Image<L8> image)
    {
        var pixels = _transformer.TransformImage(image, 17, 16);

        // 遍历像素，如果左侧像素比右侧像素亮，则将哈希设置为1。
        var hash = new ulong[4];
        var hashPos = 0;
        var hashPart = 0;
        for (var i = 0; i < 16; i++)
        {
            var rowStart = i * 17;
            for (var j = 0; j < 16; j++)
            {
                if (pixels[rowStart + j] > pixels[rowStart + j + 1])
                {
                    hash[hashPart] |= 1UL << hashPos;
                }

                if (hashPos == 63)
                {
                    hashPos = 0;
                    hashPart++;
                }
                else
                {
                    hashPos++;
                }
            }
        }

        return hash;
    }

    /// <summary>
    /// 使用DCT算法计算图像的64位哈希
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="path">图片路径</param>
    /// <returns>64位hash值</returns>
    public ulong DctHash(string path)
    {
#if NET6_0_OR_GREATER

        var decoderOptions = new DecoderOptions
        {
            TargetSize = new Size(160),
            SkipMetadata = true
        };
        using var image = Image.Load<L8>(decoderOptions, path);
#else
        using var image = Image.Load<L8>(path);
#endif
        return DctHash(image);
    }

    /// <summary>
    /// 使用DCT算法计算图像的64位哈希
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片</param>
    /// <returns>64位hash值</returns>
    public ulong DctHash(Image image)
    {
        using var clone = image.CloneAs<L8>();
        return DctHash(clone);
    }

    /// <summary>
    /// 使用DCT算法计算图像的64位哈希
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片</param>
    /// <returns>64位hash值</returns>
    public ulong DctHash(Image<L8> image)
    {
        var grayscalePixels = _transformer.GetPixelData(image, 32, 32);
        var dctMatrix = ComputeDct(grayscalePixels, 32);
        var topLeftBlock = ExtractTopLeftBlock(dctMatrix, 8);
        var median = CalculateMedian(topLeftBlock);
        var hash = GenerateHash(topLeftBlock, median);
        return hash;
    }

    /// <summary>
    /// 计算图像的DCT矩阵
    /// </summary>
    /// <param name="input"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    private double[,] ComputeDct(byte[,] input, int size)
    {
        var output = new double[size, size];
        var rowDCT = new double[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                rowDCT[y, x] = input[y, x];
            }
        }

        for (int y = 0; y < size; y++)
        {
            var row = new double[size];
            for (int x = 0; x < size; x++)
            {
                row[x] = rowDCT[y, x];
            }
            var dctRow = DCT1D(row);
            for (int x = 0; x < size; x++)
            {
                rowDCT[y, x] = dctRow[x];
            }
        }

        for (int x = 0; x < size; x++)
        {
            var col = new double[size];
            for (int y = 0; y < size; y++)
            {
                col[y] = rowDCT[y, x];
            }
            var dctCol = DCT1D(col);
            for (int y = 0; y < size; y++)
            {
                output[y, x] = dctCol[y];
            }
        }

        return output;
    }

    private double[] DCT1D(double[] input)
    {
        int n = input.Length;
        var output = new double[n];

        for (int u = 0; u < n; u++)
        {
            double sum = 0.0;
            double cu = u == 0 ? 1.0 / Math.Sqrt(2.0) : 1.0;

            for (int x = 0; x < n; x++)
            {
                sum += input[x] * Math.Cos((Math.PI / n) * (x + 0.5) * u);
            }

            output[u] = cu * sum * Math.Sqrt(2.0 / n);
        }

        return output;
    }

    private double[,] ExtractTopLeftBlock(double[,] matrix, int blockSize)
    {
        var block = new double[blockSize, blockSize];

        for (int y = 0; y < blockSize; y++)
        {
            for (int x = 0; x < blockSize; x++)
            {
                block[y, x] = matrix[y, x];
            }
        }

        return block;
    }

    private double CalculateMedian(double[,] matrix)
    {
        int height = matrix.GetLength(0);
        int width = matrix.GetLength(1);

        var flatArray = new double[height * width];
        int index = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flatArray[index++] = matrix[y, x];
            }
        }

        Array.Sort(flatArray);

        if (flatArray.Length % 2 == 0)
        {
            return (flatArray[flatArray.Length / 2 - 1] + flatArray[flatArray.Length / 2]) / 2.0;
        }

        return flatArray[flatArray.Length / 2];
    }

    private ulong GenerateHash(double[,] block, double median)
    {
        ulong hash = 0UL;
        int bitPosition = 0;
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (block[y, x] >= median)
                {
                    hash |= (1UL << bitPosition);
                }
                bitPosition++;
            }
        }

        return hash;
    }

    /// <summary>
    /// 使用汉明距离比较两幅图像的哈希值。结果1表示图像完全相同，而结果0表示图像完全不同。
    /// </summary>
    /// <param name="hash1">图像1的hash</param>
    /// <param name="hash2">图像2的hash</param>
    /// <returns>相似度范围：[0,1]</returns>
    public static float Compare(ulong hash1, ulong hash2)
    {
        // hash异或运算
        var hashDifference = hash1 ^ hash2;

        // 计算汉明距离
        var onesInHash = HammingWeight(hashDifference);

        // 得到相似度
        return 1.0f - onesInHash / 64.0f;
    }

    /// <summary>
    /// 使用汉明距离比较两幅图像的哈希值。结果1表示图像完全相同，而结果0表示图像完全不同。
    /// </summary>
    /// <param name="hash1">图像1的hash</param>
    /// <param name="hash2">图像2的hash</param>
    /// <returns>相似度范围：[0,1]</returns>
    public static float Compare(ulong[] hash1, ulong[] hash2)
    {
        // 检查两个图像的hash长度是否一致
        if (hash1.Length != hash2.Length)
        {
            throw new ArgumentException("hash1 与 hash2长度不匹配");
        }

        var hashSize = hash1.Length;
        ulong onesInHash = 0;

        // hash异或运算
        var hashDifference = new ulong[hashSize];
        for (var i = 0; i < hashSize; i++)
        {
            hashDifference[i] = hash1[i] ^ hash2[i];
        }

        // 逐个计算汉明距离
        for (var i = 0; i < hashSize; i++)
        {
            onesInHash += HammingWeight(hashDifference[i]);
        }

        // 得到相似度
        return 1.0f - onesInHash / (hashSize * 64.0f);
    }

    /// <summary>
    /// 计算hash的汉明权重.
    /// </summary>
    /// <see cref="http://en.wikipedia.org/wiki/Hamming_weight"/>
    /// <param name="hash"></param>
    /// <returns></returns>
    private static ulong HammingWeight(ulong hash)
    {
        hash -= (hash >> 1) & M1;
        hash = (hash & M2) + ((hash >> 2) & M2);
        hash = (hash + (hash >> 4)) & M4;
        var onesInHash = (hash * H01) >> 56;

        return onesInHash;
    }

    // 汉明距离常量. http://en.wikipedia.org/wiki/Hamming_weight
    private const ulong M1 = 0x5555555555555555; //binary: 0101...

    private const ulong M2 = 0x3333333333333333; //binary: 00110011..
    private const ulong M4 = 0x0f0f0f0f0f0f0f0f; //binary:  4 个0,  4 个1 ...
    private const ulong H01 = 0x0101010101010101; //the sum of 256 to the power of 0,1,2,3...
}