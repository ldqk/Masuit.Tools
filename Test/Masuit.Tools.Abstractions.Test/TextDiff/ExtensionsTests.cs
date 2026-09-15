using System.Collections.Generic;
using System.Linq;
using Masuit.Tools.TextDiff;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.TextDiff;

public class ExtensionsTests
{
    [Fact]
    public void HtmlDiff_ShouldReturnHtmlDiff()
    {
        // Arrange
        var text1 = "<p>Hello</p>";
        var text2 = "<p>Hi</p>";

        // Act
        var (html1, html2) = text1.HtmlDiff(text2);

        // Assert
        Assert.Contains("<del>ello</del>", html1);
        Assert.Contains("<ins>i</ins>", html2);
    }

    [Theory]
    [InlineData("ABCADE", "ADCABE", "A<del>BCAD</del>E", "A<ins>DCAB</ins>E")]
    [InlineData("12345", "14325", "1<del>234</del>5", "1<ins>432</ins>5")]
    public void HtmlDiff_ShouldIncludeShortUnchangedTextInChanges(string text1, string text2, string expectedHtml1, string expectedHtml2)
    {
        var (html1, html2) = text1.HtmlDiff(text2, maxUnchangedLength: 2);

        Assert.Equal(expectedHtml1, html1);
        Assert.Equal(expectedHtml2, html2);
    }

    [Fact]
    public void HtmlDiffMerge_ShouldReturnMergedHtml()
    {
        // Arrange
        var text1 = "<p>Hello</p>";
        var text2 = "<p>Hi</p>";

        // Act
        var result = text1.HtmlDiffMerge(text2);

        // Assert
        Assert.Contains("<del>ello</del>", result);
        Assert.Contains("<ins>i</ins>", result);
    }
}