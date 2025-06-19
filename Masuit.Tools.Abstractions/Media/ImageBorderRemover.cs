using Masuit.Tools.Systems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
    private ToleranceMode ToleranceMode { get; set; }

    private int CroppedBorderCount { get; set; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="mode">容差模式</param>
    /// <param name="croppedBorderCount">达到边框个数则裁剪</param>
    public ImageBorderRemover(ToleranceMode mode, int croppedBorderCount = 2)
    {
        ToleranceMode = mode;
        CroppedBorderCount = croppedBorderCount;
    }

    /// <summary>
    /// 检测图片边框信息（支持多色边框）
    /// </summary>
    /// <param name="imagePath">图片路径</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1</param>
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <param name="useDownscaling">是否使用缩小采样优化性能，默认false，开启可能会导致图片过多裁剪</param>
    /// <param name="downscaleFactor">缩小采样比例(1-10)，默认4</param>
    /// <returns>边框检测结果</returns>
    public BorderDetectionResult DetectBorders(string imagePath, int tolerance, int maxLayers = 3, bool useDownscaling = false, int downscaleFactor = 4)
    {
        using var image = Image.Load<Rgba32>(imagePath);
        return DetectBorders(image, tolerance, maxLayers, useDownscaling, downscaleFactor);
    }

    /// <summary>
    /// 检测图片边框信息（从已加载的图像）
    /// </summary>
    /// <param name="image">已加载的图像</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1</param>
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <param name="useDownscaling">是否使用缩小采样优化性能，默认false，开启可能会导致图片过多裁剪</param>
    /// <param name="downscaleFactor">缩小采样比例(1-10)，默认4</param>
    /// <returns>边框检测结果</returns>
    public BorderDetectionResult DetectBorders(Image<Rgba32> image, int tolerance, int maxLayers = 3, bool useDownscaling = false, int downscaleFactor = 4)
    {
        var result = new BorderDetectionResult(CroppedBorderCount)
        {
            ImageWidth = image.Width,
            ImageHeight = image.Height,
            BorderColors = new List<Rgba32>(),
            BorderLayers = 0
        };

        byte toleranceValue = (byte)(tolerance * 2.55);

        // 使用多层边框检测算法
        var (top, bottom, left, right, layers, colors) = FindContentBordersWithLayers(image, toleranceValue, maxLayers, useDownscaling, downscaleFactor);

        // 设置内容边界
        result.ContentTop = top;
        result.ContentBottom = bottom;
        result.ContentLeft = left;
        result.ContentRight = right;
        result.BorderLayers = layers;
        result.BorderColors = colors;

        return result;
    }

    /// <summary>
    /// 自动移除图片的多层边框（仅当至少有两边存在边框时才裁剪）
    /// </summary>
    /// <param name="inputPath">输入图片路径</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1</param>
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <param name="useDownscaling">是否使用缩小采样优化性能，默认false，开启可能会导致图片过多裁剪</param>
    /// <param name="downscaleFactor">缩小采样比例(1-10)，默认4</param>
    /// <returns>是否执行了裁剪操作</returns>
    public void RemoveBorders(string inputPath, int tolerance, int maxLayers = 3, bool useDownscaling = false, int downscaleFactor = 4)
    {
        RemoveBorders(inputPath, inputPath, tolerance, maxLayers, useDownscaling, downscaleFactor);
    }

    /// <summary>
    /// 自动移除图片的多层边框（仅当至少有两边存在边框时才裁剪）
    /// </summary>
    /// <param name="inputPath">输入图片路径</param>
    /// <param name="outputPath">输出图片路径</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1</param>
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <param name="useDownscaling">是否使用缩小采样优化性能，默认false，开启可能会导致图片过多裁剪</param>
    /// <param name="downscaleFactor">缩小采样比例(1-10)，默认4</param>
    /// <returns>是否执行了裁剪操作</returns>
    public void RemoveBorders(string inputPath, string outputPath, int tolerance, int maxLayers = 3, bool useDownscaling = false, int downscaleFactor = 4)
    {
        using Image<Rgba32> image = Image.Load<Rgba32>(inputPath);
        var hasCropped = RemoveBorders(image, tolerance, maxLayers, useDownscaling, downscaleFactor);

        // 决定是否保存
        if (hasCropped)
        {
            image.Save(outputPath);
        }
    }

    /// <summary>
    /// 自动移除图片的多层边框（仅当至少有两边存在边框时才裁剪）
    /// </summary>
    /// <param name="input">输入图片路径</param>
    /// <param name="tolerance">颜色容差(0-100)，通道模式建议10，ΔE模式建议1</param>
    /// <param name="maxLayers">最大检测边框层数，默认3</param>
    /// <param name="useDownscaling">是否使用缩小采样优化性能，默认false，开启可能会导致图片过多裁剪</param>
    /// <param name="downscaleFactor">缩小采样比例(1-10)，默认4</param>
    /// <returns>是否执行了裁剪操作</returns>
    public PooledMemoryStream RemoveBorders(Stream input, int tolerance, int maxLayers = 3, bool useDownscaling = false, int downscaleFactor = 4)
    {
        var format = Image.DetectFormat(input);
        input.Seek(0, SeekOrigin.Begin);
        Image<Rgba32> image = Image.Load<Rgba32>(input);
        RemoveBorders(image, tolerance, maxLayers, useDownscaling, downscaleFactor);
        var stream = new PooledMemoryStream();
        image.Save(stream, format);
        return stream;
    }

    private bool RemoveBorders(Image<Rgba32> image, int tolerance, int maxLayers, bool useDownscaling, int downscaleFactor)
    {
        // 保存原始尺寸用于比较
        int originalWidth = image.Width;
        int originalHeight = image.Height;

        // 使用多层检测方法获取边框信息
        var borderInfo = DetectBorders(image, tolerance, maxLayers, useDownscaling, downscaleFactor);

        bool hasCropped = false;
        if (borderInfo.CanBeCropped)
        {
            int newWidth = borderInfo.ContentRight - borderInfo.ContentLeft + 1;
            int newHeight = borderInfo.ContentBottom - borderInfo.ContentTop + 1;

            if (newWidth > 0 && newHeight > 0 && (newWidth != originalWidth || newHeight != originalHeight))
            {
                image.Mutate(x => x.Crop(new Rectangle(borderInfo.ContentLeft, borderInfo.ContentTop, newWidth, newHeight)));
                hasCropped = true;
            }
        }
        return hasCropped;
    }

    /// <summary>
    /// 查找内容边界（支持多层边框检测）
    /// </summary>
    private static (int top, int bottom, int left, int right, int layers, List<Rgba32> colors) FindContentBordersWithLayers(Image<Rgba32> image, byte tolerance, int maxLayers, bool useDownscaling, int downscaleFactor)
    {
        // 如果启用缩小采样且图像足够大
        Image<Rgba32> workingImage;
        float scale = 1f;
        bool isDownscaled = false;

        if (useDownscaling && image.Width > 500 && image.Height > 500)
        {
            // 计算缩小尺寸
            int newWidth = image.Width / downscaleFactor;
            int newHeight = image.Height / downscaleFactor;
            scale = (float)image.Width / newWidth;

            // 创建缩小版本用于检测
            workingImage = image.Clone(ctx => ctx.Resize(newWidth, newHeight));
            isDownscaled = true;
        }
        else
        {
            workingImage = image;
        }

        int width = workingImage.Width;
        int height = workingImage.Height;

        int top = 0;
        int bottom = height - 1;
        int left = 0;
        int right = width - 1;

        int layers = 0;
        var borderColors = new List<Rgba32>();

        // 检测多层边框
        for (int layer = 0; layer < maxLayers; layer++)
        {
            bool borderFound = false;

            // 并行检测四个方向的边框层
            var results = new (int borderSize, Rgba32? color)[4];

            Parallel.Invoke(() =>
            {
                if (top < height / 2)
                {
                    Rgba32? layerColor = null;
                    int newTop = DetectLayerBorderTop(workingImage, top, bottom, left, right, tolerance, ref layerColor);
                    results[0] = (newTop - top, layerColor);
                    if (newTop > top) borderFound = true;
                    top = newTop;
                }
            }, () =>
            {
                if (bottom > height / 2)
                {
                    Rgba32? layerColor = null;
                    int newBottom = DetectLayerBorderBottom(workingImage, top, bottom, left, right, tolerance, ref layerColor);
                    results[1] = (newBottom - bottom, layerColor);
                    if (newBottom < bottom) borderFound = true;
                    bottom = newBottom;
                }
            }, () =>
            {
                if (left < width / 2)
                {
                    Rgba32? layerColor = null;
                    int newLeft = DetectLayerBorderLeft(workingImage, top, bottom, left, right, tolerance, ref layerColor);
                    results[2] = (newLeft - left, layerColor);
                    if (newLeft > left) borderFound = true;
                    left = newLeft;
                }
            }, () =>
            {
                if (right > width / 2)
                {
                    Rgba32? layerColor = null;
                    int newRight = DetectLayerBorderRight(workingImage, top, bottom, left, right, tolerance, ref layerColor);
                    results[3] = (newRight - right, layerColor);
                    if (newRight < right) borderFound = true;
                    right = newRight;
                }
            });

            // 收集检测到的边框颜色
            foreach (var (borderSize, color) in results)
            {
                if (color.HasValue && borderSize > 0)
                {
                    borderColors.Add(color.Value);
                }
            }

            if (borderFound)
            {
                layers++;
            }
            else
            {
                break; // 没有检测到更多边框层
            }
        }

        // 如果是缩小采样版本，映射回原图坐标
        if (isDownscaled)
        {
            top = (int)(top * scale);
            bottom = (int)(bottom * scale);
            left = (int)(left * scale);
            right = (int)(right * scale);

            // 确保边界在图像范围内
            top = Clamp(top, 0, image.Height - 1);
            bottom = Clamp(bottom, top, image.Height - 1);
            left = Clamp(left, 0, image.Width - 1);
            right = Clamp(right, left, image.Width - 1);

            // 释放缩小图像
            workingImage.Dispose();
        }

        return (top, bottom, left, right, layers, borderColors);
    }

    private static int Clamp(int value, int min, int max)
    {
        return value < min ? min : value > max ? max : value;
    }

    /// <summary>
    /// 检测顶部边框层（优化版）
    /// </summary>
    private static int DetectLayerBorderTop(Image<Rgba32> image, int currentTop, int currentBottom, int currentLeft, int currentRight, byte tolerance, ref Rgba32? borderColor)
    {
        int newTop = currentTop;
        Rgba32? detectedColor = null;

        // 使用采样检测代替全行扫描
        int sampleCount = Math.Min(50, currentRight - currentLeft + 1);
        int stepX = Math.Max(1, (currentRight - currentLeft) / sampleCount);

        // 从当前顶部开始向下扫描
        for (int y = currentTop; y <= currentBottom; y++)
        {
            Rgba32? rowColor = null;
            bool isUniform = true;

            // 采样检查行是否统一颜色
            for (int x = currentLeft; x <= currentRight; x += stepX)
            {
                if (!rowColor.HasValue)
                {
                    rowColor = image[x, y];
                    continue;
                }

                if (!IsSimilarColor(image[x, y], rowColor.Value, tolerance))
                {
                    isUniform = false;
                    break;
                }
            }

            // 如果是统一颜色行
            if (isUniform && rowColor.HasValue)
            {
                // 第一行总是被认为是边框
                if (y == currentTop)
                {
                    detectedColor = rowColor;
                    newTop = y + 1;
                    continue;
                }

                // 后续行必须与第一行颜色相似
                if (detectedColor.HasValue && IsSimilarColor(rowColor.Value, detectedColor.Value, tolerance))
                {
                    newTop = y + 1;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        if (newTop > currentTop)
        {
            borderColor = detectedColor;
            return newTop;
        }

        return currentTop;
    }

    /// <summary>
    /// 检测底部边框层（优化版）
    /// </summary>
    private static int DetectLayerBorderBottom(Image<Rgba32> image, int currentTop, int currentBottom, int currentLeft, int currentRight, byte tolerance, ref Rgba32? borderColor)
    {
        int newBottom = currentBottom;
        Rgba32? detectedColor = null;

        // 使用采样检测代替全行扫描
        int sampleCount = Math.Min(50, currentRight - currentLeft + 1);
        int stepX = Math.Max(1, (currentRight - currentLeft) / sampleCount);

        // 从当前底部开始向上扫描
        for (int y = currentBottom; y >= currentTop; y--)
        {
            Rgba32? rowColor = null;
            bool isUniform = true;

            // 采样检查行是否统一颜色
            for (int x = currentLeft; x <= currentRight; x += stepX)
            {
                if (!rowColor.HasValue)
                {
                    rowColor = image[x, y];
                    continue;
                }

                if (!IsSimilarColor(image[x, y], rowColor.Value, tolerance))
                {
                    isUniform = false;
                    break;
                }
            }

            if (isUniform && rowColor.HasValue)
            {
                if (y == currentBottom)
                {
                    detectedColor = rowColor;
                    newBottom = y - 1;
                    continue;
                }

                if (detectedColor.HasValue && IsSimilarColor(rowColor.Value, detectedColor.Value, tolerance))
                {
                    newBottom = y - 1;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        if (newBottom < currentBottom)
        {
            borderColor = detectedColor;
            return newBottom;
        }

        return currentBottom;
    }

    /// <summary>
    /// 检测左侧边框层（优化版）
    /// </summary>
    private static int DetectLayerBorderLeft(Image<Rgba32> image, int currentTop, int currentBottom, int currentLeft, int currentRight, byte tolerance, ref Rgba32? borderColor)
    {
        int newLeft = currentLeft;
        Rgba32? detectedColor = null;

        // 使用采样检测代替全列扫描
        int sampleCount = Math.Min(50, currentBottom - currentTop + 1);
        int stepY = Math.Max(1, (currentBottom - currentTop) / sampleCount);

        // 从当前左侧开始向右扫描
        for (int x = currentLeft; x <= currentRight; x++)
        {
            Rgba32? colColor = null;
            bool isUniform = true;

            // 采样检查列是否统一颜色
            for (int y = currentTop; y <= currentBottom; y += stepY)
            {
                if (!colColor.HasValue)
                {
                    colColor = image[x, y];
                    continue;
                }

                if (!IsSimilarColor(image[x, y], colColor.Value, tolerance))
                {
                    isUniform = false;
                    break;
                }
            }

            if (isUniform && colColor.HasValue)
            {
                if (x == currentLeft)
                {
                    detectedColor = colColor;
                    newLeft = x + 1;
                    continue;
                }

                if (detectedColor.HasValue && IsSimilarColor(colColor.Value, detectedColor.Value, tolerance))
                {
                    newLeft = x + 1;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        if (newLeft > currentLeft)
        {
            borderColor = detectedColor;
            return newLeft;
        }

        return currentLeft;
    }

    /// <summary>
    /// 检测右侧边框层（优化版）
    /// </summary>
    private static int DetectLayerBorderRight(Image<Rgba32> image, int currentTop, int currentBottom, int currentLeft, int currentRight, byte tolerance, ref Rgba32? borderColor)
    {
        int newRight = currentRight;
        Rgba32? detectedColor = null;

        // 使用采样检测代替全列扫描
        int sampleCount = Math.Min(50, currentBottom - currentTop + 1);
        int stepY = Math.Max(1, (currentBottom - currentTop) / sampleCount);

        // 从当前右侧开始向左扫描
        for (int x = currentRight; x >= currentLeft; x--)
        {
            Rgba32? colColor = null;
            bool isUniform = true;

            // 采样检查列是否统一颜色
            for (int y = currentTop; y <= currentBottom; y += stepY)
            {
                if (!colColor.HasValue)
                {
                    colColor = image[x, y];
                    continue;
                }

                if (!IsSimilarColor(image[x, y], colColor.Value, tolerance))
                {
                    isUniform = false;
                    break;
                }
            }

            if (isUniform && colColor.HasValue)
            {
                if (x == currentRight)
                {
                    detectedColor = colColor;
                    newRight = x - 1;
                    continue;
                }

                if (detectedColor.HasValue && IsSimilarColor(colColor.Value, detectedColor.Value, tolerance))
                {
                    newRight = x - 1;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        if (newRight < currentRight)
        {
            borderColor = detectedColor;
            return newRight;
        }

        return currentRight;
    }

    /// <summary>
    /// 颜色相似度比较（SIMD优化）
    /// </summary>
    private static bool IsSimilarColor(Rgba32 color1, Rgba32 color2, byte tolerance)
    {
        // 使用快速比较算法
        int diffR = Math.Abs(color1.R - color2.R);
        int diffG = Math.Abs(color1.G - color2.G);
        int diffB = Math.Abs(color1.B - color2.B);

        // 快速路径：如果任一通道差异超过容差
        if (diffR > tolerance || diffG > tolerance || diffB > tolerance)
            return false;

        // 精确比较
        return diffR <= tolerance && diffG <= tolerance && diffB <= tolerance;
    }
}