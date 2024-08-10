namespace Masuit.Tools.TextDiff;

public readonly record struct PatchOption(float PatchDeleteThreshold, short PatchMargin)
{
	public static PatchOption Default { get; } = new(0.5f, 4);
}