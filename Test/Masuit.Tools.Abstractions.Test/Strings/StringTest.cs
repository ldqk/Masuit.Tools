using Masuit.Tools.Strings;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Strings;

public class StringTest
{
    [Fact]
    public void Can_MeasureString()
    {
        // arrange
        const string s = "1a啊🥳∰㍰ⅷ㍿👩‍❤️‍💋‍👩";

        // act
        var width = s.StringWidth();
        var charCount = s.CharacterCount();
        var bytesCount = s.BytesCount();
        var matchEmoji = s.MatchEmoji();

        // assert
        Assert.Equal(width, 9);
        Assert.Equal(charCount, 9);
        Assert.Equal(bytesCount, 48);
        Assert.True(matchEmoji);
    }

    [Fact]
    public void Can_Mask()
    {
        // arrange
        const string s = "13123456789";

        // act
        var mask = s.Mask();

        // assert
        Assert.Equal(mask, "131****6789");
    }

    [Theory]
    [InlineData("11@1.cn", "1****@1.cn")]
    [InlineData("admin@masuit.com", "a****@masuit.com")]
    public void Can_MaskEmail(string input, string expect)
    {
        // act
        var mask = input.MaskEmail();

        // assert
        Assert.Equal(mask, expect);
    }
}