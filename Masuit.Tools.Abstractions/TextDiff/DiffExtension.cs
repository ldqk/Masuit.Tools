using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using static Masuit.Tools.TextDiff.DiffOperation;

namespace Masuit.Tools.TextDiff;

public static class DiffExtension
{
	/// <summary>
	/// 还原文本1
	/// </summary>
	/// <param name="diffs"></param>
	/// <returns></returns>
	public static string Text1(this IEnumerable<TextDiffer> diffs) => diffs.Where(d => d.Operation != Insert).Aggregate(new StringBuilder(), (sb, diff) => sb.Append(diff.Text)).ToString();

	/// <summary>
	/// 还原文本2
	/// </summary>
	/// <param name="diffs"></param>
	/// <returns></returns>
	public static string Text2(this IEnumerable<TextDiffer> diffs) => diffs.Where(d => d.Operation != Delete).Aggregate(new StringBuilder(), (sb, diff) => sb.Append(diff.Text)).ToString();

	private readonly record struct LevenshteinState(int Insertions, int Deletions, int Levenshtein)
	{
		public LevenshteinState Consolidate() => new(0, 0, Levenshtein + Math.Max(Insertions, Deletions));
	}

	/// <summary>
	/// 计算Levenshtein距离；插入、删除或替换的字符数
	/// </summary>
	/// <param name="diffs"></param>
	/// <returns></returns>
	internal static int Levenshtein(this IEnumerable<TextDiffer> diffs)
	{
		var state = new LevenshteinState(0, 0, 0);
		state = diffs.Aggregate(state, (current, aDiff) => aDiff.Operation switch
		{
			Insert => current with
			{
				Insertions = current.Insertions + aDiff.Text.Length
			},
			Delete => current with
			{
				Deletions = current.Deletions + aDiff.Text.Length
			},
			Equal => current.Consolidate(),
			_ => throw new IndexOutOfRangeException()
		});

		return state.Consolidate().Levenshtein;
	}

	private static char ToDelta(this DiffOperation o) => o switch
	{
		Delete => '-',
		Insert => '+',
		Equal => '=',
		_ => throw new ArgumentException($"Unknown Operation: {o}")
	};

	/// <summary>
	/// 将diff压缩成一个编码字符串，该字符串描述了将text1转换为text2所需的操作。例如，=3\t-2\t+ing->：保留3个字符，删除2个字符，插入“ing”。操作以制表符分隔。插入的文本使用%xx符号转义。
	/// </summary>
	/// <param name="diffs"></param>
	/// <returns></returns>
	public static string ToDelta(this IEnumerable<TextDiffer> diffs)
	{
		var s = from aDiff in diffs
				let sign = aDiff.Operation.ToDelta()
				let textToAppend = aDiff.Operation == Insert ? aDiff.Text.UrlEncoded() : aDiff.Text.Length.ToString()
				select string.Concat(sign, textToAppend);
		return s.Join("\t");
	}

	private static DiffOperation FromDelta(char c) => c switch
	{
		'-' => Delete,
		'+' => Insert,
		'=' => Equal,
		_ => throw new ArgumentException($"Invalid Delta Token: {c}")
	};

	/// <summary>
	/// 从差异字符串中恢复diff对象
	/// </summary>
	/// <param name="text1"></param>
	/// <param name="delta"></param>
	/// <returns></returns>
	public static IEnumerable<TextDiffer> FromDelta(this string text1, string delta)
	{
		var pointer = 0;

		foreach (var token in delta.SplitBy('\t'))
		{
			if (token.Length == 0)
			{
				continue;
			}

			var param = token[1..];
			var operation = FromDelta(token[0]);
			int n = 0;
			if (operation != Insert)
			{
				if (!int.TryParse(param, out n))
				{
					throw new ArgumentException($"Invalid number in Diff.FromDelta: {param}");
				}

				if (pointer > text1.Length - n)
				{
					throw new ArgumentException($"Delta length ({pointer}) larger than source text length ({text1.Length}).");
				}
			}

			(var text, pointer) = operation switch
			{
				Insert => (param.Replace("+", "%2b").UrlDecoded(), pointer),
				Equal => (text1.Substring(pointer, n), pointer + n),
				Delete => (text1.Substring(pointer, n), pointer + n),
				_ => throw new ArgumentException($"Unknown Operation: {operation}")
			};
			yield return TextDiffer.Create(operation, text);
		}

		if (pointer != text1.Length)
		{
			throw new ArgumentException($"Delta length ({pointer}) smaller than source text length ({text1.Length}).");
		}
	}

