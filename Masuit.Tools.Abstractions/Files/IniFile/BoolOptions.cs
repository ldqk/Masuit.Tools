using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Files;

/// <summary>
/// 布尔类型的特殊处理选项
/// </summary>
public class BoolOptions
{
    private Dictionary<string, bool> _boolStringLookup;
    private string _trueString = "true";
    private string _falseString = "false";

    /// <summary>
    /// 非0值作为true
    /// </summary>
    public bool NonZeroNumbersAreTrue { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="comparer"></param>
    public BoolOptions(StringComparer comparer = null)
    {
        _boolStringLookup = new Dictionary<string, bool>(comparer ?? StringComparer.CurrentCultureIgnoreCase)
        {
            [_trueString] = true,
            [_falseString] = false,
            ["yes"] = true,
            ["no"] = false,
            ["on"] = true,
            ["off"] = false,
            ["1"] = true,
            ["0"] = false,
            ["y"] = true,
            ["n"] = false,
        };
        NonZeroNumbersAreTrue = true;
    }

    /// <summary>
    /// 设置布尔值对应的词语
    /// </summary>
    /// <param name="words"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public void SetBoolWords(IEnumerable<BoolWord> words)
    {
        if (words == null)
        {
            throw new ArgumentNullException(nameof(words));
        }

        var word = words.FirstOrDefault(w => w.Value);
        if (word == null)
        {
            throw new InvalidOperationException("布尔词列表不包含“true”值的条目。");
        }

        _trueString = word.Word;
        word = words.FirstOrDefault(w => w.Value == false);
        if (word == null)
        {
            throw new InvalidOperationException("布尔词列表不包含“false”值的条目。");
        }

        _falseString = word.Word;
        _boolStringLookup = words.ToDictionary(w => w.Word, w => w.Value);
    }

    internal string ToString(bool value) => value ? _trueString : _falseString;

    internal bool TryParse(string s, out bool value)
    {
        if (s != null)
        {
            if (_boolStringLookup.TryGetValue(s, out bool b))
            {
                value = b;
                return true;
            }

            if (NonZeroNumbersAreTrue && int.TryParse(s, out int i))
            {
                value = i != 0;
                return true;
            }
        }

        value = false;
        return false;
    }
}