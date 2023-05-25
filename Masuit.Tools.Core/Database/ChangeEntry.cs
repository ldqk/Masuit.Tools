using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Masuit.Tools.Core;

public class ChangeEntry<T>
{
    /// <summary>
    /// 所属实体
    /// </summary>
    public T Entity { get; set; }

    /// <summary>
    /// 实体类型
    /// </summary>
    public Type EntityType { get; set; }

    /// <summary>
    /// 变更类型
    /// </summary>
    public EntityState EntityState { get; set; }

    /// <summary>
    /// 字段变更信息
    /// </summary>
    public List<ChangePropertyInfo> ChangeProperties { get; set; }

    public string ChangeDescription => ChangeProperties.Select(p => p.ChangeDescription).Join("; ");

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"{ChangeDescription}，实体：{EntityType.FullName}，变更类型：{EntityState}";
    }
}

public class ChangeEntry : ChangeEntry<object>
{
}
