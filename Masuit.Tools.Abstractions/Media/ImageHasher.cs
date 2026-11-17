using SkiaSharp;

namespace Masuit.Tools.Media;

/// <summary>
/// 图像hash计算器
/// </summary>
public class ImageHasher
{
    // 预计算 DCT 余弦系数表，size=32：table[u,x] = cos((pi/32) * (x+0.5) * u)
    // 避免每次 ComputeDct 调用时执行 32*32=1024 次 Math.Cos
    private static readonly double[,] DctCosTable32 = BuildDctCosTable(32);
    private static readonly double Dct32Scale = Math.Sqrt(2.0 / 32);
    private static readonly double DctCu32Zero = 1.0 / Math.Sqrt(2.0);
    private static readonly double[,] MarrHildrethKernel = BuildMarrHildrethKernel();

    private static double[,] BuildDctCosTable(int n)
    {
        var table = new double[n, n];
        for (int u = 0; u < n; u++)
        for (int x = 0; x < n; x++)
            table[u, x] = Math.Cos((Math.PI / n) * (x + 0.5) * u);
        return table;
    }

    private readonly IImageTransformer _transformer;

    public ImageHasher()
    {
        _transformer = new SkiaSharpTransformer();
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
        using var image = SkiaImageHelper.DecodeGrayThumb(pathToImage, 160);
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
            if (pixels[i] > average) hash |= 1UL << i;
        }

