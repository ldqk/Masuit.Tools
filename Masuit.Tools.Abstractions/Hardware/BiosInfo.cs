namespace Masuit.Tools.Hardware;

/// <summary>
/// BIOS信息
/// </summary>
public class BiosInfo
{
    private static readonly BiosInfo _local = SystemInfo.GetBiosInfo();

    /// <summary>
    /// 本地实例
    /// </summary>
    public static BiosInfo Local => _local;

    /// <summary>
    /// ID
    /// </summary>
    public string ID { get; set; }

    /// <summary>
    /// 序列号
    /// </summary>
    public string SerialNumber { get; set; }
    
    /// <summary>
    /// 型号
    /// </summary>
    public string Model { get; set; }
    
    /// <summary>
    /// 制造商
    /// </summary>
    public string Manufacturer { get; set; }

}