using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<TestClass>();

[MemoryDiagnoser]
public class TestClass
{
}
