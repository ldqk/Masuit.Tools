using System.Reflection;

namespace Masuit.Tools.Core;

public class ChangePropertyInfo
{
    /// <summary>
    /// 属性
    /// </summary>
    public PropertyInfo PropertyInfo { get; set; }

    /// <summary>
    /// 原始值
    /// </summary>
    public object OriginalValue { get; set; }

    /// <summary>
    /// 新值
    /// </summary>
    public object CurrentValue { get; set; }

    /// <summary>
    /// 是否是主键
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// 是否是外键
    /// </summary>
    public bool IsForeignKey { get; set; }
}