	internal static IEnumerable<TextDiffer> CleanupMergePass1(this IEnumerable<TextDiffer> diffs)
	{
		var sbDelete = new StringBuilder();
		var sbInsert = new StringBuilder();
		var lastEquality = TextDiffer.Empty;

		using var enumerator = diffs.Concat(TextDiffer.Empty).GetEnumerator();
		while (enumerator.MoveNext())
		{
			var diff = enumerator.Current;

			(sbInsert, sbDelete) = diff.Operation switch
			{
				Insert => (sbInsert.Append(diff.Text), sbDelete),
				Delete => (sbInsert, sbDelete.Append(diff.Text)),
				_ => (sbInsert, sbDelete)
			};

			if (diff.Operation == Equal)
			{
				if (sbInsert.Length > 0 || sbDelete.Length > 0)
				{
					var prefixLength = TextUtil.CommonPrefix(sbInsert, sbDelete);
					if (prefixLength > 0)
					{
						var commonprefix = sbInsert.ToString(0, prefixLength);
						sbInsert.Remove(0, prefixLength);
						sbDelete.Remove(0, prefixLength);
						lastEquality = lastEquality.Append(commonprefix);
					}

					var suffixLength = TextUtil.CommonSuffix(sbInsert, sbDelete);
					if (suffixLength > 0)
					{
						var commonsuffix = sbInsert.ToString(sbInsert.Length - suffixLength, suffixLength);
						sbInsert.Remove(sbInsert.Length - suffixLength, suffixLength);
						sbDelete.Remove(sbDelete.Length - suffixLength, suffixLength);
						diff = diff.Prepend(commonsuffix);
					}

					if (!lastEquality.IsEmpty)
					{
						yield return lastEquality;
					}

#if NETSTANDARD2_1_OR_GREATER
                    if (sbDelete.Length > 0) yield return TextDiffer.Delete(sbDelete.ToString());
                    if (sbInsert.Length > 0) yield return TextDiffer.Insert(sbInsert.ToString());
#else
					if (sbDelete.Length > 0) yield return TextDiffer.Delete(sbDelete.ToString().AsSpan());
					if (sbInsert.Length > 0) yield return TextDiffer.Insert(sbInsert.ToString().AsSpan());
#endif
					lastEquality = diff;
					sbDelete.Clear();
					sbInsert.Clear();
				}
				else
				{
					lastEquality = lastEquality.Append(diff.Text);
				}
			}
		}

		if (!lastEquality.IsEmpty)
		{
			yield return lastEquality;
		}
	}

	internal static IEnumerable<TextDiffer> CleanupMergePass2(this IEnumerable<TextDiffer> input, out bool haschanges)
	{
		haschanges = false;
		var diffs = input.ToList();
		for (var i = 1; i < diffs.Count - 1; i++)
		{
			var previous = diffs[i - 1];
			var current = diffs[i];
			var next = diffs[i + 1];
			if (previous.Operation == Equal && next.Operation == Equal)
			{
				var currentSpan = current.Text.AsSpan();
				var previousSpan = previous.Text.AsSpan();
				var nextSpan = next.Text.AsSpan();
				if (currentSpan.Length >= previousSpan.Length && currentSpan[^previousSpan.Length..].SequenceEqual(previousSpan))
				{
					var text = previous.Text + current.Text[..^previous.Text.Length];
					diffs[i] = current.Replace(text);
					diffs[i + 1] = next.Replace(previous.Text + next.Text);
					diffs.Splice(i - 1, 1);
					haschanges = true;
				}
				else if (currentSpan.Length >= nextSpan.Length && currentSpan[..nextSpan.Length].SequenceEqual(nextSpan))
				{
					diffs[i - 1] = previous.Replace(previous.Text + next.Text);
					diffs[i] = current.Replace(current.Text[next.Text.Length..] + next.Text);
					diffs.Splice(i + 1, 1);
					haschanges = true;
				}
			}
		}

		return diffs;
	}

