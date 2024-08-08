using System.Text;
using static Masuit.Tools.TextDiff.DiffOperation;

namespace Masuit.Tools.TextDiff;

internal static class DiffAlgorithm
{
	/// <summary>
	/// 找出两段文本之间的差异。通过在区分之前剥离文本中的常见前缀或后缀来简化
	/// </summary>
	/// <param name="text1"></param>
	/// <param name="text2"></param>
	/// <param name="checklines">加速标记。如果为false，则不要先运行行级差异来识别更改的区域。如果为true，则运行一个速度稍快但不太理想的差异</param>
	/// <param name="optimized">启用优化</param>
	/// <param name="token"></param>
	/// <returns></returns>
	internal static IEnumerable<TextDiffer> Compute(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2, bool checklines, bool optimized, CancellationToken token)
	{
		if (text1.Length == text2.Length && text1.Length == 0)
		{
			return [];
		}

		var commonlength = TextUtil.CommonPrefix(text1, text2);
		if (commonlength == text1.Length && commonlength == text2.Length)
		{
			return new List<TextDiffer>()
			{
				TextDiffer.Equal(text1)
			};
		}

		var commonprefix = text1.Slice(0, commonlength);
		text1 = text1[commonlength..];
		text2 = text2[commonlength..];
		commonlength = TextUtil.CommonSuffix(text1, text2);
		var commonsuffix = text1[^commonlength..];
		text1 = text1.Slice(0, text1.Length - commonlength);
		text2 = text2.Slice(0, text2.Length - commonlength);

		List<TextDiffer> diffs = new();
		if (commonprefix.Length != 0)
		{
			diffs.Insert(0, TextDiffer.Equal(commonprefix));
		}

		diffs.AddRange(ComputeCore(text1, text2, checklines, optimized, token));
		if (commonsuffix.Length != 0)
		{
			diffs.Add(TextDiffer.Equal(commonsuffix));
		}

		return diffs.CleanupMerge();
	}

	private static IEnumerable<TextDiffer> ComputeCore(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2, bool checklines, bool optimized, CancellationToken token)
	{
		if (text1.Length == 0)
		{
			return TextDiffer.Insert(text2).ItemAsEnumerable();
		}

		if (text2.Length == 0)
		{
			return TextDiffer.Delete(text1).ItemAsEnumerable();
		}

		var longtext = text1.Length > text2.Length ? text1 : text2;
		var shorttext = text1.Length > text2.Length ? text2 : text1;
		var i = longtext.IndexOf(shorttext, StringComparison.Ordinal);
		if (i != -1)
		{
			if (text1.Length > text2.Length)
			{
				return new[]
				{
					TextDiffer.Delete(longtext.Slice(0, i)),
					TextDiffer.Equal(shorttext),
					TextDiffer.Delete(longtext[(i + shorttext.Length)..])
				};
			}

			return new[]
			{
				TextDiffer.Insert(longtext.Slice(0, i)),
				TextDiffer.Equal(shorttext),
				TextDiffer.Insert(longtext[(i + shorttext.Length)..])
			};
		}

		if (shorttext.Length == 1)
		{
			return new[]
			{
				TextDiffer.Delete(text1),
				TextDiffer.Insert(text2)
			};
		}

		if (optimized)
		{
			var result = TextUtil.HalfMatch(text1, text2);
			if (!result.IsEmpty)
			{
				var diffsA = Compute(result.Prefix1, result.Prefix2, checklines, true, token);
				var diffsB = Compute(result.Suffix1, result.Suffix2, checklines, true, token);
				return diffsA.Concat(TextDiffer.Equal(result.CommonMiddle)).Concat(diffsB);
			}
		}

		if (checklines && text1.Length > 100 && text2.Length > 100)
		{
			return DiffLines(text1, text2, optimized, token);
		}

		return MyersDiffBisect(text1, text2, optimized, token);
	}

	/// <summary>
	/// 对两个字符串进行快速的行级差分，然后重新划分部分以获得更高的精度。这种加速会产生非最小的差异。
	/// </summary>
	/// <param name="text1"></param>
	/// <param name="text2"></param>
	/// <param name="optimized"></param>
	/// <param name="token"></param>
	/// <returns></returns>
	private static List<TextDiffer> DiffLines(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2, bool optimized, CancellationToken token)
	{
		var compressor = new StringCompressor();
		text1 = compressor.Compress(text1, char.MaxValue * 2 / 3);
		text2 = compressor.Compress(text2);
		var diffs = Compute(text1, text2, false, optimized, token).Select(diff => diff.Replace(compressor.Decompress(diff.Text))).ToList().CleanupSemantic();

		return RediffAfterDiffLines(diffs, optimized, token).ToList();
	}

	/// <summary>
	/// 逐个字符清除所有替换块
	/// </summary>
	/// <param name="diffs"></param>
	/// <param name="optimized"></param>
	/// <param name="token"></param>
	/// <returns></returns>
	private static IEnumerable<TextDiffer> RediffAfterDiffLines(IEnumerable<TextDiffer> diffs, bool optimized, CancellationToken token)
	{
		var ins = new StringBuilder();
		var del = new StringBuilder();
		foreach (var diff in diffs.Concat(TextDiffer.Empty))
		{
			(ins, del) = diff.Operation switch
			{
				Insert => (ins.Append(diff.Text), del),
				Delete => (ins, del.Append(diff.Text)),
				_ => (ins, del)
			};

			if (diff.Operation != Equal)
			{
				continue;
			}

			var consolidatedDiffsBeforeEqual = diff.Operation switch
			{
				Equal when ins.Length > 0 && del.Length > 0 => Compute(del.ToString(), ins.ToString(), false, optimized, token),
				Equal when del.Length > 0 => TextDiffer.Delete(del.ToString()).ItemAsEnumerable(),
				Equal when ins.Length > 0 => TextDiffer.Insert(ins.ToString()).ItemAsEnumerable(),
				_ => []
			};

			foreach (var d in consolidatedDiffsBeforeEqual)
			{
				yield return d;
			}

			if (!diff.IsEmpty)
				yield return diff;

			ins.Clear();
			del.Clear();
		}
	}

