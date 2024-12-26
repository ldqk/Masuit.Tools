using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions.BaseType;

public class IConvertibleExtensionsTest
{
    [Fact]
    public void Can_Convert()
    {
        Assert.True(typeof(double).IsNumeric());
        Assert.Equal("12.3".ConvertTo<decimal>(), 12.3m);
        Assert.Equal("12.3".ConvertTo(typeof(decimal?)), 12.3m);
        Assert.Equal("12.3".ChangeType(typeof(decimal?)), 12.3m);
    }
}