using System.ComponentModel;
using System.Reflection;

namespace Masuit.Tools.Core;

/// <summary>
/// 变更字段信息
/// </summary>
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

    /// <summary>
    /// 变更描述格式化字符串
    /// </summary>
    public string ChangeDescription => $"{PropertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? PropertyInfo.Name}：{OriginalValue} => {CurrentValue}";

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"{ChangeDescription}，字段：{PropertyInfo.Name}，主键：{IsPrimaryKey}，外键：{IsForeignKey}";
    }
}
