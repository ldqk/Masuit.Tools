using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Masuit.Tools.Media;

internal static class DctHasher
{
    private const int Size = 64;
    private static readonly double Sqrt2DivSize = Math.Sqrt(2D / Size);
    private static readonly double Sqrt2 = 1 / Math.Sqrt(2);
    private static readonly List<Vector<double>>[] DctCoeffsSimd = GenerateDctCoeffsSIMD();

    public static ulong Compute(Image<L8> image)
    {
        if (image == null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        using var clone = image.Clone(ctx => ctx.Resize(new ResizeOptions()
        {
            Size = new Size
            {
                Width = Size,
                Height = Size
            },
            Mode = ResizeMode.Stretch,
            Sampler = new BicubicResampler()
        }));
        var rows = new double[Size, Size];
        var sequence = new double[Size];
        var matrix = new double[Size, Size];

        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                sequence[x] = clone[x, y].PackedValue;
            }

            Dct1D_SIMD(sequence, rows, y);
        }

        for (var x = 0; x < 8; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                sequence[y] = rows[y, x];
            }

            Dct1D_SIMD(sequence, matrix, x, limit: 8);
        }

        var top8X8 = new double[Size];
        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 8; x++)
            {
                top8X8[(y * 8) + x] = matrix[y, x];
            }
        }

        var median = CalculateMedian64(top8X8);
        var mask = 1UL << (Size - 1);
        var hash = 0UL;

        for (var i = 0; i < Size; i++)
        {
            if (top8X8[i] > median)
            {
                hash |= mask;
            }

            mask >>= 1;
        }

        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double CalculateMedian64(IReadOnlyCollection<double> values)
    {
        return values.OrderBy(x => x).Skip(31).Take(2).Average();
    }

    private static List<Vector<double>>[] GenerateDctCoeffsSIMD()
    {
        var results = new List<Vector<double>>[Size];
        for (var coef = 0; coef < Size; coef++)
        {
            var singleResultRaw = new double[Size];
            for (var i = 0; i < Size; i++)
            {
                singleResultRaw[i] = Math.Cos(((2.0 * i) + 1.0) * coef * Math.PI / (2.0 * Size));
            }

            var singleResultList = new List<Vector<double>>();
            var stride = Vector<double>.Count;
            for (var i = 0; i < Size; i += stride)
            {
                var v = new Vector<double>(singleResultRaw, i);
                singleResultList.Add(v);
            }

            results[coef] = singleResultList;
        }

        return results;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Dct1D_SIMD(double[] valuesRaw, double[,] coefficients, int ci, int limit = Size)
    {
        var valuesList = new List<Vector<double>>();
        var stride = Vector<double>.Count;
        for (var i = 0; i < valuesRaw.Length; i += stride)
        {
            valuesList.Add(new Vector<double>(valuesRaw, i));
        }

        for (var coef = 0; coef < limit; coef++)
        {
            for (var i = 0; i < valuesList.Count; i++)
            {
                coefficients[ci, coef] += Vector.Dot(valuesList[i], DctCoeffsSimd[coef][i]);
            }

            coefficients[ci, coef] *= Sqrt2DivSize;
            if (coef == 0)
            {
                coefficients[ci, coef] *= Sqrt2;
            }
        }
    }
}