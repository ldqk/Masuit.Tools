using System.Collections.Immutable;

namespace Masuit.Tools.TextDiff;

internal static class DifferBuilder
{
	/// <summary>
	/// 添加上下文，直到它是唯一的，不要让模式扩展到Match_MaxBits之外
	/// </summary>
	/// <param name="diffListBuilder"></param>
	/// <param name="text"></param>
	/// <param name="length2"></param>
	/// <param name="patchMargin"></param>
	/// <param name="start1"></param>
	/// <param name="length1"></param>
	/// <param name="start2"></param>
	internal static (int start1, int length1, int start2, int length2) AddContext(this ImmutableList<TextDiffer>.Builder diffListBuilder, string text, int start1, int length1, int start2, int length2, short patchMargin = 4)
	{
		if (text.Length == 0)
		{
			return (start1, length1, start2, length2);
		}

		var pattern = text.Substring(start2, length1);
		var padding = 0;
		while (text.IndexOf(pattern, StringComparison.Ordinal) != text.LastIndexOf(pattern, StringComparison.Ordinal) && pattern.Length < TextDiffConstants.MatchMaxBits - patchMargin - patchMargin)
		{
			padding += patchMargin;
			var begin = Math.Max(0, start2 - padding);
			pattern = text[begin..Math.Min(text.Length, start2 + length1 + padding)];
		}

		padding += patchMargin;
		var begin1 = Math.Max(0, start2 - padding);
		var prefix = text[begin1..start2];
		if (prefix.Length != 0)
		{
#if NETSTANDARD2_1_OR_GREATER
			diffListBuilder.Insert(0, TextDiffer.Equal(prefix));
#else
			diffListBuilder.Insert(0, TextDiffer.Equal(prefix.AsSpan()));
#endif
		}

		var begin2 = start2 + length1;
		var length = Math.Min(text.Length, start2 + length1 + padding) - begin2;
		var suffix = text.Substring(begin2, length);
		if (suffix.Length != 0)
		{
#if NETSTANDARD2_1_OR_GREATER
			diffListBuilder.Add(TextDiffer.Equal(suffix));
#else
			diffListBuilder.Add(TextDiffer.Equal(suffix.AsSpan()));
#endif
		}

		start1 -= prefix.Length;
		start2 -= prefix.Length;
		length1 = length1 + prefix.Length + suffix.Length;
		length2 = length2 + prefix.Length + suffix.Length;
		return (start1, length1, start2, length2);
	}
}