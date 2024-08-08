using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Text;
using SixLabors.ImageSharp.Drawing;

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

	public static (string html1, string html2) HtmlDiff(this string text1, string text2)
	{
		if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
		{
			return (text1, text2);
		}

		var regex = new Regex(@"<pre[\s\S]*?</pre>|<[^>]+>");
		const string sep = "\f";
		var tags1 = regex.Matches(text1).Select(m => m.Value).Append("").ToArray();
		var tags2 = regex.Matches(text2).Select(m => m.Value).Append("").ToArray();
		var html1 = regex.Replace(text1, sep);
		var html2 = regex.Replace(text2, sep);
		var diffs = TextDiffer.Compute(html1, html2);
		var s1 = diffs.Where(d => d.Operation != DiffOperation.Insert).Select(diff => diff.Operation == DiffOperation.Equal || string.IsNullOrWhiteSpace(diff.Text) ? diff.Text : diff.Text.Split(sep).Select(s => string.IsNullOrWhiteSpace(s) ? s : $"<del>{s}</del>").Join(sep)).Join("");
		var s2 = diffs.Where(d => d.Operation != DiffOperation.Delete).Select(diff => diff.Operation == DiffOperation.Equal || string.IsNullOrWhiteSpace(diff.Text) ? diff.Text : diff.Text.Split(sep).Select(s => string.IsNullOrWhiteSpace(s) ? s : $"<ins>{s}</ins>").Join(sep)).Join("");
		return (s1.Split(sep).Select((s, i) => s + tags1[i]).Join(""), s2.Split(sep).Select((s, i) => s + tags2[i]).Join(""));
	}

	public static string HtmlDiffMerge(this string text1, string text2)
	{
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
		var tags1 = regex.Matches(text1).Select(m => m.Value).Append("").ToQueue();
		var tags2 = regex.Matches(text2).Select(m => m.Value).Append("").ToQueue();
		var html1 = regex.Replace(text1, sep);
		var html2 = regex.Replace(text2, sep);
		var diffs = TextDiffer.Compute(html1, html2);
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
						var str = diff.Text.Split(sep).Select(s => string.IsNullOrWhiteSpace(s) ? s : $"<del>{s}</del>").Join(sep);
						foreach (Match m in Regex.Matches(str, sep))
						{
							var tag = tags1.Dequeue();
							str = str.ReplaceFirst(m.Value, tag);
						}

						return str;
					}

				case DiffOperation.Insert:
					{
						var str = diff.Text.Split(sep).Select(s => string.IsNullOrWhiteSpace(s) ? s : $"<ins>{s}</ins>").Join(sep);
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