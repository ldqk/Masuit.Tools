using System.Collections.Immutable;

namespace Masuit.Tools.TextDiff;

public readonly record struct TextDiffer(DiffOperation Operation, string Text)
{
	public static TextDiffer Empty => new(DiffOperation.Equal, string.Empty);
	public bool IsEmpty => Text.Length == 0;
	public static TextDiffer Equal(ReadOnlySpan<char> text) => Create(DiffOperation.Equal, text.ToString());
	public static TextDiffer Insert(ReadOnlySpan<char> text) => Create(DiffOperation.Insert, text.ToString());
	public static TextDiffer Delete(ReadOnlySpan<char> text) => Create(DiffOperation.Delete, text.ToString());
	internal static TextDiffer Create(DiffOperation operation, string text) => new(operation, text);

	internal TextDiffer Replace(string text) => this with { Text = text };
	internal TextDiffer Append(string text) => this with { Text = Text + text };
	internal TextDiffer Prepend(string text) => this with { Text = text + Text };

	/// <summary>
	/// 比较两段文本并生成差异信息
	/// </summary>
	/// <param name="text1"></param>
	/// <param name="text2"></param>
	/// <param name="timeoutInSeconds">比较超时时间，单位：秒，0表示不超时</param>
	/// <param name="checklines">如果为false，则不要先运行行级差异来识别更改的区域。如果为true，则运行一个速度稍快但不太理想的差异</param>
	/// <returns></returns>
	public static ImmutableList<TextDiffer> Compute(string text1, string text2, float timeoutInSeconds = 0f, bool checklines = true)
	{
		using var cts = timeoutInSeconds <= 0
			? new CancellationTokenSource()
			: new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));
		return Compute(text1, text2, checklines, timeoutInSeconds > 0, cts.Token);
	}

	public static ImmutableList<TextDiffer> Compute(string text1, string text2, bool checkLines, bool optimizeForSpeed, CancellationToken token)
		=> DiffAlgorithm.Compute(text1, text2, checkLines, optimizeForSpeed, token).ToImmutableList();

	public bool IsLargeDelete(int size) => Operation == DiffOperation.Delete && Text.Length > size;

	public override string ToString()
	{
		var prettyText = Text.Replace('\n', '\u00b6');
		return "Diff(" + Operation + ",\"" + prettyText + "\")";
	}
}