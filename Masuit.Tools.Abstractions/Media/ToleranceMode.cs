namespace Masuit.Tools.Media;

public enum ToleranceMode
{
    /// <summary>
    /// 通道
    /// </summary>
    Channel,

    /// <summary>
    /// 欧几里德距离法
    /// </summary>
    EuclideanDistance,

    /// <summary>
    /// ΔE1976
    /// </summary>
    DeltaE1976,

    /// <summary>
    /// ΔE1994
    /// </summary>
    DeltaE1994,

    /// <summary>
    /// ΔE2000
    /// </summary>
    DeltaE2000,

    /// <summary>
    /// ΔE CMC(l:c=2:1)
    /// </summary>
    DeltaECMC,
}