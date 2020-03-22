using System;

namespace Masuit.Tools.Core.AspNetCore
{
    /// <summary>
    /// 更新时忽略的字段，检测到这个attribute时AddOrUpdate将忽略改字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class UpdateIgnoreAttribute : Attribute
    {
    }
}