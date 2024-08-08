using System.Collections.Immutable;
using System.Text;
using static Masuit.Tools.TextDiff.DiffOperation;

namespace Masuit.Tools.TextDiff;

public record DiffPatch(int Start1, int Length1, int Start2, int Length2, SemanticsImmutableList<TextDiffer> Diffs)
{
	public bool IsEmpty => Diffs.IsEmpty;

	public DiffPatch Bump(int length) => this with
	{
		Start1 = Start1 + length,
		Start2 = Start2 + length
	};

	public bool StartsWith(DiffOperation operation) => Diffs[0].Operation == operation;
	public bool EndsWith(DiffOperation operation) => Diffs[^1].Operation == operation;

	internal DiffPatch AddPadding(string padding)
	{
		var (s1, l1, s2, l2, diffs) = this;
		var builder = diffs.ToBuilder();
		(s1, l1, s2, l2) = AddPaddingBegin(builder, s1, l1, s2, l2, padding);
		(s1, l1, s2, l2) = AddPaddingEnd(builder, s1, l1, s2, l2, padding);

		return new DiffPatch(s1, l1, s2, l2, builder.ToImmutable());
	}

	internal DiffPatch AddPaddingBegin(string padding)
	{
		var (s1, l1, s2, l2, diffs) = this;
		var builder = diffs.ToBuilder();
		(s1, l1, s2, l2) = AddPaddingBegin(builder, s1, l1, s2, l2, padding);
		return new DiffPatch(s1, l1, s2, l2, builder.ToImmutable());
	}

	private (int s1, int l1, int s2, int l2) AddPaddingBegin(ImmutableList<TextDiffer>.Builder builder, int s1, int l1, int s2, int l2, string padding)
	{
		if (!StartsWith(Equal))
		{
			builder.Insert(0, TextDiffer.Equal(padding));
			return (s1 - padding.Length, l1 + padding.Length, s2 - padding.Length, l2 + padding.Length);
		}

		if (padding.Length <= Diffs[0].Text.Length)
		{
			return (s1, l1, s2, l2);
		}

		var firstDiff = Diffs[0];
		var extraLength = padding.Length - firstDiff.Text.Length;
		var text = padding[firstDiff.Text.Length..] + firstDiff.Text;
		builder.RemoveAt(0);
		builder.Insert(0, firstDiff.Replace(text));
		return (s1 - extraLength, l1 + extraLength, s2 - extraLength, l2 + extraLength);
	}

	internal DiffPatch AddPaddingEnd(string padding)
	{
		var (s1, l1, s2, l2, diffs) = this;
		var builder = diffs.ToBuilder();
		(s1, l1, s2, l2) = AddPaddingEnd(builder, s1, l1, s2, l2, padding);
		return new DiffPatch(s1, l1, s2, l2, builder.ToImmutable());
	}

	private (int s1, int l1, int s2, int l2) AddPaddingEnd(ImmutableList<TextDiffer>.Builder builder, int s1, int l1, int s2, int l2, string padding)
	{
		if (!EndsWith(Equal))
		{
			builder.Add(TextDiffer.Equal(padding));
			return (s1, l1 + padding.Length, s2, l2 + padding.Length);
		}

		if (padding.Length <= Diffs[^1].Text.Length)
		{
			return (s1, l1, s2, l2);
		}

		var lastDiff = Diffs[^1];
		var extraLength = padding.Length - lastDiff.Text.Length;
		var text = lastDiff.Text + padding[..extraLength];
		builder.RemoveAt(builder.Count - 1);
		builder.Add(lastDiff.Replace(text));
		return (s1, l1 + extraLength, s2, l2 + extraLength);
	}

	/// <summary>
	/// 计算一个补丁列表，将text1转换为text2。将计算一组差异
	/// </summary>
	/// <param name="text1"></param>
	/// <param name="text2"></param>
	/// <param name="diffTimeout">超时限制</param>
	/// <param name="diffEditCost"></param>
	/// <returns></returns>
	public static SemanticsImmutableList<DiffPatch> Compute(string text1, string text2, float diffTimeout = 0, short diffEditCost = 4)
	{
		using var cts = diffTimeout <= 0 ? new CancellationTokenSource() : new CancellationTokenSource(TimeSpan.FromSeconds(diffTimeout));
		return Compute(text1, DiffAlgorithm.Compute(text1, text2, true, true, cts.Token).CleanupSemantic().CleanupEfficiency(diffEditCost)).ToImmutableList().WithValueSemantics();
	}

