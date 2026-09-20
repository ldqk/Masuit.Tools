using System.Drawing;
using SkiaSharp;

// ReSharper disable AccessToDisposedClosure

namespace Masuit.Tools.Media;

/// <summary>
/// 图像边框移除器
/// </summary>
public class ImageBorderRemover
{
    private readonly ImageBorderRemoverOptions _options;

    /// <summary>
    /// 容差模式
    /// </summary>
    private ToleranceMode ToleranceMode { get; }

    private int CroppedBorderCount { get; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="mode">容差模式</param>
    /// <param name="croppedBorderCount">达到边框个数则裁剪</param>
    public ImageBorderRemover(ToleranceMode mode, int croppedBorderCount = 2, ImageBorderRemoverOptions? options = null)
    {
        if (croppedBorderCount is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(croppedBorderCount));
        ToleranceMode = mode;
        CroppedBorderCount = croppedBorderCount;
        _options = options ?? new ImageBorderRemoverOptions();
        ValidateOptions(_options);
    }

    /// <summary>
    /// 检测图片边框信息（支持多色边框）
    /// </summary>
    /// <param name="imagePath">图片路径</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <returns>边框检测结果</returns>
    public BorderDetectionResult DetectBorders(string imagePath, int tolerance)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            throw new ArgumentException("Image path cannot be null or empty.", nameof(imagePath));
        }

