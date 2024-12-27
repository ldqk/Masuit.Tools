using System;
using Masuit.Tools.Strings;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Strings;

public class ValidateCodeTests
{
    [Fact]
    public void CreateValidateCode_ShouldReturnStringOfSpecifiedLength()
    {
        // Arrange
        int length = 6;

        // Act
        string result = ValidateCode.CreateValidateCode(length);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(length, result.Length);
    }

    [Fact]
    public void CreateValidateGraphic_ShouldReturnPooledMemoryStream()
    {
        // Arrange
        string validateCode = "ABC123";
        int fontSize = 28;

        // Act
        using var result = validateCode.CreateValidateGraphic(fontSize);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public void StringWidth_ShouldReturnCorrectWidth()
    {
        // Arrange
        string input = "Test";
        int fontSize = 12;

        // Act
        float width = input.StringWidth(fontSize);

        // Assert
        Assert.True(width > 0);
    }

    [Fact]
    public void StringWidth_WithFontName_ShouldReturnCorrectWidth()
    {
        // Arrange
        string input = "Test";
        string fontName = "Microsoft YaHei UI";
        int fontSize = 12;

        // Act
        float width = input.StringWidth(fontName, fontSize);

        // Assert
        Assert.True(width > 0);
    }

    [Fact]
    public void StringWidth_WithInvalidFontName_ShouldThrowArgumentException()
    {
        // Arrange
        string input = "Test";
        string fontName = "InvalidFont";
        int fontSize = 12;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => input.StringWidth(fontName, fontSize));
        Assert.Equal($"字体 {fontName} 不存在，请尝试其它字体！", exception.Message);
    }
}