	internal static IEnumerable<TextDiffer> CleanupMerge(this IEnumerable<TextDiffer> diffs)
	{
		bool changes;
		do
		{
			diffs = diffs.CleanupMergePass1().CleanupMergePass2(out changes).ToList(); // required to detect if anything changed
		} while (changes);

		return diffs;
	}

	readonly record struct EditBetweenEqualities(string Equality1, string Edit, string Equality2)
	{
		public int Score => DiffCleanupSemanticScore(Equality1, Edit) + DiffCleanupSemanticScore(Edit, Equality2);

		private readonly record struct ScoreHelper(string Str, Index I, Regex Regex)
		{
			char C => Str[I];
			public bool IsEmpty => Str.Length == 0;
			public bool NonAlphaNumeric => !char.IsLetterOrDigit(C);
			public bool IsWhitespace => char.IsWhiteSpace(C);
			public bool IsLineBreak => C == '\n' || C == '\r';
			public bool IsBlankLine => IsLineBreak && Regex.IsMatch(Str);
		}

		/// 给定两个字符串，计算一个分数，表示内部边界是否落在逻辑边界上
		private static int DiffCleanupSemanticScore(string one, string two) => (h1: new ScoreHelper(one, ^1, BlankLineEnd), h2: new ScoreHelper(two, 0, BlankLineStart)) switch
		{
			{ h1.IsEmpty: true } or { h2.IsEmpty: true } => 6,
			{ h1.IsBlankLine: true } or { h2.IsBlankLine: true } => 5,
			{ h1.IsLineBreak: true } or { h2.IsLineBreak: true } => 4,
			{ h1.NonAlphaNumeric: true } and { h1.IsWhitespace: false } and { h2.IsWhitespace: true } => 3,
			{ h1.IsWhitespace: true } or { h2.IsWhitespace: true } => 2,
			{ h1.NonAlphaNumeric: true } or { h2.NonAlphaNumeric: true } => 1,
			_ => 0
		};

		// 将编辑尽可能向左移动
		public EditBetweenEqualities ShiftLeft()
		{
#if NETSTANDARD2_1_OR_GREATER
			var commonOffset = TextUtil.CommonSuffix(Equality1, Edit);
#else
			var commonOffset = TextUtil.CommonSuffix(Equality1.AsSpan(), Edit.AsSpan());
#endif
			if (commonOffset > 0)
			{
				var commonString = Edit[^commonOffset..];
				var equality1 = Equality1[..^commonOffset];
				var edit = commonString + Edit[..^commonOffset];
				var equality2 = commonString + Equality2;
				return new EditBetweenEqualities(Equality1: equality1, Edit: edit, Equality2: equality2);
			}

			return this;
		}

		private EditBetweenEqualities ShiftRight() => new(Equality1: Equality1 + Edit[0], Edit: Edit[1..] + Equality2[0], Equality2: Equality2[1..]);

		public IEnumerable<EditBetweenEqualities> TraverseRight()
		{
			var item = this;
			while (item.Edit.Length != 0 && item.Equality2.Length != 0 && item.Edit[0] == item.Equality2[0])
			{
				yield return item = item.ShiftRight();
			}
		}

		public IEnumerable<TextDiffer> ToDiffs(DiffOperation edit)
		{
#if NETSTANDARD2_1_OR_GREATER
			yield return TextDiffer.Equal(Equality1);
			yield return TextDiffer.Create(edit, Edit);
			yield return TextDiffer.Equal(Equality2);
#else
			yield return TextDiffer.Equal(Equality1.AsSpan());
			yield return TextDiffer.Create(edit, Edit);
			yield return TextDiffer.Equal(Equality2.AsSpan());
#endif
		}
	}

