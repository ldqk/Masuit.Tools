using SkiaSharp;

namespace Masuit.Tools.Media;

internal static class PdqHasher
{
    private const int HashSize = 16;
    private const int DownsampleSize = 64;
    private const int JaroszWindowSizeDivisor = 128;
    private const int JaroszPassCount = 2;
    private const int HashBitCount = HashSize * HashSize;
    private const int ParallelPixelThreshold = 1_000_000;
    private const int ColumnTileSize = 128;
    private static readonly float[,] DctMatrix = BuildDctMatrix();

    public static ulong[] Compute(SKBitmap image, out int quality)
    {
        if (image == null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (image.Width <= 0 || image.Height <= 0)
        {
            throw new ArgumentException("图像宽度和高度必须大于0。", nameof(image));
        }

        var pixelCount = checked(image.Width * image.Height);
        var pixels = new float[pixelCount];
        FillLuma(image, pixels);

        var scratch = new float[pixelCount];
        var rowWindowSize = (image.Width + JaroszWindowSizeDivisor - 1) / JaroszWindowSizeDivisor;
        var columnWindowSize = (image.Height + JaroszWindowSizeDivisor - 1) / JaroszWindowSizeDivisor;
        for (var pass = 0; pass < JaroszPassCount; pass++)
        {
            BoxAlongRows(pixels, scratch, image.Width, image.Height, rowWindowSize);
            BoxAlongColumns(scratch, pixels, image.Width, image.Height, columnWindowSize);
        }

        var downsampled = new float[DownsampleSize * DownsampleSize];
        for (var y = 0; y < DownsampleSize; y++)
        {
            var sourceY = (int) (((y + 0.5) * image.Height) / DownsampleSize);
            for (var x = 0; x < DownsampleSize; x++)
            {
                var sourceX = (int) (((x + 0.5) * image.Width) / DownsampleSize);
                downsampled[y * DownsampleSize + x] = pixels[sourceY * image.Width + sourceX];
            }
        }

        quality = ComputeQuality(downsampled);
        return ComputeHash(downsampled);
    }

    private static void FillLuma(SKBitmap image, float[] pixels)
    {
        var colorType = image.ColorType;
        var alphaType = image.AlphaType;
        var supportedColorType = colorType is SKColorType.Rgba8888 or SKColorType.Bgra8888;
        if (supportedColorType && (alphaType == SKAlphaType.Opaque || alphaType == SKAlphaType.Unpremul || (alphaType == SKAlphaType.Premul && HasOpaquePixels(image))))
        {
            var redOffset = colorType == SKColorType.Rgba8888 ? 0 : 2;
            var blueOffset = colorType == SKColorType.Rgba8888 ? 2 : 0;
            FillLumaFast(image, pixels, redOffset, blueOffset);
            return;
        }

        FillLumaUsingGetPixel(image, pixels);
    }

    private static unsafe void FillLumaFast(SKBitmap image, float[] pixels, int redOffset, int blueOffset)
    {
        var source = image.GetPixels();
        if (source == IntPtr.Zero)
        {
            FillLumaUsingGetPixel(image, pixels);
            return;
        }

        var width = image.Width;
        var height = image.Height;
        var rowBytes = image.RowBytes;
        if (pixels.Length >= ParallelPixelThreshold)
        {
            Parallel.For(0, height, y =>
            {
                var sourceRow = (byte*) source.ToPointer() + y * rowBytes;
                var destinationOffset = y * width;
                for (var x = 0; x < width; x++)
                {
                    var pixelOffset = x * 4;
                    pixels[destinationOffset + x] = (0.299f * sourceRow[pixelOffset + redOffset]) + (0.587f * sourceRow[pixelOffset + 1]) + (0.114f * sourceRow[pixelOffset + blueOffset]);
                }
            });
            return;
        }

        for (var y = 0; y < height; y++)
        {
            var sourceRow = (byte*) source.ToPointer() + y * rowBytes;
            var destinationOffset = y * width;
            for (var x = 0; x < width; x++)
            {
                var pixelOffset = x * 4;
                pixels[destinationOffset + x] = (0.299f * sourceRow[pixelOffset + redOffset]) + (0.587f * sourceRow[pixelOffset + 1]) + (0.114f * sourceRow[pixelOffset + blueOffset]);
            }
        }
    }

    private static bool HasOpaquePixels(SKBitmap image)
    {
        var pixels = image.GetPixelSpan();
        var rowBytes = image.RowBytes;
        for (var y = 0; y < image.Height; y++)
        {
            var rowOffset = y * rowBytes;
            for (var x = 0; x < image.Width; x++)
            {
                if (pixels[rowOffset + x * 4 + 3] != byte.MaxValue)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static void FillLumaUsingGetPixel(SKBitmap image, float[] pixels)
    {
        for (var y = 0; y < image.Height; y++)
        {
            var rowOffset = y * image.Width;
            for (var x = 0; x < image.Width; x++)
            {
                var color = image.GetPixel(x, y);
                pixels[rowOffset + x] = (0.299f * color.Red) + (0.587f * color.Green) + (0.114f * color.Blue);
            }
        }
    }

    private static void BoxAlongRows(float[] input, float[] output, int width, int height, int windowSize)
    {
        if (input.Length >= ParallelPixelThreshold)
        {
            Parallel.For(0, height, y =>
            {
                BoxFilterLine(input, y * width, output, y * width, width, 1, windowSize);
            });
            return;
        }

        for (var y = 0; y < height; y++)
        {
            BoxFilterLine(input, y * width, output, y * width, width, 1, windowSize);
        }
    }

    private static void BoxAlongColumns(float[] input, float[] output, int width, int height, int windowSize)
    {
        var tileCount = (width + ColumnTileSize - 1) / ColumnTileSize;
        if (input.Length >= ParallelPixelThreshold)
        {
            Parallel.For(0, tileCount, tile =>
            {
                BoxColumnTile(input, output, width, height, windowSize, tile);
            });
            return;
        }

        for (var tile = 0; tile < tileCount; tile++)
        {
            BoxColumnTile(input, output, width, height, windowSize, tile);
        }
    }

    private static void BoxColumnTile(float[] input, float[] output, int width, int height, int windowSize, int tile)
    {
        var startX = tile * ColumnTileSize;
        var tileWidth = Math.Min(ColumnTileSize, width - startX);
        var halfWindowSize = (windowSize + 2) / 2;
        var sums = new float[tileWidth];
        Array.Copy(input, startX, sums, 0, tileWidth);
        var left = 0;
        var right = 0;
        for (var y = 0; y < height; y++)
        {
            var targetLeft = Math.Max(0, y - halfWindowSize + 1);
            var targetRight = Math.Min(height - 1, y + windowSize - halfWindowSize);
            while (right < targetRight)
            {
                right++;
                var rowOffset = right * width + startX;
                for (var x = 0; x < tileWidth; x++)
                {
                    sums[x] += input[rowOffset + x];
                }
            }

            while (left < targetLeft)
            {
                var rowOffset = left * width + startX;
                for (var x = 0; x < tileWidth; x++)
                {
                    sums[x] -= input[rowOffset + x];
                }

                left++;
            }

            var outputOffset = y * width + startX;
            var windowCount = right - left + 1;
            for (var x = 0; x < tileWidth; x++)
            {
                output[outputOffset + x] = sums[x] / windowCount;
            }
        }
    }

    private static void BoxFilterLine(float[] input, int inputOffset, float[] output, int outputOffset, int length, int stride, int windowSize)
    {
        var halfWindowSize = (windowSize + 2) / 2;
        var left = 0;
        var right = 0;
        var sum = input[inputOffset];

        for (var i = 0; i < length; i++)
        {
            var targetLeft = Math.Max(0, i - halfWindowSize + 1);
            var targetRight = Math.Min(length - 1, i + windowSize - halfWindowSize);
            while (right < targetRight)
            {
                right++;
                sum += input[inputOffset + right * stride];
            }

            while (left < targetLeft)
            {
                sum -= input[inputOffset + left * stride];
                left++;
            }

            output[outputOffset + i * stride] = sum / (right - left + 1);
        }
    }

    private static int ComputeQuality(float[] pixels)
    {
        var gradientSum = 0;
        for (var y = 0; y < DownsampleSize - 1; y++)
        {
            for (var x = 0; x < DownsampleSize; x++)
            {
                var difference = (int) (((pixels[y * DownsampleSize + x] - pixels[(y + 1) * DownsampleSize + x]) * 100) / 255);
                gradientSum += Math.Abs(difference);
            }
        }

        for (var y = 0; y < DownsampleSize; y++)
        {
            for (var x = 0; x < DownsampleSize - 1; x++)
            {
                var difference = (int) (((pixels[y * DownsampleSize + x] - pixels[y * DownsampleSize + x + 1]) * 100) / 255);
                gradientSum += Math.Abs(difference);
            }
        }

        return Math.Min(100, gradientSum / 90);
    }

    private static ulong[] ComputeHash(float[] pixels)
    {
        var rowDct = new float[HashSize * DownsampleSize];
        var coefficients = new float[HashBitCount];

        for (var frequency = 0; frequency < HashSize; frequency++)
        {
            for (var y = 0; y < DownsampleSize; y++)
            {
                var sum = 0f;
                var pixelOffset = y * DownsampleSize;
                for (var x = 0; x < DownsampleSize; x++)
                {
                    sum += DctMatrix[frequency, x] * pixels[pixelOffset + x];
                }

                rowDct[frequency * DownsampleSize + y] = sum;
            }
        }

        for (var yFrequency = 0; yFrequency < HashSize; yFrequency++)
        {
            for (var xFrequency = 0; xFrequency < HashSize; xFrequency++)
            {
                var sum = 0f;
                var rowOffset = yFrequency * DownsampleSize;
                for (var y = 0; y < DownsampleSize; y++)
                {
                    sum += rowDct[rowOffset + y] * DctMatrix[xFrequency, y];
                }

                coefficients[yFrequency * HashSize + xFrequency] = sum;
            }
        }

        var sorted = (float[]) coefficients.Clone();
        Array.Sort(sorted);
        var median = sorted[HashBitCount / 2 - 1];
        var hash = new ulong[HashBitCount / 64];
        for (var i = 0; i < coefficients.Length; i++)
        {
            if (coefficients[i] > median)
            {
                hash[i / 64] |= 1UL << (i % 64);
            }
        }

        return hash;
    }

    private static float[,] BuildDctMatrix()
    {
        var matrix = new float[HashSize, DownsampleSize];
        var scale = Math.Sqrt(2.0 / DownsampleSize);
        for (var frequency = 0; frequency < HashSize; frequency++)
        {
            for (var sample = 0; sample < DownsampleSize; sample++)
            {
                matrix[frequency, sample] = (float) (scale * Math.Cos((Math.PI / 2 / DownsampleSize) * (frequency + 1) * (2 * sample + 1)));
            }
        }

        return matrix;
    }
}