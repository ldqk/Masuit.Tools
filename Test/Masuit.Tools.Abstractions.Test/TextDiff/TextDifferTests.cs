using System;
using Masuit.Tools.TextDiff;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.TextDiff;

public class TextDifferTests
{
    [Fact]
    public void Empty_ShouldReturnEmptyTextDiffer()
    {
        var empty = TextDiffer.Empty;
        Assert.Equal(DiffOperation.Equal, empty.Operation);
        Assert.Equal(string.Empty, empty.Text);
    }

    [Fact]
    public void IsEmpty_ShouldReturnTrueForEmptyText()
    {
        var empty = TextDiffer.Empty;
        Assert.True(empty.IsEmpty);
    }

    [Fact]
    public void Equal_ShouldCreateEqualTextDiffer()
    {
        var text = "test";
        var differ = TextDiffer.Equal(text.AsSpan());
        Assert.Equal(DiffOperation.Equal, differ.Operation);
        Assert.Equal(text, differ.Text);
    }

    [Fact]
    public void Insert_ShouldCreateInsertTextDiffer()
    {
        var text = "test";
        var differ = TextDiffer.Insert(text.AsSpan());
        Assert.Equal(DiffOperation.Insert, differ.Operation);
        Assert.Equal(text, differ.Text);
    }

    [Fact]
    public void Delete_ShouldCreateDeleteTextDiffer()
    {
        var text = "test";
        var differ = TextDiffer.Delete(text.AsSpan());
        Assert.Equal(DiffOperation.Delete, differ.Operation);
        Assert.Equal(text, differ.Text);
    }

    [Fact]
    public void Compute_ShouldReturnDiffs()
    {
        var text1 = "test";
        var text2 = "test123";
        var diffs = TextDiffer.Compute(text1, text2);
        Assert.NotEmpty(diffs);
    }

    [Fact]
    public void IsLargeDelete_ShouldReturnTrueForLargeDelete()
    {
        var differ = TextDiffer.Delete("large delete".AsSpan());
        Assert.True(differ.IsLargeDelete(5));
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var differ = TextDiffer.Equal("test\n".AsSpan());
        var result = differ.ToString();
        Assert.Equal("Diff(Equal,\"test\u00b6\")", result);
    }
}