        return hash;
    }
    /// <summary>
    /// 使用平均值算法计算图像的64位哈希
    /// </summary>
    /// <param name="image">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong AverageHash64(SKBitmap image)
    {
        var pixels = _transformer.TransformImage(image, 8, 8);
        var average = pixels.Sum(b => b) / 64;

        // 遍历所有像素，如果超过平均值，则将其设置为1，如果低于平均值，则将其设置为0。
        var hash = 0UL;
        for (var i = 0; i < 64; i++)
        {
            if (pixels[i] > average) hash |= 1UL << i;
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
        using var image = SkiaImageHelper.DecodeGrayThumb(pathToImage, 160);
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
        var sorted = new byte[64];
        Array.Copy(pixels, sorted, 64);
        Array.Sort(sorted);
        var median = (byte) ((sorted[31] + sorted[32]) / 2);
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
    public ulong MedianHash64(SKBitmap image)
    {
        var pixels = _transformer.TransformImage(image, 8, 8);
        var sorted = new byte[64];
        Array.Copy(pixels, sorted, 64);
        Array.Sort(sorted);
        var median = (byte) ((sorted[31] + sorted[32]) / 2);
        var hash = 0UL;
        for (var i = 0; i < 64; i++)
        {
            if (pixels[i] > median) hash |= 1UL << i;
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
        using var image = SkiaImageHelper.DecodeGrayThumb(pathToImage, 160);
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
        var sorted = new byte[256];
        Array.Copy(pixels, sorted, 256);
        Array.Sort(sorted);
        var median = (byte) ((sorted[127] + sorted[128]) / 2);
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
    public ulong[] MedianHash256(SKBitmap image)
    {
        var pixels = _transformer.TransformImage(image, 16, 16);
        var sorted = new byte[256];
        Array.Copy(pixels, sorted, 256);
        Array.Sort(sorted);
        var median = (byte) ((sorted[127] + sorted[128]) / 2);
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
        using var image = SkiaImageHelper.DecodeGrayThumb(pathToImage, 160);
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
    public ulong DifferenceHash64(SKBitmap image)
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
        using var image = SkiaImageHelper.DecodeGrayThumb(pathToImage, 160);
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
    public ulong[] DifferenceHash256(SKBitmap image)
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
    /// 使用Marr-Hildreth算子计算图像的64位哈希。
    /// </summary>
    /// <param name="pathToImage">图片的文件路径</param>
    /// <returns>64位hash值</returns>
    public ulong MHHash64(string pathToImage)
    {
        using var image = SkiaImageHelper.DecodeGrayThumb(pathToImage, 160);
        return MHHash64(image);
    }

    /// <summary>
    /// 使用Marr-Hildreth算子计算图像的64位哈希。
    /// </summary>
    /// <param name="sourceStream">读取到的图片流</param>
    /// <returns>64位hash值</returns>
    public ulong MHHash64(Stream sourceStream)
    {
        return CreateHash(ComputeMarrHildrethHash(_transformer.TransformImage(sourceStream, 32, 32), 8));
    }

    /// <summary>
    /// 使用Marr-Hildreth算子计算图像的64位哈希。
    /// </summary>
    /// <param name="image">读取到的图片</param>
    /// <returns>64位hash值</returns>
    public ulong MHHash64(SKBitmap image)
    {
        return CreateHash(ComputeMarrHildrethHash(_transformer.TransformImage(image, 32, 32), 8));
    }

    /// <summary>
    /// 使用Marr-Hildreth算子计算图像的256位哈希。
    /// </summary>
    /// <param name="pathToImage">图片的文件路径</param>
    /// <returns>由4个64位整数组成的256位哈希</returns>
    public ulong[] MHHash256(string pathToImage)
    {
        using var image = SkiaImageHelper.DecodeGrayThumb(pathToImage, 160);
        return MHHash256(image);
    }

    /// <summary>
    /// 使用Marr-Hildreth算子计算图像的256位哈希。
    /// </summary>
    /// <param name="sourceStream">读取到的图片流</param>
    /// <returns>由4个64位整数组成的256位哈希</returns>
    public ulong[] MHHash256(Stream sourceStream)
    {
        return CreateHash256(ComputeMarrHildrethHash(_transformer.TransformImage(sourceStream, 64, 64), 16));
    }

    /// <summary>
    /// 使用Marr-Hildreth算子计算图像的256位哈希。
    /// </summary>
    /// <param name="image">读取到的图片</param>
    /// <returns>由4个64位整数组成的256位哈希</returns>
    public ulong[] MHHash256(SKBitmap image)
    {
        return CreateHash256(ComputeMarrHildrethHash(_transformer.TransformImage(image, 64, 64), 16));
    }

    /// <summary>
    /// 使用32分辨率精度DCT算法计算图像的64位哈希
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="path">图片路径</param>
    /// <returns>64位hash值</returns>
    public ulong DctHash(string path)
    {
        using var image = SkiaImageHelper.DecodeGrayThumb(path, 160);
        return DctHash(image);
    }

    /// <summary>
    /// 使用32分辨率精度DCT算法计算图像的64位哈希
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片</param>
    /// <returns>64位hash值</returns>
    public ulong DctHash(SKBitmap image)
    {
        var grayscalePixels = _transformer.GetPixelData(image, 32, 32);
        var dctMatrix = ComputeDct(grayscalePixels, 32);

        // 内联 ExtractTopLeftBlock + CalculateMedian + GenerateHash，消除中间分配
        var flatBlock = new double[64];
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
            flatBlock[y * 8 + x] = dctMatrix[y, x];

        // 对副本排序求中值，保留原数组用于哈希生成
        var sorted = new double[64];
        Array.Copy(flatBlock, sorted, 64);
        Array.Sort(sorted);
        double median = (sorted[31] + sorted[32]) / 2.0;

        ulong hash = 0UL;
        for (int i = 0; i < 64; i++)
            if (flatBlock[i] >= median)
                hash |= 1UL << i;
        return hash;
    }

    /// <summary>
    /// 使用64分辨率精度DCT算法计算图像的64位哈希
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="path">图片路径</param>
    /// <returns>64位hash值</returns>
    public ulong DctHash64(string path)
    {
        using var image = SkiaImageHelper.DecodeGrayThumb(path, 160);
        return DctHash64(image);
    }

    /// <summary>
    /// 使用64分辨率精度DCT算法计算图像的64位哈希
    /// </summary>
    /// <see cref="https://segmentfault.com/a/1190000038308093"/>
    /// <param name="image">读取到的图片</param>
    /// <returns>64位hash值</returns>
    public ulong DctHash64(SKBitmap image)
    {
        return DctHasher.Compute(image);
    }

    /// <summary>
    /// 使用64x64像素图像和16x16低频DCT系数计算256位哈希。
    /// 比64位DCT哈希保留更多图像信息，但对缩放、裁剪和其他编辑的鲁棒性仍取决于图像内容。
    /// </summary>
    /// <param name="path">图片路径</param>
    /// <returns>由4个64位整数组成的256位哈希</returns>
    public ulong[] DctHash256(string path)
    {
        using var image = SkiaImageHelper.DecodeGrayThumb(path, 160);
        return DctHash256(image);
    }

    /// <summary>
    /// 使用64x64像素图像和16x16低频DCT系数计算256位哈希。
    /// 比64位DCT哈希保留更多图像信息，但对缩放、裁剪和其他编辑的鲁棒性仍取决于图像内容。
    /// </summary>
    /// <param name="image">读取到的图片</param>
    /// <returns>由4个64位整数组成的256位哈希</returns>
    public ulong[] DctHash256(SKBitmap image)
    {
        return DctHasher.Compute256(image);
    }

    private static bool[] ComputeWaveletHash(byte[] pixels, int hashSize)
    {
        const int imageSize = 64;
        var coefficients = new double[imageSize, imageSize];
        for (var y = 0; y < imageSize; y++)
        for (var x = 0; x < imageSize; x++)
            coefficients[y, x] = pixels[y * imageSize + x] / 255.0;

        var fullLevelCount = 6;
        HaarForward(coefficients, imageSize, fullLevelCount);
        coefficients[0, 0] = 0;
        HaarInverse(coefficients, imageSize, fullLevelCount);

        var levelCount = 0;
        for (var size = imageSize; size > hashSize; size >>= 1)
            levelCount++;
        HaarForward(coefficients, imageSize, levelCount);

        var lowFrequencies = new double[hashSize * hashSize];
        for (var y = 0; y < hashSize; y++)
        for (var x = 0; x < hashSize; x++)
            lowFrequencies[y * hashSize + x] = coefficients[y, x];

        var sorted = (double[])lowFrequencies.Clone();
        Array.Sort(sorted);
        var median = (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2.0;
        var bits = new bool[lowFrequencies.Length];
        for (var i = 0; i < bits.Length; i++)
            bits[i] = lowFrequencies[i] > median;
        return bits;
    }

    private static void HaarForward(double[,] data, int imageSize, int levelCount)
    {
        var temp = new double[imageSize];
        var scale = 1.0 / Math.Sqrt(2.0);
        var activeSize = imageSize;
        for (var level = 0; level < levelCount; level++, activeSize >>= 1)
        {
            for (var y = 0; y < activeSize; y++)
            {
                for (var x = 0; x < activeSize / 2; x++)
                {
                    var left = data[y, x * 2];
                    var right = data[y, x * 2 + 1];
                    temp[x] = (left + right) * scale;
                    temp[activeSize / 2 + x] = (left - right) * scale;
                }

                for (var x = 0; x < activeSize; x++)
                    data[y, x] = temp[x];
            }

            for (var x = 0; x < activeSize; x++)
            {
                for (var y = 0; y < activeSize / 2; y++)
                {
                    var top = data[y * 2, x];
                    var bottom = data[y * 2 + 1, x];
                    temp[y] = (top + bottom) * scale;
                    temp[activeSize / 2 + y] = (top - bottom) * scale;
                }

                for (var y = 0; y < activeSize; y++)
                    data[y, x] = temp[y];
            }
        }
    }

    private static void HaarInverse(double[,] data, int imageSize, int levelCount)
    {
        var temp = new double[imageSize];
        var scale = 1.0 / Math.Sqrt(2.0);
        var activeSize = imageSize >> (levelCount - 1);
        for (var level = levelCount - 1; level >= 0; level--, activeSize <<= 1)
        {
            for (var x = 0; x < activeSize; x++)
            {
                for (var y = 0; y < activeSize / 2; y++)
                {
                    var low = data[y, x];
                    var high = data[activeSize / 2 + y, x];
                    temp[y * 2] = (low + high) * scale;
                    temp[y * 2 + 1] = (low - high) * scale;
                }

                for (var y = 0; y < activeSize; y++)
                    data[y, x] = temp[y];
            }

            for (var y = 0; y < activeSize; y++)
            {
                for (var x = 0; x < activeSize / 2; x++)
                {
                    var low = data[y, x];
                    var high = data[y, activeSize / 2 + x];
                    temp[x * 2] = (low + high) * scale;
                    temp[x * 2 + 1] = (low - high) * scale;
                }

                for (var x = 0; x < activeSize; x++)
                    data[y, x] = temp[x];
            }
        }
    }

    private static bool[] ComputeMarrHildrethHash(byte[] pixels, int hashSize)
    {
        var imageSize = hashSize * 4;
        var responses = new double[imageSize * imageSize];
        const int radius = 2;
        for (var y = 0; y < imageSize; y++)
        for (var x = 0; x < imageSize; x++)
        {
            double response = 0;
            for (var ky = -radius; ky <= radius; ky++)
            for (var kx = -radius; kx <= radius; kx++)
            {
                var sampleX = Math.Min(Math.Max(x + kx, 0), imageSize - 1);
                var sampleY = Math.Min(Math.Max(y + ky, 0), imageSize - 1);
                response += pixels[sampleY * imageSize + sampleX] * MarrHildrethKernel[ky + radius, kx + radius];
            }

            responses[y * imageSize + x] = response;
        }

        var sorted = (double[])responses.Clone();
        Array.Sort(sorted);
        var median = (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2.0;
        var bits = new bool[hashSize * hashSize];
        for (var y = 0; y < hashSize; y++)
        for (var x = 0; x < hashSize; x++)
        {
            double blockResponse = 0;
            for (var by = 0; by < 4; by++)
            for (var bx = 0; bx < 4; bx++)
                blockResponse += responses[(y * 4 + by) * imageSize + x * 4 + bx];
            bits[y * hashSize + x] = blockResponse > median * 16;
        }

        return bits;
    }

    private static double[,] BuildMarrHildrethKernel()
    {
        const int radius = 2;
        const double sigma = 1.0;
        var kernel = new double[radius * 2 + 1, radius * 2 + 1];
        double sum = 0;
        for (var y = -radius; y <= radius; y++)
        for (var x = -radius; x <= radius; x++)
        {
            var distanceSquared = x * x + y * y;
            var value = (distanceSquared - 2 * sigma * sigma) *
                        Math.Exp(-distanceSquared / (2 * sigma * sigma));
            kernel[y + radius, x + radius] = value;
            sum += value;
        }

        var mean = sum / (kernel.GetLength(0) * kernel.GetLength(1));
        for (var y = 0; y < kernel.GetLength(0); y++)
        for (var x = 0; x < kernel.GetLength(1); x++)
            kernel[y, x] -= mean;
        return kernel;
    }

    private static ulong CreateHash(bool[] bits)
    {
        ulong hash = 0;
        for (var i = 0; i < bits.Length; i++)
            if (bits[i])
                hash |= 1UL << i;
        return hash;
    }

    private static ulong[] CreateHash256(bool[] bits)
    {
        var hash = new ulong[4];
        for (var i = 0; i < bits.Length; i++)
            if (bits[i])
                hash[i / 64] |= 1UL << (i % 64);
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
        // 复用预计算余弦表，从 size*size*64 次 Math.Cos + 64 数组分配
        // 降低到只分配 2 个 double[size,size]
        var rowDCT = new double[size, size];
        var output = new double[size, size];
        double scale = Dct32Scale;
        double cu0 = DctCu32Zero;

        // 对每行做 1D DCT
        for (int y = 0; y < size; y++)
        {
            for (int u = 0; u < size; u++)
            {
                double sum = 0.0;
                for (int x = 0; x < size; x++)
                {
                    sum += input[y, x] * DctCosTable32[u, x];
                }

                rowDCT[y, u] = ((u == 0) ? cu0 : 1.0) * sum * scale;
            }
        }

        // 对每列做 1D DCT
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                double sum = 0.0;
                for (int t = 0; t < size; t++)
                {
                    sum += rowDCT[t, x] * DctCosTable32[y, t];
                }

                output[y, x] = ((y == 0) ? cu0 : 1.0) * sum * scale;
            }
        }

        return output;
    }

    /// <summary>
    /// 使用汉明距离比较两幅图像的哈希值。结果1表示图像完全相同，而结果0表示图像完全不同。
    /// </summary>
    /// <param name="hash1">图像1的hash</param>
    /// <param name="hash2">图像2的hash</param>
    /// <returns>相似度范围：[0,1]</returns>
    public static float Compare(ulong hash1, ulong hash2)
    {
        var hashDifference = hash1 ^ hash2;
        var hamming = HammingWeight(hashDifference);
        return 1.0f - hamming / 64.0f;
    }

    /// <summary>
    /// 使用汉明距离比较两幅图像的哈希值。结果1表示图像完全相同，而结果0表示图像完全不同。
    /// </summary>
    /// <param name="hash1">图像1的hash</param>
    /// <param name="hash2">图像2的hash</param>
    /// <returns>相似度范围：[0,1]</returns>
    public static float Compare(ulong[] hash1, ulong[] hash2)
    {
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

        return 1.0f - onesInHash / (hashSize * 64.0f);
    }

    private static ulong HammingWeight(ulong hash)
    {
        hash -= (hash >> 1) & M1;
        hash = (hash & M2) + ((hash >> 2) & M2);
        hash = (hash + (hash >> 4)) & M4;
        return (hash * H01) >> 56;
    }

    // 汉明距离常量. http://en.wikipedia.org/wiki/Hamming_weight
    private const ulong M1 = 0x5555555555555555;
    private const ulong M2 = 0x3333333333333333;
    private const ulong M4 = 0x0f0f0f0f0f0f0f0f;
    private const ulong H01 = 0x0101010101010101;
}