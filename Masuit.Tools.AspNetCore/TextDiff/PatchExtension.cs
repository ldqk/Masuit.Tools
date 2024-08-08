using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using static Masuit.Tools.TextDiff.DiffOperation;

namespace Masuit.Tools.TextDiff;

public static class PatchExtension
{
	internal static readonly string NullPadding = new(Enumerable.Range(1, 4).Select(i => (char)i).ToArray());

	private static readonly Regex PatchHeader = new("^@@ -(\\d+),?(\\d*) \\+(\\d+),?(\\d*) @@$");

	/// <summary>
	/// 在文本的开始和结束处添加一些填充，以便边缘可以匹配某些内容。patch_apply内部调用
	/// </summary>
	/// <param name="patches"></param>
	/// <param name="padding"></param>
	/// <returns></returns>
	internal static IEnumerable<DiffPatch> AddPadding(this IEnumerable<DiffPatch> patches, string padding)
	{
		var paddingLength = padding.Length;
		using var enumerator = patches.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			yield break;
		}

		var current = enumerator.Current.Bump(paddingLength);
		var next = current;
		var isfirst = true;
		while (true)
		{
			var hasnext = enumerator.MoveNext();
			if (hasnext)
			{
				next = enumerator.Current.Bump(paddingLength);
			}

			yield return (isfirst, hasnext) switch
			{
				(true, false) => current.AddPadding(padding), // list has only one patch
				(true, true) => current.AddPaddingBegin(padding),
				(false, true) => current,
				(false, false) => current.AddPaddingEnd(padding)
			};

			isfirst = false;
			if (!hasnext) yield break;
			current = next;
		}
	}

	/// <summary>
	/// 获取补丁列表并重建文本
	/// </summary>
	/// <param name="patches"></param>
	/// <returns></returns>
	public static string ToText(this IEnumerable<DiffPatch> patches) => patches.Aggregate(new StringBuilder(), (sb, patch) => sb.Append(patch)).ToString();

	/// <summary>
	/// 解析补丁的文本表示，并返回补丁对象列表
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	public static ImmutableList<DiffPatch> ParsePatches(this string text) => ParseCore(text).ToImmutableList();

	private static IEnumerable<DiffPatch> ParseCore(string text)
	{
		if (text.Length == 0)
		{
			yield break;
		}

		var lines = text.SplitBy('\n').ToArray();
		var index = 0;
		while (index < lines.Length)
		{
			var line = lines[index];
			var m = PatchHeader.Match(line);
			if (!m.Success)
			{
				throw new ArgumentException("Invalid patch string: " + line);
			}

			var (start1, length1) = m.GetStartAndLength(1, 2);
			var (start2, length2) = m.GetStartAndLength(3, 4);
			index++;
			IEnumerable<TextDiffer> CreateDiffs()
			{
				while (index < lines.Length)
				{
					line = lines[index];
					if (!string.IsNullOrEmpty(line))
					{
						var sign = line[0];
						if (sign == '@')
						{
							break;
						}

						yield return sign switch
						{
							'+' => TextDiffer.Insert(line[1..].Replace("+", "%2b").UrlDecoded()),
							'-' => TextDiffer.Delete(line[1..].Replace("+", "%2b").UrlDecoded()),
							_ => TextDiffer.Equal(line[1..].Replace("+", "%2b").UrlDecoded())
						};
					}

					index++;
				}
			}

			yield return new DiffPatch(start1, length1, start2, length2, CreateDiffs().ToImmutableList());
		}
	}

	private static (int start, int length) GetStartAndLength(this Match m, int startIndex, int lengthIndex)
	{
		var lengthStr = m.Groups[lengthIndex].Value;
		var value = Convert.ToInt32(m.Groups[startIndex].Value);
		return lengthStr switch
		{
			"0" => (value, 0),
			"" => (value - 1, 1),
			_ => (value - 1, Convert.ToInt32(lengthStr))
		};
	}

	/// <summary>
	/// 将一组补丁合并到文本上。返回一个补丁文本，以及一个指示应用了哪些补丁应用成功
	/// </summary>
	/// <param name="patches"></param>
	/// <param name="text"></param>
	/// <returns></returns>
	public static (string newText, bool[] results) Apply(this IEnumerable<DiffPatch> patches, string text) => Apply(patches, text, MatchOption.Default, PatchOption.Default);

	public static (string newText, bool[] results) Apply(this IEnumerable<DiffPatch> patches, string text, MatchOption matchOption) => Apply(patches, text, matchOption, PatchOption.Default);

	/// <summary>
	/// 将一组补丁合并到文本上。返回一个补丁文本，以及一个指示应用了哪些补丁应用成功
	/// </summary>
	/// <param name="input"></param>
	/// <param name="text"></param>
	/// <param name="matchOption"></param>
	/// <param name="option"></param>
	/// <returns></returns>
	public static (string newText, bool[] results) Apply(this IEnumerable<DiffPatch> input, string text, MatchOption matchOption, PatchOption option)
	{
		if (!input.Any())
		{
			return (text, []);
		}

		var nullPadding = NullPadding;
		text = nullPadding + text + nullPadding;
		var patches = input.AddPadding(nullPadding).SplitMax().ToList();
		var x = 0;
		var delta = 0;
		var results = new bool[patches.Count];
		foreach (var aPatch in patches)
		{
			var expectedLoc = aPatch.Start2 + delta;
			var text1 = aPatch.Diffs.Text1();
			int startLoc;
			var endLoc = -1;
			if (text1.Length > TextDiffConstants.MatchMaxBits)
			{
				startLoc = text.FindBestMatchIndex(text1[..TextDiffConstants.MatchMaxBits], expectedLoc, matchOption);

				if (startLoc != -1)
				{
					endLoc = text.FindBestMatchIndex(text1[^TextDiffConstants.MatchMaxBits..], expectedLoc + text1.Length - TextDiffConstants.MatchMaxBits, matchOption);
					if (endLoc == -1 || startLoc >= endLoc)
					{
						startLoc = -1;
					}
				}
			}
			else
			{
				startLoc = text.FindBestMatchIndex(text1, expectedLoc, matchOption);
			}

			if (startLoc == -1)
			{
				results[x] = false;
				delta -= aPatch.Length2 - aPatch.Length1;
			}
			else
			{
				results[x] = true;
				delta = startLoc - expectedLoc;
				var actualEndLoc = endLoc == -1 ? Math.Min(startLoc + text1.Length, text.Length) : Math.Min(endLoc + TextDiffConstants.MatchMaxBits, text.Length);
				var text2 = text[startLoc..actualEndLoc];
				if (text1 == text2)
				{
					text = text[..startLoc] + aPatch.Diffs.Text2() + text[(startLoc + text1.Length)..];
				}
				else
				{
					var diffs = TextDiffer.Compute(text1, text2, 0f, false);
					if (text1.Length > TextDiffConstants.MatchMaxBits && diffs.Levenshtein() / (float)text1.Length > option.PatchDeleteThreshold)
					{
						results[x] = false;
					}
					else
					{
						diffs = diffs.CleanupSemanticLossless().ToImmutableList();
						var index1 = 0;
						foreach (var aDiff in aPatch.Diffs)
						{
							if (aDiff.Operation != Equal)
							{
								var index2 = diffs.FindEquivalentLocation2(index1);
								if (aDiff.Operation == Insert)
								{
									text = text.Insert(startLoc + index2, aDiff.Text);
								}
								else if (aDiff.Operation == Delete)
								{
									text = text.Remove(startLoc + index2, diffs.FindEquivalentLocation2(index1 + aDiff.Text.Length) - index2);
								}
							}

							if (aDiff.Operation != Delete)
							{
								index1 += aDiff.Text.Length;
							}
						}
					}
				}
			}

			x++;
		}

		text = text.Substring(nullPadding.Length, text.Length - 2 * nullPadding.Length);
		return (text, results);
	}

	internal static IEnumerable<DiffPatch> SplitMax(this IEnumerable<DiffPatch> patches, short patchMargin = 4)
	{
		const short patchSize = TextDiffConstants.MatchMaxBits;
		foreach (var patch in patches)
		{
			if (patch.Length1 <= patchSize)
			{
				yield return patch;
				continue;
			}

			var (start1, _, start2, _, diffs) = patch;
			var precontext = string.Empty;
			while (diffs.Any())
			{
				var (s1, l1, s2, l2, thediffs) = (start1 - precontext.Length, precontext.Length, start2 - precontext.Length, precontext.Length, new List<TextDiffer>());
				var empty = true;
				if (precontext.Length != 0)
				{
					thediffs.Add(TextDiffer.Equal(precontext));
				}

				while (diffs.Any() && l1 < patchSize - patchMargin)
				{
					var first = diffs[0];
					var diffType = diffs[0].Operation;
					var diffText = diffs[0].Text;
					if (first.Operation == Insert)
					{
						l2 += diffText.Length;
						start2 += diffText.Length;
						thediffs.Add(TextDiffer.Insert(diffText));
						diffs = diffs.RemoveAt(0);
						empty = false;
					}
					else if (first.IsLargeDelete(2 * patchSize) && thediffs.Count == 1 && thediffs[0].Operation == Equal)
					{
						l1 += diffText.Length;
						start1 += diffText.Length;
						thediffs.Add(TextDiffer.Delete(diffText));
						diffs = diffs.RemoveAt(0);
						empty = false;
					}
					else
					{
						var cutoff = diffText[..Math.Min(diffText.Length, patchSize - l1 - patchMargin)];
						l1 += cutoff.Length;
						start1 += cutoff.Length;
						if (diffType == Equal)
						{
							l2 += cutoff.Length;
							start2 += cutoff.Length;
						}
						else
						{
							empty = false;
						}

						thediffs.Add(TextDiffer.Create(diffType, cutoff));
						if (cutoff == first.Text)
						{
							diffs = diffs.RemoveAt(0);
						}
						else
						{
							diffs = diffs.RemoveAt(0).Insert(0, first with
							{
								Text = first.Text[cutoff.Length..]
							});
						}
					}
				}

				precontext = thediffs.Text2();
				precontext = precontext[Math.Max(0, precontext.Length - patchMargin)..];
				var text1 = diffs.Text1();
				var postcontext = text1.Length > patchMargin ? text1[..patchMargin] : text1;
				if (postcontext.Length != 0)
				{
					l1 += postcontext.Length;
					l2 += postcontext.Length;
					var lastDiff = thediffs.Last();
					if (thediffs.Count > 0 && lastDiff.Operation == Equal)
					{
						thediffs[^1] = lastDiff.Append(postcontext);
					}
					else
					{
						thediffs.Add(TextDiffer.Equal(postcontext));
					}
				}

				if (!empty)
				{
					yield return new DiffPatch(s1, l1, s2, l2, thediffs.ToImmutableList());
				}
			}
		}
	}
}