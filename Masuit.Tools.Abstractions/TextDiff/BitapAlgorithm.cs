namespace Masuit.Tools.TextDiff;

/*
 * https://en.wikipedia.org/wiki/Bitap_algorithm
 */

/// <summary>
/// 实现Bitap算法，允许近似字符串匹配的文本搜索算法。提供了在文本字符串中定位给定模式的最佳实例的功能，考虑了潜在的不匹配和错误。该算法通过MatchSettings配置，其中包括匹配阈值和距离，确定搜索的灵敏度和灵活性。
/// </summary>
internal class BitapAlgorithm(MatchOption option)
{
	// 匹配阈值 (0.0 = 最完美, 1.0 = 非常宽松).
	private readonly float _matchThreshold = option.MatchThreshold;

	// 搜索匹配的距离（0=精确位置，1000以上=广泛匹配）。与预期位置相差多少字符的匹配将分数增加1.0（0.0是完美匹配）。
	private readonly int _matchDistance = option.MatchDistance;

	/// <summary>
	/// 使用Bitap算法在“loc”附近的“text”中找到“pattern”的最佳实例。如果未找到匹配项，则返回-1。
	/// </summary>
	/// <param name="text">需要搜索的文本</param>
	/// <param name="pattern">被搜索片段</param>
	/// <param name="startIndex">搜索位置</param>
	/// <returns>最匹配的位置或 -1.</returns>
	public int Match(string text, string pattern, int startIndex)
	{
		double scoreThreshold = _matchThreshold;

		var bestMatchIndex = text.IndexOf(pattern, startIndex, StringComparison.Ordinal);
		if (bestMatchIndex != -1)
		{
			scoreThreshold = Math.Min(MatchBitapScore(0, bestMatchIndex, startIndex, pattern), scoreThreshold);
			bestMatchIndex = text.LastIndexOf(pattern, Math.Min(startIndex + pattern.Length, text.Length), StringComparison.Ordinal);
			if (bestMatchIndex != -1)
			{
				scoreThreshold = Math.Min(MatchBitapScore(0, bestMatchIndex, startIndex, pattern), scoreThreshold);
			}
		}

		var s = InitAlphabet(pattern);
		var matchmask = 1 << (pattern.Length - 1);
		bestMatchIndex = -1;
		var currentMaxRange = pattern.Length + text.Length;
		var lastComputedRow = Array.Empty<int>();
		for (var d = 0; d < pattern.Length; d++)
		{
			var currentMinRange = 0;
			var currentMidpoint = currentMaxRange;
			while (currentMinRange < currentMidpoint)
			{
				if (MatchBitapScore(d, startIndex + currentMidpoint, startIndex, pattern) <= scoreThreshold)
					currentMinRange = currentMidpoint;
				else
					currentMaxRange = currentMidpoint;
				currentMidpoint = (currentMaxRange - currentMinRange) / 2 + currentMinRange;
			}

			currentMaxRange = currentMidpoint;
			var start = Math.Max(1, startIndex - currentMidpoint + 1);
			var finish = Math.Min(startIndex + currentMidpoint, text.Length) + pattern.Length;
			var rd = new int[finish + 2];
			rd[finish + 1] = (1 << d) - 1;
			for (var j = finish; j >= start; j--)
			{
				int charMatch;
				if (text.Length <= j - 1 || !s.ContainsKey(text[j - 1]))
				{
					charMatch = 0;
				}
				else
				{
					charMatch = s[text[j - 1]];
				}

				if (d == 0)
				{
					rd[j] = ((rd[j + 1] << 1) | 1) & charMatch;
				}
				else
				{
					rd[j] = ((rd[j + 1] << 1) | 1) & charMatch | ((lastComputedRow[j + 1] | lastComputedRow[j]) << 1) | 1 | lastComputedRow[j + 1];
				}

				if ((rd[j] & matchmask) != 0)
				{
					var score = MatchBitapScore(d, j - 1, startIndex, pattern);
					if (score <= scoreThreshold)
					{
						scoreThreshold = score;
						bestMatchIndex = j - 1;
						if (bestMatchIndex > startIndex)
						{
							start = Math.Max(1, 2 * startIndex - bestMatchIndex);
						}
						else
						{
							break;
						}
					}
				}
			}

			if (MatchBitapScore(d + 1, startIndex, startIndex, pattern) > scoreThreshold)
			{
				break;
			}

			lastComputedRow = rd;
		}

		return bestMatchIndex;
	}

	/// <summary>
	/// 初始化Bitap算法的字母表
	/// </summary>
	/// <param name="pattern"></param>
	/// <returns></returns>
	internal static Dictionary<char, int> InitAlphabet(string pattern) => pattern.Select((c, i) => (c, i)).Aggregate(new Dictionary<char, int>(), (d, x) =>
	{
		d.TryGetValue(x.c, out var value);
		d[x.c] = value | (1 << (pattern.Length - x.i - 1));
		return d;
	});

	/// <summary>
	/// 计算并匹配得分
	/// </summary>
	/// <param name="errors">匹配中的错误数</param>
	/// <param name="location">匹配位置.</param>
	/// <param name="expectedLocation">预期位置</param>
	/// <param name="pattern">查找片段</param>
	/// <returns>匹配得分 (0.0 = 最好, 1.0 = 最差).</returns>
	private double MatchBitapScore(int errors, int location, int expectedLocation, string pattern)
	{
		var accuracy = (float)errors / pattern.Length;
		var proximity = Math.Abs(expectedLocation - location);
		return (_matchDistance, proximity) switch
		{
			(0, 0) => accuracy,
			(0, _) => 1.0,
			_ => accuracy + proximity / (float)_matchDistance
		};
	}
}