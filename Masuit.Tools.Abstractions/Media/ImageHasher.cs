using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Masuit.Tools.Media;

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
        using var stream = new FileStream(pathToImage, FileMode.Open, FileAccess.Read);
        return AverageHash64(stream);
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
    public ulong AverageHash64(Image<Rgba32> image)
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
        using var stream = new FileStream(pathToImage, FileMode.Open, FileAccess.Read);
        return MedianHash64(stream);
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
    public ulong MedianHash64(Image<Rgba32> image)
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
        using var stream = new FileStream(pathToImage, FileMode.Open, FileAccess.Read);
        return MedianHash256(stream);
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
    public ulong[] MedianHash256(Image<Rgba32> image)
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
        using var stream = new FileStream(pathToImage, FileMode.Open, FileAccess.Read);
        return DifferenceHash64(stream);
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
    public ulong DifferenceHash64(Image<Rgba32> image)
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
        using var stream = new FileStream(pathToImage, FileMode.Open, FileAccess.Read);
        return DifferenceHash256(stream);
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
    public ulong[] DifferenceHash256(Image<Rgba32> image)
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
    /// <param name="sourceStream">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong DctHash(Stream sourceStream)
    {
        lock (_dctMatrixLockObject)
        {
            if (!_isDctMatrixInitialized)
            {
                _dctMatrix = GenerateDctMatrix(32);
                _isDctMatrixInitialized = true;
            }
        }

        var pixels = _transformer.TransformImage(sourceStream, 32, 32);

        // 将像素转换成float类型数组
        var fPixels = new float[1024];
        for (var i = 0; i < 1024; i++)
        {
            fPixels[i] = pixels[i] / 255.0f;
        }

        // 计算 dct 矩阵
        var dctPixels = ComputeDct(fPixels, _dctMatrix);

        // 从矩阵中1,1到8,8获得8x8的区域，忽略最低频率以提高检测
        var dctHashPixels = new float[64];
        for (var x = 0; x < 8; x++)
        {
            for (var y = 0; y < 8; y++)
            {
                dctHashPixels[x + y * 8] = dctPixels[x + 1][y + 1];
            }
        }

        // 计算中值
        var pixelList = new List<float>(dctHashPixels);
        pixelList.Sort();

        // 中间像素的平均值
        var median = (pixelList[31] + pixelList[32]) / 2;

        // 遍历所有像素，如果超过中值，则将其设置为1，如果低于中值，则将其设置为0。
        var hash = 0UL;
        for (var i = 0; i < 64; i++)
        {
            if (dctHashPixels[i] > median)
            {
                hash |= 1UL << i;
            }
        }

        return hash;
    }

    /// <summary>
    /// 使用DCT算法计算图像的64位哈希
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong DctHash(Image<Rgba32> image)
    {
        lock (_dctMatrixLockObject)
        {
            if (!_isDctMatrixInitialized)
            {
                _dctMatrix = GenerateDctMatrix(32);
                _isDctMatrixInitialized = true;
            }
        }

        var pixels = _transformer.TransformImage(image, 32, 32);

        // 将像素转换成float类型数组
        var fPixels = new float[1024];
        for (var i = 0; i < 1024; i++)
        {
            fPixels[i] = pixels[i] / 255.0f;
        }

        // 计算 dct 矩阵
        var dctPixels = ComputeDct(fPixels, _dctMatrix);

        // 从矩阵中1,1到8,8获得8x8的区域，忽略最低频率以提高检测
        var dctHashPixels = new float[64];
        for (var x = 0; x < 8; x++)
        {
            for (var y = 0; y < 8; y++)
            {
                dctHashPixels[x + y * 8] = dctPixels[x + 1][y + 1];
            }
        }

        // 计算中值
        var pixelList = new List<float>(dctHashPixels);
        pixelList.Sort();

        // 中间像素的平均值
        var median = (pixelList[31] + pixelList[32]) / 2;

        // 遍历所有像素，如果超过中值，则将其设置为1，如果低于中值，则将其设置为0。
        var hash = 0UL;
        for (var i = 0; i < 64; i++)
        {
            if (dctHashPixels[i] > median)
            {
                hash |= 1UL << i;
            }
        }

        return hash;
    }

    /// <summary>
    /// 使用DCT算法计算图像的64位哈希
    /// </summary>
    /// <param name="path">图片的文件路径</param>
    /// <returns>64位hash值</returns>
    public ulong DctHash(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return DctHash(stream);
    }

    /// <summary>
    /// 计算图像的DCT矩阵
    /// </summary>
    /// <param name="image">用于计算dct的图像</param>
    /// <param name="dctMatrix">DCT系数矩阵</param>
    /// <returns>图像的DCT矩阵</returns>
    private static float[][] ComputeDct(float[] image, float[][] dctMatrix)
    {
        // dct矩阵的大小，图像的大小与DCT矩阵相同
        var size = dctMatrix.GetLength(0);

        // 降图像转换成矩阵
        var imageMat = new float[size][];
        for (var i = 0; i < size; i++)
        {
            imageMat[i] = new float[size];
        }

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                imageMat[y][x] = image[x + y * size];
            }
        }

        return Multiply(Multiply(dctMatrix, imageMat), Transpose(dctMatrix));
    }

    /// <summary>
    /// 生成DCT系数矩阵
    /// </summary>
    /// <param name="size">矩阵的大小</param>
    /// <returns>DCT系数矩阵</returns>
    private static float[][] GenerateDctMatrix(int size)
    {
        var matrix = new float[size][];
        for (int i = 0; i < size; i++)
        {
            matrix[i] = new float[size];
        }

        var c1 = Math.Sqrt(2.0f / size);
        for (var j = 0; j < size; j++)
        {
            matrix[0][j] = (float)Math.Sqrt(1.0d / size);
        }

        for (var j = 0; j < size; j++)
        {
            for (var i = 1; i < size; i++)
            {
                matrix[i][j] = (float)(c1 * Math.Cos(((2 * j + 1) * i * Math.PI) / (2.0d * size)));
            }
        }

        return matrix;
    }

    /// <summary>
    /// 矩阵的乘法运算
    /// </summary>
    /// <param name="a">矩阵a</param>
    /// <param name="b">矩阵b</param>
    /// <returns>Result matrix.</returns>
    private static float[][] Multiply(float[][] a, float[][] b)
    {
        var n = a[0].Length;
        var c = new float[n][];
        for (var i = 0; i < n; i++)
        {
            c[i] = new float[n];
        }

        for (var i = 0; i < n; i++)
            for (var k = 0; k < n; k++)
                for (var j = 0; j < n; j++)
                    c[i][j] += a[i][k] * b[k][j];
        return c;
    }

    /// <summary>
    /// 矩阵转置
    /// </summary>
    /// <param name="mat">待转换的矩阵</param>
    /// <returns>转换后的矩阵</returns>
    private static float[][] Transpose(float[][] mat)
    {
        var size = mat[0].Length;
        var transpose = new float[size][];
        for (var i = 0; i < size; i++)
        {
            transpose[i] = new float[size];
            for (var j = 0; j < size; j++)
            {
                transpose[i][j] = mat[j][i];
            }
        }

        return transpose;
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
