using System.Linq;
using Masuit.Tools.Html;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Html;

public class HtmlToolsTests
{
    [Fact]
    public void HtmlSanitizerStandard_ShouldSanitizeHtml()
    {
        var html = "<div><script>alert('xss');</script><p>Test</p></div>";
        var sanitizedHtml = html.HtmlSanitizerStandard();
        Assert.DoesNotContain("<script>", sanitizedHtml);
        Assert.Contains("<p>Test</p>", sanitizedHtml);
    }

    [Fact]
    public void HtmlSanitizerCustom_ShouldSanitizeHtmlWithCustomSettings()
    {
        var html = "<div><script>alert('xss');</script><p>Test</p></div>";
        var sanitizedHtml = html.HtmlSanitizerCustom(new[] { "script" });
        Assert.DoesNotContain("<script>", sanitizedHtml);
        Assert.Contains("<p>Test</p>", sanitizedHtml);
    }

    [Fact]
    public void RemoveHtmlTag_ShouldRemoveHtmlTags()
    {
        var html = "<div><p>Test</p></div>";
        var result = html.RemoveHtmlTag();
        Assert.Equal("Test", result);
    }

    [Fact]
    public void RemoveHtmlTag_ShouldRemoveHtmlTagsAndTruncate()
    {
        var html = "<div><p>Test</p></div>";
        var result = html.RemoveHtmlTag(2);
        Assert.Equal("Te", result);
    }

    [Fact]
    public void ReplaceHtmlImgSource_ShouldReplaceImgSrc()
    {
        var html = "<img src=\"image.jpg\">";
        var result = html.ReplaceHtmlImgSource("http://example.com");
        Assert.Equal("<img src=\"http://example.com/image.jpg\">", result);
    }

    [Fact]
    public void ConvertImgSrcToRelativePath_ShouldConvertToRelativePath()
    {
        var html = "<img src=\"http://example.com/image.jpg\">";
        var result = html.ConvertImgSrcToRelativePath();
        Assert.Equal("<img src=\"/image.jpg\">", result);
    }

    [Fact]
    public void MatchImgTags_ShouldReturnImgTags()
    {
        var html = "<div><img src=\"image1.jpg\"><img src=\"image2.jpg\"></div>";
        var result = html.MatchImgTags();
        Assert.Equal(2, result.Length);
    }

    [Fact]
    public void MatchImgSrcs_ShouldReturnImgSrcs()
    {
        var html = "<div><img src=\"image1.jpg\"><img src=\"image2.jpg\"></div>";
        var result = html.MatchImgSrcs();
        Assert.Equal(2, result.Count());
        Assert.Contains("image1.jpg", result);
        Assert.Contains("image2.jpg", result);
    }

    [Fact]
    public void MatchFirstImgSrc_ShouldReturnFirstImgSrc()
    {
        var html = "<div><img src=\"image1.jpg\"><img src=\"image2.jpg\"></div>";
        var result = html.MatchFirstImgSrc();
        Assert.Equal("image1.jpg", result);
    }

    [Fact]
    public void MatchRandomImgSrc_ShouldReturnRandomImgSrc()
    {
        var html = "<div><img src=\"image1.jpg\"><img src=\"image2.jpg\"></div>";
        var result = html.MatchRandomImgSrc();
        Assert.Contains(result, new[] { "image1.jpg", "image2.jpg" });
    }

    [Fact]
    public void EncodeHtml_ShouldEncodeHtml()
    {
        var html = "Hello, World!";
        var result = html.EncodeHtml();
        Assert.Equal("Hello&def World!", result);
    }
}