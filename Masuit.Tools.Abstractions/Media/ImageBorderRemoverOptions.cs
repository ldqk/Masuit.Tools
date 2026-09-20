namespace Masuit.Tools.Media;

/// <summary>
/// 图像边框检测参数。较小的采样数优先速度，较大的采样数更适合细小或不规则边框。
/// </summary>
public sealed class ImageBorderRemoverOptions
{
    /// <summary>每条扫描线最多采集的像素数。</summary>
    public int SampleCount { get; set; } = 64;

    /// <summary>边界扫描在线段两端忽略的比例，避免角部主体内容干扰。</summary>
    public double SampleMarginRatio { get; set; } = 0.15;

    /// <summary>一条扫描线被认定为边框所需的相似像素比例，范围为 0 到 1。</summary>
    public double MinimumSimilarRatio { get; set; } = 0.9;

    /// <summary>用于抵消 JPEG 压缩微小色差的附加容差。</summary>
    public int CompressionNoiseTolerance { get; set; } = 2;

    /// <summary>边框扫描允许的连续异常行数。</summary>
    public int MaximumBorderGaps { get; set; } = 1;

    /// <summary>边框扫描线允许的最大灰度方差，用于排除内容区域的高频细节。</summary>
    public double MaximumGrayVariance { get; set; } = 225;

    /// <summary>渐变边框扫描线允许的最大灰度方差。</summary>
    public double MaximumGradientVariance { get; set; } = 10000;

    /// <summary>渐变边框相邻采样点允许的最大灰度变化。</summary>
    public double MaximumGradientStep { get; set; } = 18;

    /// <summary>相邻扫描线灰度方差的突变阈值，用于定位渐变边框与内容的交界。</summary>
    public double VarianceTransitionThreshold { get; set; } = 400;

    /// <summary>扫描每个方向的最大像素数。0 表示使用较短边的三分之一。</summary>
    public int MaximumScanPixels { get; set; }

    /// <summary>忽略小于该厚度的偶发噪点边界。</summary>
    public int MinimumBorderThickness { get; set; } = 2;

    /// <summary>裁剪后内容区域最小宽度或高度。</summary>
    public int MinimumContentSize { get; set; } = 16;

    /// <summary>精裁时保留在内容周围的安全像素。</summary>
    public int SafetyPadding { get; set; }

    /// <summary>是否启用针对不规则边框的边缘密度精裁。</summary>
    public bool EnableContourRefinement { get; set; } = true;

    /// <summary>轮廓检测的采样步长。较大值降低超大图的计算量。</summary>
    public int ContourSampleStride { get; set; } = 2;

    /// <summary>Sobel 灰度梯度阈值，超过该值的像素被视为内容轮廓。</summary>
    public double ContourEdgeThreshold { get; set; } = 48;

    /// <summary>接受轮廓精裁所需的最少轮廓像素数。</summary>
    public int MinimumContourPixels { get; set; } = 32;

    /// <summary>轮廓精裁相对粗裁边界最多向内容区域推进的像素数。</summary>
    public int MaximumContourRefinementPixels { get; set; } = 32;
}