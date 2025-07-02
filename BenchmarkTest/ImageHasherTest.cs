using BenchmarkDotNet.Attributes;
using Masuit.Tools.Media;

namespace BenchmarkTest;

[MemoryDiagnoser]
public class ImageHasherTest
{
    private const string Path = @"F:\1.jpg";

    [Benchmark]
    public void AverageHash64()
    {
        var hasher = new ImageHasher();
        hasher.AverageHash64(Path);
    }

    [Benchmark]
    public void MedianHash256()
    {
        var hasher = new ImageHasher();
        hasher.MedianHash256(Path);
    }

    [Benchmark]
    public void MedianHash64()
    {
        var hasher = new ImageHasher();
        hasher.MedianHash64(Path);
    }

    [Benchmark]
    public void DifferenceHash64()
    {
        var hasher = new ImageHasher();
        hasher.DifferenceHash64(Path);
    }

    [Benchmark]
    public void DifferenceHash256()
    {
        var hasher = new ImageHasher();
        hasher.DifferenceHash256(Path);
    }

    [Benchmark]
    public void DctHash()
    {
        var hasher = new ImageHasher();
        hasher.DctHash(Path);
    }
}