	/// <summary>
	/// 计算一个patch列表，将text1转换为text2。不提供text2，Diffs是text1和text2之间的差值
	/// </summary>
	/// <param name="text1"></param>
	/// <param name="diffs"></param>
	/// <param name="patchMargin"></param>
	/// <returns></returns>
	public static IEnumerable<DiffPatch> Compute(string text1, IEnumerable<TextDiffer> diffs, short patchMargin = 4)
	{
		if (!diffs.Any())
		{
			yield break;
		}

		var charCount1 = 0;
		var charCount2 = 0;
		var prepatchText = text1;
		var postpatchText = text1;
		var newdiffs = ImmutableList.CreateBuilder<TextDiffer>();
		int start1 = 0, length1 = 0, start2 = 0, length2 = 0;
		foreach (var aDiff in diffs)
		{
			if (!newdiffs.Any() && aDiff.Operation != Equal)
			{
				start1 = charCount1;
				start2 = charCount2;
			}

			switch (aDiff.Operation)
			{
				case Insert:
					newdiffs.Add(aDiff);
					length2 += aDiff.Text.Length;
					postpatchText = postpatchText.Insert(charCount2, aDiff.Text);
					break;

				case Delete:
					length1 += aDiff.Text.Length;
					newdiffs.Add(aDiff);
					postpatchText = postpatchText.Remove(charCount2, aDiff.Text.Length);
					break;

				case Equal:
					if (aDiff.Text.Length <= 2 * patchMargin && newdiffs.Any() && aDiff != diffs.Last())
					{
						newdiffs.Add(aDiff);
						length1 += aDiff.Text.Length;
						length2 += aDiff.Text.Length;
					}

					if (aDiff.Text.Length >= 2 * patchMargin)
					{
						if (newdiffs.Any())
						{
							(start1, length1, start2, length2) = newdiffs.AddContext(prepatchText, start1, length1, start2, length2);
							yield return new DiffPatch(start1, length1, start2, length2, newdiffs.ToImmutable());
							start1 = start2 = length1 = length2 = 0;
							newdiffs.Clear();

							// http://code.google.com/p/google-diff-match-patch/wiki/Unidiff
							prepatchText = postpatchText;
							charCount1 = charCount2;
						}
					}

					break;
			}

			if (aDiff.Operation != Insert)
			{
				charCount1 += aDiff.Text.Length;
			}

			if (aDiff.Operation != Delete)
			{
				charCount2 += aDiff.Text.Length;
			}
		}

		if (newdiffs.Any())
		{
			(start1, length1, start2, length2) = newdiffs.AddContext(prepatchText, start1, length1, start2, length2);
			yield return new DiffPatch(start1, length1, start2, length2, newdiffs.ToImmutable());
		}
	}

	/// <summary>
	/// 计算一个patch列表，将text1转换为text2。text1将从提供的Diff中导出。
	/// </summary>
	/// <param name="diffs"></param>
	/// <returns></returns>
	public static SemanticsImmutableList<DiffPatch> FromDiffs(IEnumerable<TextDiffer> diffs) => Compute(diffs.Text1(), diffs).ToImmutableList().WithValueSemantics();

	public override string ToString()
	{
		var coords1 = Length1 switch
		{
			0 => Start1 + ",0",
			1 => Convert.ToString(Start1 + 1),
			_ => Start1 + 1 + "," + Length1
		};

		var coords2 = Length2 switch
		{
			0 => Start2 + ",0",
			1 => Convert.ToString(Start2 + 1),
			_ => Start2 + 1 + "," + Length2
		};

		var text = new StringBuilder().Append("@@ -").Append(coords1).Append(" +").Append(coords2).Append(" @@\n");
		foreach (var aDiff in Diffs)
		{
			text.Append((char)aDiff.Operation);
			text.Append(aDiff.Text.UrlEncoded()).Append("\n");
		}

		return text.ToString();
	}
}