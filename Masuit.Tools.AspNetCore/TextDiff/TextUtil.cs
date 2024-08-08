using System.Text;
using System.Text.RegularExpressions;

namespace Masuit.Tools.TextDiff;

internal static class TextUtil
{
	private static readonly Regex HexCode = new("%[0-9A-F][0-9A-F]");

	/// <summary>
	/// 求两个字符串的最长公共前子串长度
	/// </summary>
	/// <param name="text1"></param>
	/// <param name="text2"></param>
	/// <param name="i1">text1子字符串的起始索引</param>
	/// <param name="i2">text2子字符串的起始索引</param>
	/// <returns>每个字符串开头共有的字符数</returns>
	internal static int CommonPrefix(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2, int i1 = 0, int i2 = 0)
	{
		var l1 = text1.Length - i1;
		var l2 = text2.Length - i2;
		var n = Math.Min(l1, l2);
		for (var i = 0; i < n; i++)
		{
			if (text1[i + i1] != text2[i + i2])
			{
				return i;
			}
		}

		return n;
	}

	internal static int CommonPrefix(StringBuilder text1, StringBuilder text2)
	{
		var n = Math.Min(text1.Length, text2.Length);
		for (var i = 0; i < n; i++)
		{
			if (text1[i] != text2[i])
			{
				return i;
			}
		}

		return n;
	}

	/// <summary>
	/// 求两个字符串的最长公共后子串长度
	/// </summary>
	/// <param name="text1"></param>
	/// <param name="text2"></param>
	/// <param name="l1">text1的最大长度</param>
	/// <param name="l2">text2的最大长度</param>
	/// <returns>每个字符串末尾共有的字符数</returns>
	internal static int CommonSuffix(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2, int? l1 = null, int? l2 = null)
	{
		var text1Length = l1 ?? text1.Length;
		var text2Length = l2 ?? text2.Length;
		var n = Math.Min(text1Length, text2Length);
		for (var i = 1; i <= n; i++)
		{
			if (text1[text1Length - i] != text2[text2Length - i])
			{
				return i - 1;
			}
		}

		return n;
	}

	internal static int CommonSuffix(StringBuilder text1, StringBuilder text2)
	{
		var text1Length = text1.Length;
		var text2Length = text2.Length;
		var n = Math.Min(text1Length, text2Length);
		for (var i = 1; i <= n; i++)
		{
			if (text1[text1Length - i] != text2[text2Length - i])
			{
				return i - 1;
			}
		}

		return n;
	}

	/// <summary>
	/// 确定一个字符串的后缀是否是另一个字符串。返回第一个字符串末尾和第二个字符串开头共有的字符数。
	/// </summary>
	/// <param name="text1"></param>
	/// <param name="text2"></param>
	/// <returns></returns>
	internal static int CommonOverlap(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2)
	{
		var text1Length = text1.Length;
		var text2Length = text2.Length;
		if (text1Length == 0 || text2Length == 0)
		{
			return 0;
		}

		if (text1Length > text2Length)
		{
			text1 = text1[(text1Length - text2Length)..];
		}
		else if (text1Length < text2Length)
		{
			text2 = text2[..text1Length];
		}

		var last = text1[^1];
		for (var length = text2.Length; length > 0; length--)
		{
			if (text2[length - 1] == last && text1.EndsWith(text2[..length]))
			{
				return length;
			}
		}

		return 0;
	}

