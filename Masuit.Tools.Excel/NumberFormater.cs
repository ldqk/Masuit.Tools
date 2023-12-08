using System;
using System.Linq;

namespace Masuit.Tools.Excel;

/// <summary>
/// 数制格式化器
/// </summary>
internal class NumberFormater
{
    /// <summary>
    /// 数制表示字符集
    /// </summary>
    internal string Characters { get; set; }

    /// <summary>
    /// 进制长度
    /// </summary>
    public int Length => Characters.Length;

    /// <summary>
    /// 起始值偏移
    /// </summary>
    private readonly int _offset;

    /// <summary>
    /// 数制格式化器
    /// </summary>
    public NumberFormater()
    {
        Characters = "0123456789";
    }

    /// <summary>
    /// 数制格式化器
    /// </summary>
    /// <param name="characters">符号集</param>
    /// <param name="offset">起始值偏移</param>
    public NumberFormater(string characters, int offset = 0)
    {
        if (string.IsNullOrEmpty(characters))
        {
            throw new ArgumentException("符号集不能为空");
        }

        Characters = characters;
        _offset = offset;
    }

    /// <summary>
    /// 指定字符串转换为指定进制的数字形式
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public long FromString(string str)
    {
        int j = 0;
        return new string(str.ToCharArray().Reverse().ToArray()).Where(ch => Characters.Contains(ch)).Sum(ch => (Characters.IndexOf(ch) + _offset) * (long)Math.Pow(Length, j++));
    }
}
