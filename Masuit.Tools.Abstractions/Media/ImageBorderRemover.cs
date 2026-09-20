using System.Drawing;
using SkiaSharp;

// ReSharper disable AccessToDisposedClosure

namespace Masuit.Tools.Media;

/// <summary>
/// 图像边框移除器
/// </summary>
public class ImageBorderRemover
{
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
    public ImageBorderRemover(ToleranceMode mode, int croppedBorderCount = 2)
    {
        if (croppedBorderCount is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(croppedBorderCount));
        ToleranceMode = mode;
        CroppedBorderCount = croppedBorderCount;
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

        using var image = SKBitmap.Decode(inputPath) ?? throw new InvalidDataException("无法解码图像。");
        // todo
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