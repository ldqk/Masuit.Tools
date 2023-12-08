using System;
using System.ComponentModel.DataAnnotations;

namespace Masuit.Tools.Core.Validator;

/// <summary>
/// 枚举值校验
/// </summary>
public class EnumOfAttribute : ValidationAttribute
{
    private Type Type { get; set; }

    /// <summary>
    /// 枚举类型
    /// </summary>
    /// <param name="value"></param>
    public EnumOfAttribute(Type value)
    {
        Type = value;
    }

    public override bool IsValid(object value)
    {
        return value is null || Enum.IsDefined(Type, value);
    }
}

/// <summary>
/// 非空值校验
/// </summary>
public class NotNullOrEmptyAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        return value is not null && !value.IsNullOrEmpty();
    }
}
