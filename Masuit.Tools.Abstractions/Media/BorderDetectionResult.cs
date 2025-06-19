using SixLabors.ImageSharp.PixelFormats;

namespace Masuit.Tools.Media;

/// <summary>
/// 边框检测结果（包含多层边框信息）
/// </summary>
public struct BorderDetectionResult
{
    private int CroppedBorderCount { get; set; }

    public BorderDetectionResult(int croppedBorderCount)
    {
        CroppedBorderCount = croppedBorderCount;
    }

    /// <summary>原始图片宽度</summary>
    public int ImageWidth { get; set; }

    /// <summary>原始图片高度</summary>
    public int ImageHeight { get; set; }

    /// <summary>内容上边界位置</summary>
    public int ContentTop { get; set; }

    /// <summary>内容下边界位置</summary>
    public int ContentBottom { get; set; }

    /// <summary>内容左边界位置</summary>
    public int ContentLeft { get; set; }

    /// <summary>内容右边界位置</summary>
    public int ContentRight { get; set; }

    /// <summary>检测到的边框层数</summary>
    public int BorderLayers { get; set; }

    /// <summary>边框颜色层次（从外到内）</summary>
    public List<Rgba32> BorderColors { get; set; }

    /// <summary>顶部边框总宽度（像素）</summary>
    public int TopBorderWidth => ContentTop;

    /// <summary>底部边框总宽度（像素）</summary>
    public int BottomBorderWidth => ImageHeight - 1 - ContentBottom;

    /// <summary>左侧边框总宽度（像素）</summary>
    public int LeftBorderWidth => ContentLeft;

    /// <summary>右侧边框总宽度（像素）</summary>
    public int RightBorderWidth => ImageWidth - 1 - ContentRight;

    /// <summary>是否有顶部边框</summary>
    public bool HasTopBorder => TopBorderWidth > 0;

    /// <summary>是否有底部边框</summary>
    public bool HasBottomBorder => BottomBorderWidth > 0;

    /// <summary>是否有左侧边框</summary>
    public bool HasLeftBorder => LeftBorderWidth > 0;

    /// <summary>是否有右侧边框</summary>
    public bool HasRightBorder => RightBorderWidth > 0;

    /// <summary>是否有任意边框</summary>
    public bool HasAnyBorder => BorderCount > 0;

    /// <summary>是否满足裁剪条件（至少两个边）</summary>
    public bool CanBeCropped => BorderCount >= CroppedBorderCount;

    public int BorderCount => (HasTopBorder ? 1 : 0) + (HasBottomBorder ? 1 : 0) + (HasLeftBorder ? 1 : 0) + (HasRightBorder ? 1 : 0);

    /// <summary>内容区域宽度</summary>
    public int ContentWidth => ContentRight - ContentLeft + 1;

    /// <summary>内容区域高度</summary>
    public int ContentHeight => ContentBottom - ContentTop + 1;
}