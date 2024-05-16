using BenchmarkDotNet.Attributes;
using Masuit.Tools.Systems;

namespace BenchmarkTest;

[MemoryDiagnoser]
public class StreamTest
{
    [Benchmark]
    public void MemoryStreamTest()
    {
        foreach (var file in new DirectoryInfo(@"D:\images\emotion\emoji").EnumerateFiles())
        {
            using var stream = file.OpenRead();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
        }
    }

    [Benchmark]
    public void PooledMemoryStreamTest()
    {
        foreach (var file in new DirectoryInfo(@"D:\images\emotion\emoji").EnumerateFiles())
        {
            using var stream = file.OpenRead();
            using var ms = new PooledMemoryStream();
            stream.CopyTo(ms);
        }
    }
}