        using var image = SKBitmap.Decode(imagePath) ?? throw new InvalidDataException("无法解码图像。");
        return DetectBorders(image, tolerance);
    }

    /// <summary>
    /// 检测图片边框信息（从已加载的图像）
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <returns>边框检测结果</returns>
    public BorderDetectionResult DetectBorders(SKBitmap image, int tolerance)
    {
        if (image == null)
        {
            throw new ArgumentNullException(nameof(image), "Image cannot be null.");
        }

        if (image.Width < 1 || image.Height < 1)
        {
            throw new ArgumentException("Image must contain at least one pixel.", nameof(image));
        }

        if (tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance));
        }

        var result = new BorderDetectionResult(CroppedBorderCount)
        {
            ImageWidth = image.Width,
            ImageHeight = image.Height,
            ContentTop = 0,
            ContentBottom = image.Height - 1,
            ContentLeft = 0,
            ContentRight = image.Width - 1,
            BorderColors = new List<SKColor>()
        };

        result.ContentTop = FindBorder(image, tolerance, image.Height/2-1, BorderSide.Top, result.BorderColors);
        result.ContentBottom = image.Height - 1 - FindBorder(image, tolerance, image.Height / 2 - 1, BorderSide.Bottom, result.BorderColors);
        result.ContentLeft = FindBorder(image, tolerance, image.Width / 2 - 1, BorderSide.Left, result.BorderColors);
        result.ContentRight = image.Width - 1 - FindBorder(image, tolerance, image.Width / 2 - 1, BorderSide.Right, result.BorderColors);

        if (!IsValidContentArea(result))
        {
            return CreateUncroppedResult(image);
        }

        if (_options.EnableContourRefinement && result.BorderCount == 4)
        {
            RefineWithEdgeDensity(image, ref result);
        }

        result.BorderLayers = result.BorderColors.Count;
        return IsValidContentArea(result) ? result : CreateUncroppedResult(image);
    }

    /// <summary>
    /// 自动移除图片的多层边框
    /// </summary>
    /// <param name="inputPath">输入图片路径</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <returns>是否执行了裁剪操作</returns>
    public void RemoveBorders(string inputPath, int tolerance) => RemoveBorders(inputPath, inputPath, tolerance);

    /// <summary>
    /// 自动移除图片的多层边框
    /// </summary>
    /// <param name="inputPath">输入图片路径</param>
    /// <param name="outputPath">输出图片路径</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <returns>是否执行了裁剪操作</returns>
    public void RemoveBorders(string inputPath, string outputPath, int tolerance)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            throw new ArgumentException("Input path cannot be null or empty.", nameof(inputPath));
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));
        }

        using var image = SKBitmap.Decode(inputPath) ?? throw new InvalidDataException("无法解码图像。文件可能已损坏或格式不受支持。");
        using var cropped = RemoveBorders(image, tolerance);
        if (cropped!=null)
        {
            image.Dispose();
            var format = GetEncodedFormat(outputPath);
            using var output = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var outputImage = SKImage.FromBitmap(cropped);
            using var data = outputImage.Encode(format, 100) ?? throw new InvalidOperationException("无法编码输出图像。");
            data.SaveTo(output);
        }
    }

    /// <summary>
    /// 自动移除图片的多层边框
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <returns>是否执行了裁剪操作</returns>
    public SKBitmap? RemoveBorders(SKBitmap image, int tolerance)
    {
        var border = DetectBorders(image, tolerance);
        if (!border.CanBeCropped || border.ContentWidth <= 0 || border.ContentHeight <= 0 || (border.ContentWidth == image.Width && border.ContentHeight == image.Height)) return null;
        var result = new SKBitmap(border.ContentWidth, border.ContentHeight);
        if (!image.ExtractSubset(result, new SKRectI(border.ContentLeft, border.ContentTop, border.ContentRight + 1, border.ContentBottom + 1)))
        {
            result.Dispose();
            throw new InvalidOperationException("无法提取裁剪区域。");
        }

        return result;
    }

    private BorderDetectionResult CreateUncroppedResult(SKBitmap image) => new(CroppedBorderCount)
    {
        ImageWidth = image.Width,
        ImageHeight = image.Height,
        ContentTop = 0,
        ContentBottom = image.Height - 1,
        ContentLeft = 0,
        ContentRight = image.Width - 1,
        BorderColors = new List<SKColor>()
    };

    private int FindBorder(SKBitmap image, int tolerance, int maxScan, BorderSide side, List<SKColor> colors)
    {
        var lastBorderOffset = -1;
        var borderGaps = 0;
        var previousVariance = -1d;
        for (var offset = 0; offset < maxScan; offset++)
        {
            var line = GetLineSample(image, side, offset);
            if (!IsBorderLine(line, tolerance, out var variance) || previousVariance >= 0 && Math.Abs(variance - previousVariance) > _options.VarianceTransitionThreshold)
            {
                if (++borderGaps > _options.MaximumBorderGaps)
                {
                    break;
                }

                continue;
            }

            lastBorderOffset = offset;
            borderGaps = 0;
            previousVariance = variance;
            if (offset == 0 || offset == maxScan - 1)
            {
                colors.Add(line[line.Length / 2]);
            }
        }

        var borderWidth = lastBorderOffset + 1;
        return borderWidth >= _options.MinimumBorderThickness ? borderWidth : 0;
    }

    private SKColor[] GetLineSample(SKBitmap image, BorderSide side, int offset)
    {
        var horizontal = side is BorderSide.Top or BorderSide.Bottom;
        var length = horizontal ? image.Width : image.Height;
        var margin = (int)(length * _options.SampleMarginRatio);
        margin = Math.Max(0, Math.Min(margin, Math.Max(0, (length - 1) / 2)));
        var sampledLength = length - margin * 2;
        var count = Math.Min(sampledLength, _options.SampleCount);
        var samples = new SKColor[count];
        for (var index = 0; index < count; index++)
        {
            var coordinate = count == 1 ? margin : margin + index * (sampledLength - 1) / (count - 1);
            samples[index] = side switch
            {
                BorderSide.Top => image.GetPixel(coordinate, offset),
                BorderSide.Bottom => image.GetPixel(coordinate, image.Height - 1 - offset),
                BorderSide.Left => image.GetPixel(offset, coordinate),
                BorderSide.Right => image.GetPixel(image.Width - 1 - offset, coordinate),
                _ => throw new ArgumentOutOfRangeException(nameof(side))
            };
        }

        return samples;
    }

    private bool IsBorderLine(IReadOnlyList<SKColor> samples, int tolerance, out double variance)
    {
        var reference = samples[samples.Count / 2];
        var similar = 0;
        var adjacentSimilar = 0;
        var maximumGradientStep = 0d;
        var graySum = 0d;
        for (var index = 0; index < samples.Count; index++)
        {
            var sample = samples[index];
            graySum += ToGray(sample);
            if (IsSimilarColor(reference, sample, tolerance))
            {
                similar++;
            }

            if (index > 0 && IsSimilarColor(samples[index - 1], sample, tolerance))
            {
                adjacentSimilar++;
            }

            if (index > 0)
            {
                maximumGradientStep = Math.Max(maximumGradientStep, Math.Abs(ToGray(sample) - ToGray(samples[index - 1])));
            }
        }

        var mean = graySum / samples.Count;
        variance = 0;
        foreach (var sample in samples)
        {
            var difference = ToGray(sample) - mean;
            variance += difference * difference;
        }

        var smoothlyChanging = samples.Count > 1 && (double)adjacentSimilar / (samples.Count - 1) >= _options.MinimumSimilarRatio;
        variance /= samples.Count;
        var uniformlyColored = (double)similar / samples.Count >= _options.MinimumSimilarRatio && variance <= _options.MaximumGrayVariance;
        return uniformlyColored || smoothlyChanging && maximumGradientStep <= _options.MaximumGradientStep;
    }

    private bool IsValidContentArea(BorderDetectionResult result) => result.ContentLeft >= 0 && result.ContentTop >= 0 && result.ContentRight < result.ImageWidth && result.ContentBottom < result.ImageHeight && result.ContentWidth >= _options.MinimumContentSize && result.ContentHeight >= _options.MinimumContentSize;

    private void RefineWithEdgeDensity(SKBitmap image, ref BorderDetectionResult result)
    {
        var padding = Math.Max(0, _options.SafetyPadding);
        var left = result.HasLeftBorder ? result.ContentLeft : 0;
        var top = result.HasTopBorder ? result.ContentTop : 0;
        var right = result.HasRightBorder ? result.ContentRight : image.Width - 1;
        var bottom = result.HasBottomBorder ? result.ContentBottom : image.Height - 1;
        var edgeCount = 0;
        var topLimit = Math.Min(image.Height - 1, result.ContentTop + _options.MaximumContourRefinementPixels);
        var bottomLimit = Math.Max(0, result.ContentBottom - _options.MaximumContourRefinementPixels);
        var area = (long)result.ContentWidth * result.ContentHeight;
        var stride = Math.Max(_options.ContourSampleStride, (int)Math.Ceiling(Math.Sqrt(area / 1_000_000d)));
        for (var y = Math.Max(1, result.ContentTop); y < Math.Min(image.Height - 1, result.ContentBottom); y += stride)
        {
            for (var x = Math.Max(1, result.ContentLeft); x < Math.Min(image.Width - 1, result.ContentRight); x += stride)
            {
                var horizontal = Math.Abs(ToGray(image.GetPixel(x + 1, y)) - ToGray(image.GetPixel(x - 1, y)));
                var vertical = Math.Abs(ToGray(image.GetPixel(x, y + 1)) - ToGray(image.GetPixel(x, y - 1)));
                if (horizontal + vertical < _options.ContourEdgeThreshold)
                {
                    continue;
                }

                if (result.HasTopBorder && y <= topLimit)
                {
                    top = Math.Max(top, y);
                }

                if (result.HasBottomBorder && y >= bottomLimit)
                {
                    bottom = Math.Min(bottom, y);
                }
                edgeCount++;
            }
        }

        if (edgeCount < _options.MinimumContourPixels)
        {
            return;
        }

        left = Math.Max(0, left - padding);
        top = Math.Max(0, top - padding);
        right = Math.Min(image.Width - 1, right + padding);
        bottom = Math.Min(image.Height - 1, bottom + padding);
        var refined = new BorderDetectionResult(CroppedBorderCount)
        {
            ImageWidth = image.Width,
            ImageHeight = image.Height,
            ContentLeft = left,
            ContentTop = top,
            ContentRight = right,
            ContentBottom = bottom
        };
        if (CountBordersAtMinimumThickness(refined) >= CroppedBorderCount && IsValidContentArea(refined))
        {
            result.ContentLeft = left;
            result.ContentTop = top;
            result.ContentRight = right;
            result.ContentBottom = bottom;
        }
    }

    private int CountBordersAtMinimumThickness(BorderDetectionResult result)
    {
        var thickness = _options.MinimumBorderThickness;
        return (result.TopBorderWidth >= thickness ? 1 : 0) +
               (result.BottomBorderWidth >= thickness ? 1 : 0) +
               (result.LeftBorderWidth >= thickness ? 1 : 0) +
               (result.RightBorderWidth >= thickness ? 1 : 0);
    }

    private static double ToGray(SKColor color) => color.Red * 0.299 + color.Green * 0.587 + color.Blue * 0.114;

    private static SKEncodedImageFormat GetEncodedFormat(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
        ".webp" => SKEncodedImageFormat.Webp,
        ".bmp" => SKEncodedImageFormat.Bmp,
        ".gif" => SKEncodedImageFormat.Gif,
        ".ico" => SKEncodedImageFormat.Ico,
        _ => SKEncodedImageFormat.Png
    };

    private static void ValidateOptions(ImageBorderRemoverOptions options)
    {
        if (options.SampleCount < 1 || options.SampleMarginRatio is < 0 or >= 0.5 || options.MinimumSimilarRatio is <= 0 or > 1 || options.MaximumBorderGaps < 0 || options.MaximumGrayVariance < 0 || options.MaximumGradientStep < 0 || options.VarianceTransitionThreshold < 0 ||  options.MinimumBorderThickness < 1 || options.MinimumContentSize < 1 || options.SafetyPadding < 0 || options.ContourSampleStride < 1 || options.ContourEdgeThreshold < 0 || options.MinimumContourPixels < 1 || options.MaximumContourRefinementPixels < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options));
        }
    }

    private enum BorderSide
    {
        Top,
        Bottom,
        Left,
        Right
    }

    /// <summary>
    /// 判断两个颜色是否相似
    /// </summary>
    /// <param name="first">第一个颜色</param>
    /// <param name="second">第二个颜色</param>
    /// <param name="tolerance">颜色容差</param>
    /// <returns>是否相似</returns>
    private bool IsSimilarColor(SKColor first, SKColor second, int tolerance) => ToleranceMode switch
    {
        ToleranceMode.EuclideanDistance => CompareWithEuclideanDistance(first, second, tolerance),
        ToleranceMode.Channel => CompareColors(first, second, tolerance),
        ToleranceMode.DeltaE1976 => Color.FromArgb(first.Alpha, first.Red, first.Green, first.Blue).CIE1976(Color.FromArgb(second.Alpha, second.Red, second.Green, second.Blue)) <= tolerance,
        ToleranceMode.DeltaE1994 => Color.FromArgb(first.Alpha, first.Red, first.Green, first.Blue).CIE1994(Color.FromArgb(second.Alpha, second.Red, second.Green, second.Blue)) <= tolerance,
        ToleranceMode.DeltaE2000 => Color.FromArgb(first.Alpha, first.Red, first.Green, first.Blue).CIE2000(Color.FromArgb(second.Alpha, second.Red, second.Green, second.Blue)) <= tolerance,
        ToleranceMode.DeltaECMC => Color.FromArgb(first.Alpha, first.Red, first.Green, first.Blue).CMC(Color.FromArgb(second.Alpha, second.Red, second.Green, second.Blue)) <= tolerance,
        _ => throw new ArgumentOutOfRangeException()
    };

    /// <summary>
    /// 比较两个颜色是否在指定容差范围内相似  
    /// </summary>
    /// <param name="first">第一个颜色</param>
    /// <param name="second">第二个颜色</param>
    /// <param name="tolerance">颜色容差</param>
    /// <returns>是否相似</returns>
    private static bool CompareColors(SKColor first, SKColor second, int tolerance) => Math.Abs(first.Alpha - second.Alpha) <= tolerance && Math.Abs(first.Red - second.Red) <= tolerance && Math.Abs(first.Green - second.Green) <= tolerance && Math.Abs(first.Blue - second.Blue) <= tolerance;

    /// <summary>
    /// 比较两个颜色的欧几里得距离是否在指定容差范围内相似
    /// </summary>
    /// <param name="first">第一个颜色</param>
    /// <param name="second">第二个颜色</param>
    /// <param name="tolerance">颜色容差</param>
    /// <returns>是否相似</returns>
    private static bool CompareWithEuclideanDistance(SKColor first, SKColor second, double tolerance)
    {
        var alpha = first.Alpha - second.Alpha;
        var red = first.Red - second.Red;
        var green = first.Green - second.Green;
        var blue = first.Blue - second.Blue;
        return Math.Sqrt(alpha * alpha + red * red + green * green + blue * blue) <= tolerance;
    }
}

public static class ImageBorderRemoverExt
{
    /// <summary>
    /// 检测图片边框信息（从已加载的图像）
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="toleranceMode">容差模式</param>
    /// <returns>边框检测结果</returns>
    public static BorderDetectionResult DetectBorders(this SKBitmap image, int tolerance, ToleranceMode toleranceMode)
    {
        var remover = new ImageBorderRemover(toleranceMode);
        return remover.DetectBorders(image, tolerance);
    }

    /// <summary>
    /// 自动移除图片的多层边框（仅当至少有两边存在边框时才裁剪）
    /// </summary>
    /// <param name="image"></param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="toleranceMode">容差模式</param>
    /// <returns>是否执行了裁剪操作</returns>
    public static SKBitmap RemoveBorders(this SKBitmap image, int tolerance, ToleranceMode toleranceMode, int cropBorderCount = 2)
    {
        var remover = new ImageBorderRemover(toleranceMode, cropBorderCount);
        return remover.RemoveBorders(image, tolerance);
    }
}