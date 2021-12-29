using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Masuit.Tools.Core.Validator;

/// <summary>
/// 元素个数校验
/// </summary>
public class MinItemsCountAttribute : ValidationAttribute
{
    private int MinItems { get; }

    /// <summary>
    /// 最小个数
    /// </summary>
    /// <param name="value"></param>
    public MinItemsCountAttribute(int value)
    {
        MinItems = value;
    }

    /// <summary>
    /// 校验
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override bool IsValid(object value)
    {
        if (value is null)
        {
            return false;
        }

        var list = value as IList;
        return list.Count >= MinItems;
    }
}
