namespace Masuit.Tools.TextDiff;

/// <summary>
///
/// </summary>
/// <param name="MatchThreshold">匹配阈值 (0.0 = 最好, 1.0 = 最差).</param>
/// <param name="MatchDistance"> 搜索匹配的距离（0=精确位置，1000以上=广泛匹配） (0.0 为完美匹配).
/// </param>
public readonly record struct MatchOption(float MatchThreshold, int MatchDistance)
{
	public static MatchOption Default { get; } = new(0.5f, 1000);
}