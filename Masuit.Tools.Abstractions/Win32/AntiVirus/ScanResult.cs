using System;

namespace Masuit.Tools.Win32.AntiVirus;

public class ScanResult
{
    /// <summary>
    /// 扫描结果
    /// </summary>
    public ResultCode Result { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string Msg { get; set; }
}

[Flags]
public enum ResultCode
{
    /// <summary>
    /// 未检测到
    /// </summary>
    NotDetected,

    /// <summary>
    /// 检测到
    /// </summary>
    Detected,

    /// <summary>
    /// 异常
    /// </summary>
    Exception
}
