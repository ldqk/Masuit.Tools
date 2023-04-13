using Microsoft.EntityFrameworkCore;

namespace Masuit.Tools.Core;

public class ChangeEntry
{
    /// <summary>
    /// 所属实体
    /// </summary>
    public object Entity { get; set; }

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
}