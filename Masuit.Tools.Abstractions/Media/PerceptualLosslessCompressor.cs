using Masuit.Tools.Files;
using SkiaSharp;

namespace Masuit.Tools.Media;

public static class PerceptualLosslessCompressor
{
    private static readonly byte[] CompressionMarker = "Masuit.Tools.PerceptualCompressor\0v1"u8.ToArray();
    private const byte ApplicationMarker = 0xEF;
    private const int MinimumQuality = 90;
    private const int MaximumQuality = 100;

    public static double SsimThreshold { get; set; } = 0.99;

    /// <summary>
    /// 使用感知质量约束压缩 JPEG。这里的“无损”指感知无损，不是逐像素无损。
    /// </summary>
    /// <param name="inputPath">原图路径</param>
    /// <param name="outputPath">输出压缩JPG</param>
    public static void Compress(string inputPath, string outputPath)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException("找不到输入图片。", inputPath);
        }

        if (SsimThreshold is <= 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(SsimThreshold), "SSIM 阈值必须在 (0, 1] 范围内。");
        }

        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        using var input = File.OpenRead(inputPath);
        using var stream = Compress(input);
        stream.SaveFile(outputPath);
    }

    /// <summary>
    /// 从输入流读取图片并将感知压缩后的 JPEG 写入输出流。
    /// </summary>
    /// <param name="input">输入图片流。</param>
    public static Stream Compress(Stream input)
    {
        if (!input.CanRead)
        {
            throw new ArgumentException("输入流必须可读。", nameof(input));
        }

        if (SsimThreshold is <= 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(SsimThreshold), "SSIM 阈值必须在 (0, 1] 范围内。");
        }

        using var inputData = new MemoryStream();
        input.CopyTo(inputData);
        var inputBytes = inputData.ToArray();
        if (HasCompressionMarker(inputBytes))
        {
            var unchanged = new MemoryStream(inputBytes, writable: false);
            unchanged.Position = 0;
            return unchanged;
        }

        using var sourceData = SKData.CreateCopy(inputBytes);
        using var source = SKBitmap.Decode(sourceData) ?? throw new InvalidDataException("无法解码输入图片。");
        return LosslessCompress(source);
    }

    public static Stream LosslessCompress(this SKBitmap source)
    {
        var selectedJpeg = EncodeAtQuality(source, MaximumQuality);
        var low = MinimumQuality;
        var high = MaximumQuality;

        while (low <= high)
        {
            var quality = low + (high - low) / 2;
            var candidate = EncodeAtQuality(source, quality);
            using var candidateData = SKData.CreateCopy(candidate);
            using var decoded = SKBitmap.Decode(candidateData) ?? throw new InvalidDataException("无法解码 JPEG 候选结果。");
            var ssim = CalculateSsim(source, decoded);

            if (ssim >= SsimThreshold)
            {
                selectedJpeg = candidate;
                high = quality - 1;
            }
            else
            {
                low = quality + 1;
            }
        }

        var output = new MemoryStream(AddCompressionMarker(selectedJpeg));
        output.Position = 0;
        return output;
    }

    private static bool HasCompressionMarker(ReadOnlySpan<byte> jpeg)
    {
        if (jpeg.Length < 4 || jpeg[0] != 0xFF || jpeg[1] != 0xD8)
        {
            return false;
        }

        var offset = 2;
        while (offset + 3 < jpeg.Length)
        {
            if (jpeg[offset] != 0xFF)
            {
                offset++;
                continue;
            }

            while (offset < jpeg.Length && jpeg[offset] == 0xFF)
            {
                offset++;
            }

            if (offset >= jpeg.Length)
            {
                return false;
            }

            var marker = jpeg[offset++];
            if (marker == 0xDA || marker == 0xD9)
            {
                return false;
            }

            if (marker == 0xD8 || marker == 0x01 || marker is >= 0xD0 and <= 0xD7)
            {
                continue;
            }

            if (offset + 1 >= jpeg.Length)
            {
                return false;
            }

            var segmentLength = (jpeg[offset] << 8) | jpeg[offset + 1];
            if (segmentLength < 2 || offset + segmentLength > jpeg.Length)
            {
                return false;
            }

            if (marker == ApplicationMarker && jpeg.Slice(offset + 2, segmentLength - 2).SequenceEqual(CompressionMarker))
            {
                return true;
            }

            offset += segmentLength;
        }

        return false;
    }

    private static byte[] AddCompressionMarker(byte[] jpeg)
    {
        var segmentLength = CompressionMarker.Length + 2;
        var result = new byte[jpeg.Length + CompressionMarker.Length + 4];
        result[0] = 0xFF;
        result[1] = 0xD8;
        result[2] = 0xFF;
        result[3] = ApplicationMarker;
        result[4] = (byte)(segmentLength >> 8);
        result[5] = (byte)segmentLength;
        CompressionMarker.CopyTo(result, 6);
        Buffer.BlockCopy(jpeg, 2, result, 6 + CompressionMarker.Length, jpeg.Length - 2);
        return result;
    }

    private static byte[] EncodeAtQuality(SKBitmap source, int quality)
    {
        using var image = SKImage.FromBitmap(source);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
        return data.ToArray();
    }

    private static double CalculateSsim(SKBitmap source, SKBitmap candidate)
    {
        if (source.Width != candidate.Width || source.Height != candidate.Height)
        {
            return 0;
        }

        const int sampleLimit = 256;
        var stepX = Math.Max(1, source.Width / sampleLimit);
        var stepY = Math.Max(1, source.Height / sampleLimit);
        var valuesA = new List<double>();
        var valuesB = new List<double>();

        for (var y = 0; y < source.Height; y += stepY)
        {
            for (var x = 0; x < source.Width; x += stepX)
            {
                valuesA.Add(Luminance(source.GetPixel(x, y)));
                valuesB.Add(Luminance(candidate.GetPixel(x, y)));
            }
        }

        var meanA = valuesA.Average();
        var meanB = valuesB.Average();
        var varianceA = 0d;
        var varianceB = 0d;
        var covariance = 0d;

        for (var index = 0; index < valuesA.Count; index++)
        {
            var differenceA = valuesA[index] - meanA;
            var differenceB = valuesB[index] - meanB;
            varianceA += differenceA * differenceA;
            varianceB += differenceB * differenceB;
            covariance += differenceA * differenceB;
        }

        var count = Math.Max(1, valuesA.Count - 1);
        varianceA /= count;
        varianceB /= count;
        covariance /= count;

        const double c1 = 6.5025;
        const double c2 = 58.5225;
        return ((2 * meanA * meanB + c1) * (2 * covariance + c2)) / ((meanA * meanA + meanB * meanB + c1) * (varianceA + varianceB + c2));
    }

    private static double Luminance(SKColor pixel)
    {
        return 0.2126 * pixel.Red + 0.7152 * pixel.Green + 0.0722 * pixel.Blue;
    }
}