	/// <summary>
	/// 寻找两侧被等式包围的单个编辑，等式可以侧向移动，将编辑与单词边界对齐。
	/// e.g: The c<ins>at c</ins>ame. -> The <ins>cat </ins>came.
	/// </summary>
	/// <param name="diffs"></param>
	internal static IEnumerable<TextDiffer> CleanupSemanticLossless(this IEnumerable<TextDiffer> diffs)
	{
		using var enumerator = diffs.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			yield break;
		}

		var previous = enumerator.Current;
		if (!enumerator.MoveNext())
		{
			yield return previous;
			yield break;
		}

		var current = enumerator.Current;
		while (true)
		{
			if (!enumerator.MoveNext())
			{
				yield return previous;
				yield return current;
				yield break;
			}

			var next = enumerator.Current;
			if (previous.Operation == Equal && next.Operation == Equal)
			{
				var item = new EditBetweenEqualities(previous.Text, current.Text, next.Text).ShiftLeft();
				var best = item.TraverseRight().Aggregate(item, (best, x) => best.Score > x.Score ? best : x);
				if (previous.Text != best.Equality1)
				{
					foreach (var d in best.ToDiffs(current.Operation).Where(d => !d.IsEmpty))
					{
						yield return d;
					}

					if (!enumerator.MoveNext())
					{
						yield break;
					}

					previous = current;
					current = next;
					next = enumerator.Current;
				}
				else
				{
					yield return previous;
				}
			}
			else
			{
				yield return previous;
			}

			previous = current;
			current = next;
		}
	}

	private static readonly Regex BlankLineEnd = new("\\n\\r?\\n\\Z", RegexOptions.Compiled);

	private static readonly Regex BlankLineStart = new("\\A\\r?\\n\\r?\\n", RegexOptions.Compiled);

	/// <summary>
	/// 从效率上清理一些无用的差异对象
	/// </summary>
	/// <param name="input"></param>
	/// <param name="diffEditCost"></param>
	internal static IEnumerable<TextDiffer> CleanupEfficiency(this IEnumerable<TextDiffer> input, short diffEditCost = 4)
	{
		var diffs = input.ToList();
		var changes = false;
		var equalities = new Stack<int>();
		var lastEquality = string.Empty;
		var insertionBeforeLastEquality = false;
		var deletionBeforeLastEquality = false;
		var insertionAfterLastEquality = false;
		var deletionAfterLastEquality = false;
		for (var i = 0; i < diffs.Count; i++)
		{
			var diff = diffs[i];
			if (diff.Operation == Equal)
			{
				if (diff.Text.Length < diffEditCost && (insertionAfterLastEquality || deletionAfterLastEquality))
				{
					equalities.Push(i);
					(insertionBeforeLastEquality, deletionBeforeLastEquality) = (insertionAfterLastEquality, deletionAfterLastEquality);
					lastEquality = diff.Text;
				}
				else
				{
					equalities.Clear();
					lastEquality = string.Empty;
				}

				insertionAfterLastEquality = deletionAfterLastEquality = false;
			}
			else
			{
				if (diff.Operation == Delete)
				{
					deletionAfterLastEquality = true;
				}
				else
				{
					insertionAfterLastEquality = true;
				}

				/*
                 * 分割以下几种情况:
                 * <ins>A</ins><del>B</del>XY<ins>C</ins><del>D</del>
                 * <ins>A</ins>X<ins>C</ins><del>D</del>
                 * <ins>A</ins><del>B</del>X<ins>C</ins>
                 * <ins>A</del>X<ins>C</ins><del>D</del>
                 * <ins>A</ins><del>B</del>X<del>C</del>
                 */
				if ((lastEquality.Length != 0) && ((insertionBeforeLastEquality && deletionBeforeLastEquality && insertionAfterLastEquality && deletionAfterLastEquality) || ((lastEquality.Length < diffEditCost / 2) && (insertionBeforeLastEquality ? 1 : 0) + (deletionBeforeLastEquality ? 1 : 0) + (insertionAfterLastEquality ? 1 : 0) + (deletionAfterLastEquality ? 1 : 0) == 3)))
				{
#if NETSTANDARD2_1_OR_GREATER
					diffs.Splice(equalities.Peek(), 1, TextDiffer.Delete(lastEquality), TextDiffer.Insert(lastEquality));
#else
					diffs.Splice(equalities.Peek(), 1, TextDiffer.Delete(lastEquality.AsSpan()), TextDiffer.Insert(lastEquality.AsSpan()));
#endif
					equalities.Pop();
					lastEquality = string.Empty;
					if (insertionBeforeLastEquality && deletionBeforeLastEquality)
					{
						insertionAfterLastEquality = deletionAfterLastEquality = true;
						equalities.Clear();
					}
					else
					{
						if (equalities.Count > 0)
						{
							equalities.Pop();
						}

						i = equalities.Count > 0 ? equalities.Peek() : -1;
						insertionAfterLastEquality = deletionAfterLastEquality = false;
					}

					changes = true;
				}
			}
		}

		if (changes)
		{
			return diffs.CleanupMerge();
		}

		return input;
	}

	/// <summary>
	/// 两个不相关文本的差异可以用巧合的匹配来填充。例如，“mouse”和“sofas”的区别是
	/// `[(-1, "m"), (1, "s"), (0, "o"), (-1, "u"), (1, "fa"), (0, "s"), (-1, "e")]`.
	/// 虽然这是最佳差异，但人类很难理解。语义清理重写了差异，将其扩展为更易于理解的格式。上述示例将变为: `[(-1, "mouse"), (1, "sofas")]`.
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static IImmutableList<TextDiffer> MakeHumanReadable(this IEnumerable<TextDiffer> input) => input.CleanupSemantic().ToImmutableList();

	/// <summary>
	/// 此函数类似于“OptimizeForReadability”，除了它不是将差异优化为人类可读，而是优化差异以提高机器处理的效率。两种清理类型的结果通常是相同的。CleanupEfficiency就是基于这样的，即由大量小差异编辑组成的差异可能需要更长的时间来处理（在下游应用程序中），或者需要比少量较大差异更多的存储或传输容量。
	/// </summary>
	/// <param name="input"></param>
	/// <param name="diffEditCost">处理新编辑的成本，即处理现有编辑中的额外字符。默认值为4，这意味着如果将差异的长度扩展三个字符可以消除一次编辑，那么这种优化将降低总成本</param>
	/// <returns></returns>
	public static IImmutableList<TextDiffer> OptimizeForMachineProcessing(this IEnumerable<TextDiffer> input, short diffEditCost = 4) => input.CleanupEfficiency(diffEditCost).ToImmutableList();

	/// <summary>
	/// 从语法上清理一些无用的差异对象
	/// </summary>
	/// <param name="input"></param>
	internal static List<TextDiffer> CleanupSemantic(this IEnumerable<TextDiffer> input)
	{
		var diffs = input.ToList();
		var equalities = new Stack<int>();
		string lastEquality = null;
		var pointer = 0;
		var lengthInsertions1 = 0;
		var lengthDeletions1 = 0;
		var lengthInsertions2 = 0;
		var lengthDeletions2 = 0;
		while (pointer < diffs.Count)
		{
			if (diffs[pointer].Operation == Equal)
			{
				equalities.Push(pointer);
				lengthInsertions1 = lengthInsertions2;
				lengthDeletions1 = lengthDeletions2;
				lengthInsertions2 = 0;
				lengthDeletions2 = 0;
				lastEquality = diffs[pointer].Text;
			}
			else
			{
				if (diffs[pointer].Operation == Insert)
				{
					lengthInsertions2 += diffs[pointer].Text.Length;
				}
				else
				{
					lengthDeletions2 += diffs[pointer].Text.Length;
				}

				if (lastEquality != null && (lastEquality.Length <= Math.Max(lengthInsertions1, lengthDeletions1)) && (lastEquality.Length <= Math.Max(lengthInsertions2, lengthDeletions2)))
				{
#if NETSTANDARD2_1_OR_GREATER
					diffs.Splice(equalities.Peek(), 1, TextDiffer.Delete(lastEquality), TextDiffer.Insert(lastEquality));
#else
					diffs.Splice(equalities.Peek(), 1, TextDiffer.Delete(lastEquality.AsSpan()), TextDiffer.Insert(lastEquality.AsSpan()));
#endif
					equalities.Pop();
					if (equalities.Count > 0)
					{
						equalities.Pop();
					}

					pointer = equalities.Count > 0 ? equalities.Peek() : -1;
					lengthInsertions1 = 0; // Reset the counters.
					lengthDeletions1 = 0;
					lengthInsertions2 = 0;
					lengthDeletions2 = 0;
					lastEquality = null;
				}
			}

			pointer++;
		}

		diffs = diffs.CleanupMerge().CleanupSemanticLossless().ToList();

		// 查找删除和插入之间的重叠.
		// e.g: <del>abcxxx</del><ins>xxxdef</ins>
		//   -> <del>abc</del>xxx<ins>def</ins>
		// e.g: <del>xxxabc</del><ins>defxxx</ins>
		//   -> <ins>def</ins>xxx<del>abc</del>
		// 只有当重叠与前面或后面的编辑一样大时，才提取重叠
		pointer = 1;
		while (pointer < diffs.Count)
		{
			if (diffs[pointer - 1].Operation == Delete && diffs[pointer].Operation == Insert)
			{
				var deletion = diffs[pointer - 1].Text.AsSpan();
				var insertion = diffs[pointer].Text.AsSpan();
				var overlapLength1 = TextUtil.CommonOverlap(deletion, insertion);
				var overlapLength2 = TextUtil.CommonOverlap(insertion, deletion);
				var minLength = Math.Min(deletion.Length, insertion.Length);

				TextDiffer[] newdiffs = null;
				if ((overlapLength1 >= overlapLength2) && (overlapLength1 >= minLength / 2.0))
				{
					newdiffs = new[]
					{
						TextDiffer.Delete(deletion.Slice(0, deletion.Length - overlapLength1).ToArray()),
						TextDiffer.Equal(insertion.Slice(0, overlapLength1).ToArray()),
						TextDiffer.Insert(insertion[overlapLength1..].ToArray())
					};
				}
				else if ((overlapLength2 >= overlapLength1) && overlapLength2 >= minLength / 2.0)
				{
					newdiffs = new[]
					{
						TextDiffer.Insert(insertion.Slice(0, insertion.Length - overlapLength2)),
						TextDiffer.Equal(deletion.Slice(0, overlapLength2)),
						TextDiffer.Delete(deletion[overlapLength2..])
					};
				}

				if (newdiffs != null)
				{
					diffs.Splice(pointer - 1, 2, newdiffs);
					pointer++;
				}

				pointer++;
			}

			pointer++;
		}

		return diffs;
	}

	/// <summary>
	/// 计算并返回目标文本中的等效位置
	/// </summary>
	/// <param name="diffs"></param>
	/// <param name="location1"></param>
	/// <returns></returns>
	internal static int FindEquivalentLocation2(this IEnumerable<TextDiffer> diffs, int location1)
	{
		var chars1 = 0;
		var chars2 = 0;
		var lastChars1 = 0;
		var lastChars2 = 0;
		var lastDiff = TextDiffer.Empty;
		foreach (var aDiff in diffs)
		{
			if (aDiff.Operation != Insert)
			{
				chars1 += aDiff.Text.Length;
			}

			if (aDiff.Operation != Delete)
			{
				chars2 += aDiff.Text.Length;
			}

			if (chars1 > location1)
			{
				lastDiff = aDiff;
				break;
			}

			lastChars1 = chars1;
			lastChars2 = chars2;
		}

		if (lastDiff.Operation == Delete)
		{
			return lastChars2;
		}

		return lastChars2 + (location1 - lastChars1);
	}
}