using Masuit.Tools.Strings;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Strings;

public class NumberFormaterTest
{
    [Fact]
    public void Can_ToBase36()
    {
        // arrange
        var formater = new NumberFormater(36);

        // act
        var s = formater.ToString(12345678);

        // assert
        Assert.Equal(s, "7clzi");
    }

    [Fact]
    public void Can_FromBase36()
    {
        // arrange
        var formater = new NumberFormater(36);

        // act
        var num = formater.FromString("7clzi");

        // assert
        Assert.Equal(num, 12345678);
    }
}
