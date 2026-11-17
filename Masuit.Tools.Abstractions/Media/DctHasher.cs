using System.Numerics;
using System.Runtime.CompilerServices;
using SkiaSharp;

namespace Masuit.Tools.Media;

internal static class DctHasher
{
    private const int Size = 64;
    private static readonly double Sqrt2DivSize = Math.Sqrt(2D / Size);
    private static readonly double Sqrt2 = 1 / Math.Sqrt(2);
    private static readonly int VectorStride = Vector<double>.Count;
    private static readonly int VectorCount = Size / Vector<double>.Count;
    // 预计算 DCT 系数，改用 Vector<double>[][] 避免每次调用创建 List<T>
    private static readonly Vector<double>[][] DctCoeffsSimd = GenerateDctCoeffsSIMD();

    // 每线程复用的向量缓冲，避免每次 Dct1D_SIMD 调用分配 List
    [ThreadStatic]
    private static Vector<double>[]? _valuesBuffer;

    public static ulong Compute(SKBitmap image)
    {
        return Compute(image, 8)[0];
    }

    public static ulong[] Compute256(SKBitmap image)
    {
        return Compute(image, 16);
    }

    private static ulong[] Compute(SKBitmap image, int hashSize)
    {
        if (image == null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        var imageInfo = new SKImageInfo(Size, Size, SKColorType.Gray8, SKAlphaType.Opaque);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // 源图片完整矩形
        var sourceRect = new SKRect(0, 0, image.Width, image.Height);
        var destRect = new SKRect(0, 0, Size, Size);

        // 绘制：不做比例适配，直接拉伸填充
        canvas.DrawBitmap(image, sourceRect, destRect, new SKSamplingOptions(SKFilterMode.Linear));
        var rows = new double[Size, Size];
        var sequence = new double[Size];
        var matrix = new double[Size, Size];

        // 用不安全指针直接读取 Gray8 像素，替代 4096 次 GetPixel() 互操作调用
        unsafe
        {
            using var pixels = surface.PeekPixels();
            var ptr = (byte*)pixels.GetPixels().ToPointer();
            for (var y = 0; y < Size; y++)
            {
                var rowPtr = ptr + y * Size;
                for (var x = 0; x < Size; x++)
                {
                    sequence[x] = rowPtr[x];
                }

                Dct1D_SIMD(sequence, rows, y);
            }
        }

        for (var x = 0; x < hashSize; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                sequence[y] = rows[y, x];
            }

            Dct1D_SIMD(sequence, matrix, x, limit: hashSize);
        }

        var topLeftBlock = new double[hashSize * hashSize];
        for (var y = 0; y < hashSize; y++)
        {
            for (var x = 0; x < hashSize; x++)
            {
                topLeftBlock[(y * hashSize) + x] = matrix[y, x];
            }
        }

        var median = CalculateMedian(topLeftBlock);
        var hash = new ulong[(topLeftBlock.Length + 63) / 64];

        for (var i = 0; i < topLeftBlock.Length; i++)
        {
            if (topLeftBlock[i] > median)
            {
                hash[i / 64] |= 1UL << (63 - (i % 64));
            }
        }

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double CalculateMedian(double[] values)
    {
        // 用数组拷贝+Array.Sort替代LINQ OrderBy，避免IEnumerable枚举器分配
        var copy = new double[values.Length];
        Array.Copy(values, copy, values.Length);
        Array.Sort(copy);
        var middle = copy.Length / 2;
        return (copy[middle - 1] + copy[middle]) / 2.0;
    }

    private static Vector<double>[][] GenerateDctCoeffsSIMD()
    {
        var stride = Vector<double>.Count;
        var vectorCount = Size / stride;
        var results = new Vector<double>[Size][];
        for (var coef = 0; coef < Size; coef++)
        {
            var singleResultRaw = new double[Size];
            for (var i = 0; i < Size; i++)
            {
                singleResultRaw[i] = Math.Cos(((2.0 * i) + 1.0) * coef * Math.PI / (2.0 * Size));
            }

            var vectors = new Vector<double>[vectorCount];
            for (var i = 0; i < vectorCount; i++)
            {
                vectors[i] = new Vector<double>(singleResultRaw, i * stride);
            }

            results[coef] = vectors;
        }

        return results;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Dct1D_SIMD(double[] valuesRaw, double[,] coefficients, int ci, int limit = Size)
    {
        // 复用线程本地缓冲，避免每次调用创建新的 List<Vector<double>>
        var buf = _valuesBuffer ??= new Vector<double>[VectorCount];
        for (var i = 0; i < VectorCount; i++)
        {
            buf[i] = new Vector<double>(valuesRaw, i * VectorStride);
        }

        var dctCoeffs = DctCoeffsSimd;
        for (var coef = 0; coef < limit; coef++)
        {
            var coeffVecs = dctCoeffs[coef];
            double sum = 0;
            for (var i = 0; i < VectorCount; i++)
            {
                sum += Vector.Dot(buf[i], coeffVecs[i]);
            }

            coefficients[ci, coef] = sum * Sqrt2DivSize;
            if (coef == 0)
            {
                coefficients[ci, coef] *= Sqrt2;
            }
        }
    }
}