	/// <summary>
	/// 长文本中是否存在短文本的子字符串，使得子字符串至少是长文本长度的一半
	/// </summary>
	/// <param name="longtext"></param>
	/// <param name="shorttext"></param>
	/// <param name="i">在长文本内开始四分之一长度的子字符串索引位置</param>
	/// <returns></returns>
	private static HalfMatchResult HalfMatchI(ReadOnlySpan<char> longtext, ReadOnlySpan<char> shorttext, int i)
	{
		var seed = longtext.Slice(i, longtext.Length / 4);
		var j = -1;

		var bestCommon = string.Empty;
		string bestLongtextA = string.Empty, bestLongtextB = string.Empty;
		string bestShorttextA = string.Empty, bestShorttextB = string.Empty;

		int n = j;
		while (n < shorttext.Length && (j = shorttext[(j + 1)..].IndexOf(seed, StringComparison.Ordinal)) != -1)
		{
			j = n = j + n + 1;
			var prefixLength = CommonPrefix(longtext, shorttext, i, j);
			var suffixLength = CommonSuffix(longtext, shorttext, i, j);
			if (bestCommon.Length < suffixLength + prefixLength)
			{
				bestCommon = shorttext.Slice(j - suffixLength, suffixLength).ToString() + shorttext.Slice(j, prefixLength).ToString();
				bestLongtextA = longtext[..(i - suffixLength)].ToString();
				bestLongtextB = longtext[(i + prefixLength)..].ToString();
				bestShorttextA = shorttext[..(j - suffixLength)].ToString();
				bestShorttextB = shorttext[(j + prefixLength)..].ToString();
			}
		}

		return bestCommon.Length * 2 >= longtext.Length ? new(bestLongtextA, bestLongtextB, bestShorttextA, bestShorttextB, bestCommon) : HalfMatchResult.Empty;
	}

	/// <summary>
	/// 这两个文本是否共享一个子字符串，子字符串的长度至少是较长文本的一半？
	/// 这种加速会产生非最小的差异。
	/// </summary>
	/// <param name="text1"></param>
	/// <param name="text2"></param>
	/// <returns></returns>
	internal static HalfMatchResult HalfMatch(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2)
	{
		var longtext = text1.Length > text2.Length ? text1 : text2;
		var shorttext = text1.Length > text2.Length ? text2 : text1;
		if (longtext.Length < 4 || shorttext.Length * 2 < longtext.Length)
		{
			return HalfMatchResult.Empty;
		}

		var hm1 = HalfMatchI(longtext, shorttext, (longtext.Length + 3) / 4);
		var hm2 = HalfMatchI(longtext, shorttext, (longtext.Length + 1) / 2);
		var hm = (hm1, hm2) switch
		{
			{ hm1.IsEmpty: true } and { hm2.IsEmpty: true } => hm1,
			{ hm2.IsEmpty: true } => hm1,
			{ hm1.IsEmpty: true } => hm2,
			_ when hm1 > hm2 => hm1,
			_ => hm2
		};

		return text1.Length > text2.Length ? hm : -hm;
	}

	internal static string UrlEncoded(this string str)
	{
		const int maxLength = 0xFFEF;
		StringBuilder sb = new();
		var index = 0;
		while (index + maxLength < str.Length)
		{
			sb.Append(Uri.EscapeDataString(str.Substring(index, maxLength)));
			index += maxLength;
		}

		sb.Append(Uri.EscapeDataString(str[index..]));
		sb = sb.Replace('+', ' ').Replace("%20", " ").Replace("%21", "!").Replace("%2A", "*").Replace("%27", "'").Replace("%28", "(").Replace("%29", ")").Replace("%3B", ";").Replace("%2F", "/").Replace("%3F", "?").Replace("%3A", ":").Replace("%40", "@").Replace("%26", "&").Replace("%3D", "=").Replace("%2B", "+").Replace("%24", "$").Replace("%2C", ",").Replace("%23", "#");
		return HexCode.Replace(sb.ToString(), s => s.Value.ToLower());
	}

	internal static string UrlDecoded(this string str) => Uri.UnescapeDataString(str);

	/// <summary>
	/// 查找最匹配的索引位置
	/// 返回 -1 则未匹配到
	/// </summary>
	/// <param name="text"></param>
	/// <param name="pattern"></param>
	/// <param name="loc"></param>
	/// <param name="option"></param>
	/// <returns></returns>
	internal static int FindBestMatchIndex(this string text, string pattern, int loc, MatchOption option)
	{
		loc = Math.Max(0, Math.Min(loc, text.Length));

		if (text == pattern)
		{
			return 0;
		}

		if (text.Length == 0)
		{
			return -1;
		}

		if (loc + pattern.Length <= text.Length && text.AsSpan(loc, pattern.Length).SequenceEqual(pattern))
		{
			return loc;
		}

		var bitap = new BitapAlgorithm(option);
		return bitap.Match(text, pattern, loc);
	}
}