using BenchmarkDotNet.Attributes;
using Masuit.Tools.Media;

namespace BenchmarkTest;

[MemoryDiagnoser]
public class ImageHasherTest
{
    [Benchmark]
    public void AverageHash64()
    {
        var hasher = new ImageHasher();
        hasher.AverageHash64(@"F:\1.jpg");
    }

    [Benchmark]
    public void MedianHash256()
    {
        var hasher = new ImageHasher();
        hasher.MedianHash256(@"F:\1.jpg");
    }

    [Benchmark]
    public void MedianHash64()
    {
        var hasher = new ImageHasher();
        hasher.MedianHash64(@"F:\1.jpg");
    }

    [Benchmark]
    public void DifferenceHash64()
    {
        var hasher = new ImageHasher();
        hasher.DifferenceHash64(@"F:\1.jpg");
    }

    [Benchmark]
    public void DifferenceHash256()
    {
        var hasher = new ImageHasher();
        hasher.DifferenceHash256(@"F:\1.jpg");
    }

    [Benchmark]
    public void DctHash()
    {
        var hasher = new ImageHasher();
        hasher.DctHash(@"F:\1.jpg");
    }
}