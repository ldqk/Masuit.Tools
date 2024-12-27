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