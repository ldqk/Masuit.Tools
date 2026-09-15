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
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <param name="useDownscaling">是否使用缩小采样优化性能，默认false，开启可能会导致图片过多裁剪</param>
    /// <param name="downscaleFactor">缩小采样比例(1-10)，默认4</param>
    /// <returns>边框检测结果</returns>
    public BorderDetectionResult DetectBorders(string imagePath, int tolerance, int maxLayers = 5, bool useDownscaling = false, int downscaleFactor = 4)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            throw new ArgumentException("Image path cannot be null or empty.", nameof(imagePath));
        }

        using var image = SKBitmap.Decode(imagePath) ?? throw new InvalidDataException("无法解码图像。");
        return DetectBorders(image, tolerance, maxLayers, useDownscaling, downscaleFactor);
    }

    /// <summary>
    /// 检测图片边框信息（从已加载的图像）
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <param name="useDownscaling">是否使用缩小采样优化性能，默认false，开启可能会导致图片过多裁剪</param>
    /// <param name="downscaleFactor">缩小采样比例(1-10)，默认4</param>
    /// <returns>边框检测结果</returns>
    public BorderDetectionResult DetectBorders(SKBitmap image, int tolerance, int maxLayers = 5, bool useDownscaling = false, int downscaleFactor = 4)
    {
        if (image == null)
        {
            throw new ArgumentNullException(nameof(image), "Image cannot be null.");
        }

        ValidateOptions(tolerance, maxLayers, downscaleFactor);
        using var sample = useDownscaling ? CreateSample(image, downscaleFactor) : image.Copy();
        var detected = IsLightStudioBackground(sample, tolerance) ? FindBackgroundBounds(sample, tolerance) : FindContentBordersWithLayers(sample, tolerance, maxLayers);
        var scale = useDownscaling ? downscaleFactor : 1;
        return new BorderDetectionResult(CroppedBorderCount)
        {
            ImageWidth = image.Width,
            ImageHeight = image.Height,
            ContentTop = ScaleStart(detected.top, scale, image.Height),
            ContentBottom = ScaleEnd(detected.bottom, scale, image.Height),
            ContentLeft = ScaleStart(detected.left, scale, image.Width),
            ContentRight = ScaleEnd(detected.right, scale, image.Width),
            BorderLayers = detected.layers,
            BorderColors = detected.colors
        };
    }

    /// <summary>
    /// 自动移除图片的多层边框
    /// </summary>
    /// <param name="inputPath">输入图片路径</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <param name="useDownscaling">是否使用缩小采样优化性能，默认false，开启可能会导致图片过多裁剪</param>
    /// <param name="downscaleFactor">缩小采样比例(1-10)，默认4</param>
    /// <returns>是否执行了裁剪操作</returns>
    public void RemoveBorders(string inputPath, int tolerance, int maxLayers = 5, bool useDownscaling = false, int downscaleFactor = 4) => RemoveBorders(inputPath, inputPath, tolerance, maxLayers, useDownscaling, downscaleFactor);

    /// <summary>
    /// 自动移除图片的多层边框
    /// </summary>
    /// <param name="inputPath">输入图片路径</param>
    /// <param name="outputPath">输出图片路径</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <param name="useDownscaling">是否使用缩小采样优化性能，默认false，开启可能会导致图片过多裁剪</param>
    /// <param name="downscaleFactor">缩小采样比例(1-10)，默认4</param>
    /// <returns>是否执行了裁剪操作</returns>
    public void RemoveBorders(string inputPath, string outputPath, int tolerance, int maxLayers = 5, bool useDownscaling = false, int downscaleFactor = 4)
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
        using var cropped = RemoveBorders(image, tolerance, maxLayers, useDownscaling, downscaleFactor);
        using var data = (cropped ?? image).Encode(SKEncodedImageFormat.Jpeg, 90) ?? throw new InvalidOperationException("无法编码图像。");
        using var stream = File.Create(outputPath);
        data.SaveTo(stream);
    }

    /// <summary>
    /// 自动移除图片的多层边框
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <param name="useDownscaling">是否使用缩小采样优化性能，默认false，开启可能会导致图片过多裁剪</param>
    /// <param name="downscaleFactor">缩小采样比例(1-10)，默认4</param>
    /// <returns>是否执行了裁剪操作</returns>
    public SKBitmap? RemoveBorders(SKBitmap image, int tolerance, int maxLayers = 5, bool useDownscaling = false, int downscaleFactor = 4)
    {
        var border = DetectBorders(image, tolerance, maxLayers, useDownscaling, downscaleFactor);
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
    /// 查找图像内容边界（支持多层边框）
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <returns>边界信息</returns>
    private (int top, int bottom, int left, int right, int layers, List<SKColor> colors) FindContentBordersWithLayers(SKBitmap image, int tolerance, int maxLayers)
    {
        var top = 0;
        var bottom = image.Height - 1;
        var left = 0;
        var right = image.Width - 1;
        var colors = new List<SKColor>();
        var layers = 0;
        while (layers < maxLayers && top < bottom && left < right)
        {
            var layerColors = new List<SKColor>();
            var changed = false;
            var nextTop = DetectLayerBorderTop(image, top, bottom, left, right, tolerance, layerColors);
            var nextBottom = DetectLayerBorderBottom(image, top, bottom, left, right, tolerance, layerColors);
            var nextLeft = DetectLayerBorderLeft(image, top, bottom, left, right, tolerance, layerColors);
            var nextRight = DetectLayerBorderRight(image, top, bottom, left, right, tolerance, layerColors);
            changed |= nextTop > top || nextBottom < bottom || nextLeft > left || nextRight < right;
            top = nextTop;
            bottom = nextBottom;
            left = nextLeft;
            right = nextRight;
            if (!changed) break;
            layers++;
            colors.AddRange(layerColors.Distinct());
        }

        return (top, bottom, left, right, layers, colors);
    }

    private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

    /// <summary>
    /// 判断图像是否为浅色工作背景
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <returns>是否为浅色工作背景</returns>
    private static bool IsLightStudioBackground(SKBitmap image, int tolerance)
    {
        var corners = new[]
        {
            image.GetPixel(0, 0),
            image.GetPixel(image.Width - 1, 0),
            image.GetPixel(0, image.Height - 1),
            image.GetPixel(image.Width - 1, image.Height - 1)
        };
        var cornerTolerance = Clamp(tolerance * 2, 1, 32);
        if (!corners.All(color => color.Red >= 180 && color.Green >= 180 && color.Blue >= 180 && color.Alpha >= 220) || !corners.All(color => CompareColors(color, corners[0], Math.Max(cornerTolerance, 80)))) return false;

        var bandHeight = Math.Max(1, image.Height / 10);
        var bandWidth = Math.Max(1, image.Width / 10);
        return IsBackgroundBand(image, 0, bandHeight, true, corners[0], tolerance) || IsBackgroundBand(image, image.Height - bandHeight, image.Height, true, corners[0], tolerance) || IsBackgroundBand(image, 0, bandWidth, false, corners[0], tolerance) || IsBackgroundBand(image, image.Width - bandWidth, image.Width, false, corners[0], tolerance);
    }

    /// <summary>
    /// 判断图像是否为背景带
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="start">起始位置</param>
    /// <param name="end">结束位置</param>
    /// <param name="row">是否为行</param>
    /// <param name="background">背景颜色</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <returns>是否为背景带</returns>
    private static bool IsBackgroundBand(SKBitmap image, int start, int end, bool row, SKColor background, int tolerance)
    {
        var matching = 0;
        var total = 0;
        var step = Math.Max(1, (row ? image.Width : image.Height) / 64);
        for (var edge = start; edge < end; edge += Math.Max(1, (end - start) / 16))
        for (var coordinate = 0; coordinate < (row ? image.Width : image.Height); coordinate += step)
        {
            var color = row ? image.GetPixel(coordinate, edge) : image.GetPixel(edge, coordinate);
            if (CompareColors(color, background, Math.Max(12, tolerance * 2))) matching++;
            total++;
        }

        return matching >= total * 0.8;
    }

    /// <summary>
    /// 查找图像背景边界
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <returns>边界信息</returns>
    private (int top, int bottom, int left, int right, int layers, List<SKColor> colors) FindBackgroundBounds(SKBitmap image, int tolerance)
    {
        var background = image.GetPixel(0, 0);
        const int safetyMargin = 1;
        var top = Math.Max(0, FindBackgroundEdge(image, 0, image.Height - 1, 1, true, background, tolerance) - safetyMargin);
        var bottom = Math.Min(image.Height - 1, FindBackgroundEdge(image, image.Height - 1, top, -1, true, background, tolerance) + safetyMargin);
        var left = Math.Max(0, FindBackgroundEdge(image, 0, image.Width - 1, 1, false, background, tolerance) - safetyMargin);
        var right = Math.Min(image.Width - 1, FindBackgroundEdge(image, image.Width - 1, left, -1, false, background, tolerance) + safetyMargin);
        return (top, bottom, left, right, 1, []);
    }

    private static int FindBackgroundEdge(SKBitmap image, int start, int limit, int direction, bool row, SKColor background, int tolerance)
    {
        const int sampleCount = 129;
        var edge = start;
        while (edge != limit)
        {
            var foreground = 0;
            for (var i = 0; i < sampleCount; i++)
            {
                var coordinate = i * (row ? image.Width - 1 : image.Height - 1) / (sampleCount - 1);
                var color = row ? image.GetPixel(coordinate, edge) : image.GetPixel(edge, coordinate);
                if (!CompareColors(color, background, Math.Max(48, tolerance * 3))) foreground++;
            }

            if (foreground >= 2) return edge;
            edge += direction;
        }

        return direction > 0 ? edge : edge + 1;
    }

    /// <summary>
    /// 检测图像顶部图层边界
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="top">顶部位置</param>
    /// <param name="bottom">底部位置</param>
    /// <param name="left">左侧位置</param>
    /// <param name="right">右侧位置</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="colors">颜色列表</param>
    /// <returns>顶部图层边界位置</returns>
    private int DetectLayerBorderTop(SKBitmap image, int top, int bottom, int left, int right, int tolerance, List<SKColor> colors) => ScanRows(image, top, bottom, left, right, tolerance, colors, 1);

    /// <summary>
    /// 检测图像底部图层边界
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="top">顶部位置</param>
    /// <param name="bottom">底部位置</param>
    /// <param name="left">左侧位置</param>
    /// <param name="right">右侧位置</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="colors">颜色列表</param>
    /// <returns>底部图层边界位置</returns>
    private int DetectLayerBorderBottom(SKBitmap image, int top, int bottom, int left, int right, int tolerance, List<SKColor> colors) => ScanRows(image, bottom, top, left, right, tolerance, colors, -1);

    /// <summary>
    /// 检测图像左侧图层边界
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="top">顶部位置</param>
    /// <param name="bottom">底部位置</param>
    /// <param name="left">左侧位置</param>
    /// <param name="right">右侧位置</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="colors">颜色列表</param>
    /// <returns>左侧图层边界位置</returns>
    private int DetectLayerBorderLeft(SKBitmap image, int top, int bottom, int left, int right, int tolerance, List<SKColor> colors) => ScanColumns(image, left, right, top, bottom, tolerance, colors, 1);

    /// <summary>
    /// 检测图像右侧图层边界
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="top">顶部位置</param>
    /// <param name="bottom">底部位置</param>
    /// <param name="left">左侧位置</param>
    /// <param name="right">右侧位置</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="colors">颜色列表</param>
    /// <returns>右侧图层边界位置</returns>
    private int DetectLayerBorderRight(SKBitmap image, int top, int bottom, int left, int right, int tolerance, List<SKColor> colors) => ScanColumns(image, right, left, top, bottom, tolerance, colors, -1);

    /// <summary>
    /// 扫描图像行以检测边界
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="start">起始行位置</param>
    /// <param name="limit">限制行位置</param>
    /// <param name="left">左侧位置</param>
    /// <param name="right">右侧位置</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="colors">颜色列表</param>
    /// <param name="direction">扫描方向(1表示从上到下，-1表示从下到上)</param>
    /// <param name="allowedMissedRows">允许的连续未检测到边界的行数</param>
    /// <returns>检测到的边界行位置</returns>
    private int ScanRows(SKBitmap image, int start, int limit, int left, int right, int tolerance, List<SKColor> colors, int direction, int allowedMissedRows = 4)
    {
        var edge = start;
        var missedRows = 0;
        while (edge != limit)
        {
            if (IsBorderRow(image, edge, left, right, tolerance, out var color))
            {
                colors.Add(color);
                missedRows = 0;
            }
            else if (++missedRows >= allowedMissedRows)
            {
                return direction > 0 ? edge - allowedMissedRows + 1 : edge;
            }

            edge += direction;
        }

        return direction > 0 ? edge : edge + 1;
    }

    /// <summary>
    /// 扫描图像列以检测边界
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="start">起始列位置</param>
    /// <param name="limit">限制列位置</param>
    /// <param name="top">顶部位置</param>
    /// <param name="bottom">底部位置</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="colors">颜色列表</param>
    /// <param name="direction">扫描方向(1表示从左到右，-1表示从右到左)</param>
    /// <param name="allowedMissedColumns">允许的连续未检测到边界的列数</param>
    /// <returns>检测到的边界列位置</returns>
    private int ScanColumns(SKBitmap image, int start, int limit, int top, int bottom, int tolerance, List<SKColor> colors, int direction, int allowedMissedColumns = 4)
    {
        var edge = start;
        var missedColumns = 0;
        while (edge != limit)
        {
            if (IsBorderColumn(image, edge, top, bottom, tolerance, out var color))
            {
                colors.Add(color);
                missedColumns = 0;
            }
            else if (++missedColumns >= allowedMissedColumns)
            {
                return direction > 0 ? edge - allowedMissedColumns + 1 : edge;
            }

            edge += direction;
        }

        return direction > 0 ? edge : edge + 1;
    }

    /// <summary>
    /// 判断指定行是否为边界行
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="y">行位置</param>
    /// <param name="left">左侧位置</param>
    /// <param name="right">右侧位置</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="color">检测到的边界颜色</param>
    /// <returns>是否为边界行</returns>
    private bool IsBorderRow(SKBitmap image, int y, int left, int right, int tolerance, out SKColor color)
    {
        const int sampleCount = 9;
        var samples = Enumerable.Range(0, sampleCount).Select(i => image.GetPixel(left + (right - left) * i / (sampleCount - 1), y)).ToArray();
        var referenceColor = samples[sampleCount / 2];
        color = referenceColor;
        return samples.Count(sample => IsSimilarColor(sample, referenceColor, tolerance)) >= sampleCount - 1;
    }

    /// <summary>
    /// 判断指定列是否为边界列
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="x">列位置</param>
    /// <param name="top">顶部位置</param>
    /// <param name="bottom">底部位置</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="color">检测到的边界颜色</param>
    /// <returns>是否为边界列</returns>
    private bool IsBorderColumn(SKBitmap image, int x, int top, int bottom, int tolerance, out SKColor color)
    {
        const int sampleCount = 9;
        var samples = Enumerable.Range(0, sampleCount).Select(i => image.GetPixel(x, top + (bottom - top) * i / (sampleCount - 1))).ToArray();
        var referenceColor = samples[sampleCount / 2];
        color = referenceColor;
        return samples.Count(sample => IsSimilarColor(sample, referenceColor, tolerance)) >= sampleCount - 1;
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
        _ => CompareColors(first, second, tolerance)
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

    /// <summary>
    /// 创建图像的缩小采样版本
    /// </summary>
    /// <param name="source">源图像</param>
    /// <param name="factor">缩小比例</param>
    /// <returns>缩小采样后的图像</returns>
    private static SKBitmap CreateSample(SKBitmap source, int factor)
    {
        var sample = new SKBitmap(new SKImageInfo(Math.Max(1, source.Width / factor), Math.Max(1, source.Height / factor), source.ColorType, source.AlphaType));
        source.ScalePixels(sample, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
        return sample;
    }

    /// <summary>
    /// 将边界值按缩放比例进行调整
    /// </summary>
    /// <param name="value">边界值</param>
    /// <param name="factor">缩放比例</param>
    /// <param name="size">图像尺寸</param>
    /// <returns>调整后的边界值</returns>
    private static int ScaleStart(int value, int factor, int size) => Clamp(value * factor, 0, size - 1);

    /// <summary>
    /// 将边界值按缩放比例进行调整（用于结束边界）
    /// </summary>
    /// <param name="value">边界值</param>
    /// <param name="factor">缩放比例</param>
    /// <param name="size">图像尺寸</param>
    /// <returns>调整后的边界值</returns>
    private static int ScaleEnd(int value, int factor, int size) => Clamp((value + 1) * factor - 1, 0, size - 1);

    /// <summary>
    /// 验证输入参数的有效性
    /// </summary>
    /// <param name="tolerance">颜色容差</param>
    /// <param name="maxLayers">最大检测边框层数</param>
    /// <param name="downscaleFactor">缩小采样比例</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static void ValidateOptions(int tolerance, int maxLayers, int downscaleFactor)
    {
        if (tolerance is < 0 or > 255) throw new ArgumentOutOfRangeException(nameof(tolerance));
        if (maxLayers < 1) throw new ArgumentOutOfRangeException(nameof(maxLayers));
        if (downscaleFactor is < 1 or > 10) throw new ArgumentOutOfRangeException(nameof(downscaleFactor));
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
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <param name="useDownscaling">是否使用缩小采样优化性能，默认false，开启可能会导致图片过多裁剪</param>
    /// <param name="downscaleFactor">缩小采样比例(1-10)，默认4</param>
    /// <returns>边框检测结果</returns>
    public static BorderDetectionResult DetectBorders(this SKBitmap image, int tolerance, ToleranceMode toleranceMode, int maxLayers = 3, bool useDownscaling = false, int downscaleFactor = 4)
    {
        var remover = new ImageBorderRemover(toleranceMode);
        return remover.DetectBorders(image, tolerance, maxLayers, useDownscaling, downscaleFactor);
    }

    /// <summary>
    /// 自动移除图片的多层边框（仅当至少有两边存在边框时才裁剪）
    /// </summary>
    /// <param name="image"></param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1-10，欧几里德模式建议(0-442之间)</param>
    /// <param name="toleranceMode">容差模式</param>
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <param name="cropBorderCount">最少边框数</param>
    /// <param name="useDownscaling">是否使用缩小采样优化性能，默认false，开启可能会导致图片过多裁剪</param>
    /// <param name="downscaleFactor">缩小采样比例(1-10)，默认4</param>
    /// <returns>是否执行了裁剪操作</returns>
    public static SKBitmap RemoveBorders(this SKBitmap image, int tolerance, ToleranceMode toleranceMode, int maxLayers = 3, int cropBorderCount = 2, bool useDownscaling = false, int downscaleFactor = 4)
    {
        var remover = new ImageBorderRemover(toleranceMode, cropBorderCount);
        return remover.RemoveBorders(image, tolerance, maxLayers, useDownscaling, downscaleFactor);
    }
}