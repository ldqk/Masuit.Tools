using System.Collections.Generic;

namespace Masuit.Tools.Hardware;

/// <summary>
/// 磁盘信息
/// </summary>
public class DiskInfo
{
    private static readonly List<DiskInfo> _locals = SystemInfo.GetDiskInfo();

    /// <summary>
    /// 本地实例
    /// </summary>
    public static List<DiskInfo> Locals => _locals;
    /// <summary>
    /// 序列号
    /// </summary>
    public string SerialNumber { get; set; }

    /// <summary>
    /// 型号
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// 总容量
    /// </summary>
    public float Total { get; set; }
}