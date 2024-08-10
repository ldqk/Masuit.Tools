using System.Text;

namespace Masuit.Tools.TextDiff;

internal class StringCompressor
{
	private readonly List<string> _lineArray = [];
	private readonly Dictionary<string, char> _lineHash = new();

	private char this[string line] => _lineHash[line];

	private string this[int c] => _lineArray[c];

	/// <summary>
	/// 将文本的所有行压缩为一系列索引 (starting at \u0001, ending at (char)text.Length)
	/// </summary>
	/// <param name="text"></param>
	/// <param name="maxLines"></param>
	/// <returns></returns>
	public string Compress(ReadOnlySpan<char> text, int maxLines = char.MaxValue) => Encode(text, maxLines);

	public string Decompress(string text) => text.Aggregate(new StringBuilder(), (sb, c) => sb.Append(this[c])).Append(text.Length == char.MaxValue ? this[char.MaxValue] : "").ToString();

	private string Encode(ReadOnlySpan<char> text, int maxLines)
	{
		var sb = new StringBuilder();
		var start = 0;
		var end = -1;
		while (end < text.Length - 1)
		{
			var i = text[start..].IndexOf('\n');
			end = _lineArray.Count == maxLines || i == -1 ? text.Length - 1 : i + start;
			var line = text[start..(end + 1)].ToString();
			EnsureHashed(line);
			sb.Append(this[line]);
			start = end + 1;
		}

		return sb.ToString();
	}

	// e.g. _lineArray[4] == "Hello\n"
	// e.g. _lineHash["Hello\n"] == 4

	private void EnsureHashed(string line)
	{
		if (_lineHash.ContainsKey(line)) return;
		_lineArray.Add(line);
		_lineHash.Add(line, (char)(_lineArray.Count - 1));
	}
}