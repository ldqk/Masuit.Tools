using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Masuit.Tools.Security;

/// <summary>
/// 零宽字符串编解码器
/// </summary>
public static class ZeroWidthCodec
{
	/// <summary>
	/// 注入零宽字符串
	/// </summary>
	/// <param name="s"></param>
	/// <param name="hidden"></param>
	/// <returns></returns>
	public static string InjectZeroWidthString(this string s, string hidden)
	{
		return Encrypt(s, hidden);
	}

	/// <summary>
	/// 编码为零宽字符串
	/// </summary>
	/// <param name="s"></param>
	/// <returns></returns>
	public static string EncodeToZeroWidthText(this string s)
	{
		return Encode(s);
	}

	/// <summary>
	/// 解码零宽字符串
	/// </summary>
	/// <param name="s"></param>
	/// <returns></returns>
	public static string DecodeZeroWidthString(this string s)
	{
		return Decrypt(s);
	}

	/// <summary>
	/// 注入零宽字符串
	/// </summary>
	/// <param name="pub">显示字符串</param>
	/// <param name="hidden">隐式字符串</param>
	/// <returns></returns>
	public static string Encrypt(string pub, string hidden)
	{
		// 将字符串拆分成单个字符
		var pubMsg = pub.ToCharArray();
		var hiddenMsg = Str2Bin(Encoding.UTF8.GetBytes(hidden));// 将需要隐藏的字符串转换成二进制格式
		hiddenMsg = Bin2Hidden(hiddenMsg);// 将二进制转换成隐藏字符格式
		hiddenMsg = $"\uFEFF{hiddenMsg}\uFEFF";// 用边界字符包装隐藏的字符
		if (pubMsg.Length == 1)
		{
			return pub + hiddenMsg;
		}

		// 将编码的隐藏消息注入到公共字符串的大约中间位置
		int half = pub.Length / 2;
		var chars = new List<char>();
		for (var i = 0; i < pubMsg.Length; i++)
		{
			if (i == half)
			{
				chars.AddRange(hiddenMsg);
			}

			chars.Add(pubMsg[i]);
		}

		return string.Concat(chars);
	}

	/// <summary>
	/// 编码为零宽字符串
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static string Encode(string str)
	{
		var encodeText = new StringBuilder(string.Join(" ", Encoding.UTF8.GetBytes(str).Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0'))))
			.Replace('\u0030', '\u200B')
			.Replace('\u0031', '\u200C')
			.Replace('\u0020', '\u200D')
			.ToString();
		return encodeText;
	}

	/// <summary>
	/// 解码零宽字符串
	/// </summary>
	/// <param name="pub"></param>
	/// <returns></returns>
	public static string Decrypt(string pub)
	{
		var unwrapped = Unwrap(pub);
		var message = Bin2Str(unwrapped == "false" ? Hidden2Bin(pub) : Hidden2Bin(unwrapped));
		return message.Length < 2 ? "No private message was found in that text." : message;
	}

	private static string Str2Bin(byte[] text)
	{
		return string.Join(" ", text.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
	}

	private static string Unwrap(string text)
	{
		var tmp = text.Split("\uFEFF".ToCharArray());
		return tmp.Length == 1 ? "false" : tmp[1];
	}

	private static string Bin2Str(string bin)
	{
		bin = Regex.Replace(bin, "[^0-1]", " ");
		return Encoding.UTF8.GetString(Enumerable.Range(0, bin.Split().Length).Select(vw => Convert.ToByte(Regex.Replace(bin.Split()[vw], @"\s", ""), 2)).ToArray());
	}

	private static string Bin2Hidden(string text)
	{
		text = text.Replace(" ", "\u2060"); // Unicode Character 'WORD JOINER' (U+2060) 0xE2 0x81 0xA0
		text = text.Replace("0", "\u200B"); // Unicode Character 'ZERO WIDTH SPACE' (U+200B) 0xE2 0x80 0x8B
		text = text.Replace("1", "\u200C"); // Unicode Character 'ZERO WIDTH NON-JOINER' (U+200C) 0xE2 0x80 0x8C
		return text;
	}

	private static string Hidden2Bin(string text)
	{
		text = text.Replace("\u2060", " "); // Unicode Character 'WORD JOINER' (U+2060) 0xE2 0x81 0xA0
		text = text.Replace("\u200B", "0"); // Unicode Character 'ZERO WIDTH SPACE' (U+200B) 0xE2 0x80 0x8B
		text = text.Replace("\u200C", "1"); // Unicode Character 'ZERO WIDTH NON-JOINER' (U+200C) 0xE2 0x80 0x8C
		return text;
	}
}