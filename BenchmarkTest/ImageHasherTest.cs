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
        hasher.AverageHash64(@"F:\guomo\1未精简\!已粗精简\lingyu69 - 性感法式白蕾丝情趣 大尺度漏奶漏鲍 [71P]\P.jpg");
    }

    [Benchmark]
    public void MedianHash256()
    {
        var hasher = new ImageHasher();
        hasher.MedianHash256(@"F:\guomo\1未精简\!已粗精简\lingyu69 - 性感法式白蕾丝情趣 大尺度漏奶漏鲍 [71P]\P.jpg");
    }

    [Benchmark]
    public void MedianHash64()
    {
        var hasher = new ImageHasher();
        hasher.MedianHash64(@"F:\guomo\1未精简\!已粗精简\lingyu69 - 性感法式白蕾丝情趣 大尺度漏奶漏鲍 [71P]\P.jpg");
    }

    [Benchmark]
    public void DifferenceHash64()
    {
        var hasher = new ImageHasher();
        hasher.DifferenceHash64(@"F:\guomo\1未精简\!已粗精简\lingyu69 - 性感法式白蕾丝情趣 大尺度漏奶漏鲍 [71P]\P.jpg");
    }

    [Benchmark]
    public void DifferenceHash256()
    {
        var hasher = new ImageHasher();
        hasher.DifferenceHash256(@"F:\guomo\1未精简\!已粗精简\lingyu69 - 性感法式白蕾丝情趣 大尺度漏奶漏鲍 [71P]\P.jpg");
    }

    [Benchmark]
    public void DctHash()
    {
        var hasher = new ImageHasher();
        hasher.DctHash(@"F:\guomo\1未精简\!已粗精简\lingyu69 - 性感法式白蕾丝情趣 大尺度漏奶漏鲍 [71P]\P.jpg");
    }
}