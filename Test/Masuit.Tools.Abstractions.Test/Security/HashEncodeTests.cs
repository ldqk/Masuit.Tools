using System;
using Masuit.Tools.Security;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Security;

public class HashEncodeTests
{
    [Fact]
    public void GetSecurity_ShouldNotDependOnTheProvidedRandomSeed()
    {
        string first = new Random(42).GetSecurity();
        string second = new Random(42).GetSecurity();

        Assert.NotEqual(first, second);
    }
}
