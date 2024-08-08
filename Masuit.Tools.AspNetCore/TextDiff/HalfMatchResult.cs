namespace Masuit.Tools.TextDiff;

internal readonly record struct HalfMatchResult(string Prefix1, string Suffix1, string Prefix2, string Suffix2, string CommonMiddle)
{
    public bool IsEmpty => string.IsNullOrEmpty(CommonMiddle);

    public static readonly HalfMatchResult Empty = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    public static bool operator >(HalfMatchResult left, HalfMatchResult right) => left.CommonMiddle.Length > right.CommonMiddle.Length;

    public static bool operator <(HalfMatchResult left, HalfMatchResult right) => left.CommonMiddle.Length < right.CommonMiddle.Length;
    public static HalfMatchResult operator -(HalfMatchResult item) => new(item.Prefix2, item.Suffix2, item.Prefix1, item.Suffix1, item.CommonMiddle);
    public override string ToString() => $"[{Prefix1}/{Prefix2}] - {CommonMiddle} - [{Suffix1}/{Suffix2}]";
}