	/// <summary>
	/// 找到diff的“中间值”，将问题一分为二，并返回递归构造的diff。
	/// </summary>
	/// <param name="text1"></param>
	/// <param name="text2"></param>
	/// <param name="optimizeForSpeed"></param>
	/// <param name="token"></param>
	/// <returns></returns>
	internal static IEnumerable<TextDiffer> MyersDiffBisect(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2, bool optimizeForSpeed, CancellationToken token)
	{
		var text1Length = text1.Length;
		var text2Length = text2.Length;
		var maxD = (text1Length + text2Length + 1) / 2;
		var vOffset = maxD;
		var vLength = 2 * maxD;
		var v1 = new int[vLength];
		var v2 = new int[vLength];
		for (var x = 0; x < vLength; x++)
		{
			v1[x] = -1;
		}

		for (var x = 0; x < vLength; x++)
		{
			v2[x] = -1;
		}

		v1[vOffset + 1] = 0;
		v2[vOffset + 1] = 0;
		var delta = text1Length - text2Length;
		var front = delta % 2 != 0;
		var k1Start = 0;
		var k1End = 0;
		var k2Start = 0;
		var k2End = 0;
		for (var d = 0; d < maxD; d++)
		{
			if (token.IsCancellationRequested)
			{
				break;
			}

			for (var k1 = -d + k1Start; k1 <= d - k1End; k1 += 2)
			{
				var k1Offset = vOffset + k1;
				int x1;
				if (k1 == -d || k1 != d && v1[k1Offset - 1] < v1[k1Offset + 1])
				{
					x1 = v1[k1Offset + 1];
				}
				else
				{
					x1 = v1[k1Offset - 1] + 1;
				}

				var y1 = x1 - k1;
				while (x1 < text1Length && y1 < text2Length && text1[x1] == text2[y1])
				{
					x1++;
					y1++;
				}

				v1[k1Offset] = x1;
				if (x1 > text1Length)
				{
					k1End += 2;
				}
				else if (y1 > text2Length)
				{
					k1Start += 2;
				}
				else if (front)
				{
					var k2Offset = vOffset + delta - k1;
					if (k2Offset >= 0 && k2Offset < vLength && v2[k2Offset] != -1)
					{
						var x2 = text1Length - v2[k2Offset];
						if (x1 >= x2)
						{
							return BisectSplit(text1, text2, x1, y1, optimizeForSpeed, token);
						}
					}
				}
			}

			for (var k2 = -d + k2Start; k2 <= d - k2End; k2 += 2)
			{
				var k2Offset = vOffset + k2;
				int x2;
				if (k2 == -d || k2 != d && v2[k2Offset - 1] < v2[k2Offset + 1])
				{
					x2 = v2[k2Offset + 1];
				}
				else
				{
					x2 = v2[k2Offset - 1] + 1;
				}

				var y2 = x2 - k2;
				while (x2 < text1Length && y2 < text2Length && text1[text1Length - x2 - 1] == text2[text2Length - y2 - 1])
				{
					x2++;
					y2++;
				}

				v2[k2Offset] = x2;
				if (x2 > text1Length)
				{
					k2End += 2;
				}
				else if (y2 > text2Length)
				{
					k2Start += 2;
				}
				else if (!front)
				{
					var k1Offset = vOffset + delta - k2;
					if (k1Offset >= 0 && k1Offset < vLength && v1[k1Offset] != -1)
					{
						var x1 = v1[k1Offset];
						var y1 = vOffset + x1 - k1Offset;
						x2 = text1Length - v2[k2Offset];
						if (x1 >= x2)
						{
							return BisectSplit(text1, text2, x1, y1, optimizeForSpeed, token);
						}
					}
				}
			}
		}

		return new[]
		{
			TextDiffer.Delete(text1),
			TextDiffer.Insert(text2)
		};
	}

	/// <summary>
	/// 给定“中值”的位置，将diff分成两部分并递归。
	/// </summary>
	/// <param name="text1"></param>
	/// <param name="text2"></param>
	/// <param name="x">文本1的中值位置.</param>
	/// <param name="y">文本2的中值位置.</param>
	/// <param name="optimized"></param>
	/// <param name="token"></param>
	/// <returns></returns>
	private static IEnumerable<TextDiffer> BisectSplit(ReadOnlySpan<char> text1, ReadOnlySpan<char> text2, int x, int y, bool optimized, CancellationToken token)
	{
		var text1A = text1[..x];
		var text2A = text2[..y];
		var text1B = text1[x..];
		var text2B = text2[y..];
		var diffsa = Compute(text1A, text2A, false, optimized, token);
		var diffsb = Compute(text1B, text2B, false, optimized, token);
		return diffsa.Concat(diffsb);
	}
}