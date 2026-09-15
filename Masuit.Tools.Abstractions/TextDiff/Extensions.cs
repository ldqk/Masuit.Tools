using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Text;

namespace Masuit.Tools.TextDiff;

public static partial class Extensions
{
	internal static IEnumerable<T> Concat<T>(this IEnumerable<T> items, T item)
	{
		foreach (var i in items)
		{
			yield return i;
		}

		yield return item;
	}

	internal static IEnumerable<T> ItemAsEnumerable<T>(this T item)
	{
		yield return item;
	}

	internal static void Splice<T>(this List<T> input, int start, int count, params T[] objects) => input.Splice(start, count, (IEnumerable<T>)objects);

	internal static void Splice<T>(this List<T> input, int start, int count, IEnumerable<T> objects)
	{
		input.RemoveRange(start, count);
		input.InsertRange(start, objects);
	}

	internal static IEnumerable<string> SplitBy(this string s, char separator)
	{
		StringBuilder sb = new();
		foreach (var c in s)
		{
			if (c == separator)
			{
				yield return sb.ToString();
				sb.Clear();
			}
			else
			{
				sb.Append(c);
			}
		}

		if (sb.Length > 0)
		{
			yield return sb.ToString();
		}
	}

	/// <summary>
	/// 比较两段 HTML 文本，并使用 <c>del</c> 和 <c>ins</c> 标记差异。
	/// </summary>
	/// <param name="text1">原始 HTML 文本。</param>
	/// <param name="text2">新 HTML 文本。</param>
	/// <param name="maxUnchangedLength">两侧均有差异时，一并标记的最长未变文本长度。设为 <c>0</c> 时不合并。</param>
	/// <returns>分别标记删除内容和插入内容的 HTML。</returns>
	public static (string html1, string html2) HtmlDiff(this string text1, string text2, int maxUnchangedLength = 0)
	{
		if (maxUnchangedLength < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(maxUnchangedLength));
		}

		if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
		{
			return (text1, text2);
		}

		var regex = new Regex(@"<pre[\s\S]*?</pre>|<[^>]+>");
		const string sep = "\f";
#if NETSTANDARD2_1_OR_GREATER
		var tags1 = regex.Matches(text1).Select(m => m.Value).Append("").ToArray();
		var tags2 = regex.Matches(text2).Select(m => m.Value).Append("").ToArray();
#else
		var tags1 = regex.Matches(text1).Cast<Match>().Select(m => m.Value).Append("").ToArray();
		var tags2 = regex.Matches(text2).Cast<Match>().Select(m => m.Value).Append("").ToArray();
#endif
		var html1 = regex.Replace(text1, sep);
		var html2 = regex.Replace(text2, sep);
		var diffs = MergeShortEqualities(TextDiffer.Compute(html1, html2), maxUnchangedLength, sep[0]);
		var deletions = diffs.Where(d => d.Operation != DiffOperation.Insert);
		var insertions = diffs.Where(d => d.Operation != DiffOperation.Delete);
		if (maxUnchangedLength > 0)
		{
			deletions = deletions.CleanupMerge();
			insertions = insertions.CleanupMerge();
		}

		var s1 = deletions.Select(diff => diff.Operation == DiffOperation.Equal || string.IsNullOrWhiteSpace(diff.Text) ? diff.Text : diff.Text.Split(sep[0]).Select(s => string.IsNullOrWhiteSpace(s) ? s : $"<del>{s}</del>").Join(sep)).Join("");
		var s2 = insertions.Select(diff => diff.Operation == DiffOperation.Equal || string.IsNullOrWhiteSpace(diff.Text) ? diff.Text : diff.Text.Split(sep[0]).Select(s => string.IsNullOrWhiteSpace(s) ? s : $"<ins>{s}</ins>").Join(sep)).Join("");
		return (s1.Split(sep[0]).Select((s, i) => s + tags1[i]).Join(""), s2.Split(sep[0]).Select((s, i) => s + tags2[i]).Join(""));
	}

	private static IEnumerable<TextDiffer> MergeShortEqualities(IEnumerable<TextDiffer> diffs, int maxUnchangedLength, char tagSeparator)
	{
		var items = diffs.ToList();
		if (maxUnchangedLength == 0)
		{
			return items;
		}

		for (var i = 1; i < items.Count - 1; i++)
		{
			var diff = items[i];
			if (diff.Operation == DiffOperation.Equal && diff.Text.Length <= maxUnchangedLength && !diff.Text.Contains(tagSeparator) && items[i - 1].Operation != DiffOperation.Equal && items[i + 1].Operation != DiffOperation.Equal)
			{
				items[i] = TextDiffer.Delete(diff.Text);
				items.Insert(++i, TextDiffer.Insert(diff.Text));
			}
		}

		return items;
	}

	/// <summary>
	/// 比较两段 HTML 文本，并将删除和插入差异合并输出。
	/// </summary>
	/// <param name="text1">原始 HTML 文本。</param>
	/// <param name="text2">新 HTML 文本。</param>
	/// <param name="maxUnchangedLength">两侧均有差异时，一并标记的最长未变文本长度。设为 <c>0</c> 时不合并。</param>
	/// <returns>合并后的差异 HTML。</returns>
	public static string HtmlDiffMerge(this string text1, string text2, int maxUnchangedLength = 0)
	{
		if (maxUnchangedLength < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(maxUnchangedLength));
		}

		if (string.IsNullOrWhiteSpace(text1))
		{
			return text2;
		}

		if (string.IsNullOrWhiteSpace(text2))
		{
			return text1;
		}

		var regex = new Regex(@"<pre[\s\S]*?</pre>|<[^>]+>");
		const string sep = "\f";
#if NETSTANDARD2_1_OR_GREATER
		var tags1 = regex.Matches(text1).Select(m => m.Value).Append("").ToQueue();
		var tags2 = regex.Matches(text2).Select(m => m.Value).Append("").ToQueue();
#else
		var tags1 = regex.Matches(text1).Cast<Match>().Select(m => m.Value).Append("").ToQueue();
		var tags2 = regex.Matches(text2).Cast<Match>().Select(m => m.Value).Append("").ToQueue();
#endif
		var html1 = regex.Replace(text1, sep);
		var html2 = regex.Replace(text2, sep);
		var diffs = MergeShortEqualities(TextDiffer.Compute(html1, html2), maxUnchangedLength, sep[0]).CleanupMerge();
		return diffs.Select(diff =>
		{
			switch (diff.Operation)
			{
				case DiffOperation.Equal:
					{
						var str = diff.Text;
						foreach (Match m in Regex.Matches(str, sep))
						{
							tags1.Dequeue();
							var tag = tags2.Dequeue();
							str = str.ReplaceFirst(m.Value, tag);
						}

						return str;
					}

				case DiffOperation.Delete:
					{
						var str = diff.Text.Split(sep[0]).Select(s => string.IsNullOrWhiteSpace(s) ? s : $"<del>{s}</del>").Join(sep);
						foreach (Match m in Regex.Matches(str, sep))
						{
							var tag = tags1.Dequeue();
							str = str.ReplaceFirst(m.Value, tag);
						}

						return str;
					}

				case DiffOperation.Insert:
					{
						var str = diff.Text.Split(sep[0]).Select(s => string.IsNullOrWhiteSpace(s) ? s : $"<ins>{s}</ins>").Join(sep);
						foreach (Match m in Regex.Matches(str, sep))
						{
							var tag = tags2.Dequeue();
							str = str.ReplaceFirst(m.Value, tag);
						}

						return str;
					}

				default:
					throw new ArgumentOutOfRangeException();
			}
		}).Join("");
	}
}