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

        if (value is ICollection sources)
        {
            return sources.Count >= MinItems;
        }

        int num = 0;
        if (value is IEnumerable source)
        {
            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                checked
                {
                    ++num;
                    if (num >= MinItems)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
