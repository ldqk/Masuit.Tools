namespace Masuit.Tools.Win32.AntiVirus;

public enum AmsiResult
{
    /// <summary>
    /// 正常文件
    /// </summary>
    AmsiResultClean = 0,

    /// <summary>
    /// 正常文件
    /// </summary>
    AmsiResultNotDetected = 1,

    AmsiResultBlockedByAdminStart = 16384,
    AmsiResultBlockedByAdminEnd = 20479,

    /// <summary>
    /// 非正常文件
    /// </summary>
    AmsiResultDetected = 32768,

    /// <summary>
    /// 扫描失败
    /// </summary>
    ScanException = -1
}
