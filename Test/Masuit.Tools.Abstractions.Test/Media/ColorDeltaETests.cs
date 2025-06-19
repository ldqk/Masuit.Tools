using Masuit.Tools.Media;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Media;

public class ColorDeltaETests
{
    [Fact]
    public void CIE1976_LabColor_SameColor_ReturnsZero()
    {
        var lab1 = new LabColor(50, 20, 30);
        var lab2 = new LabColor(50, 20, 30);

        double deltaE = ColorDeltaE.CIE1976(lab1, lab2);

        Assert.Equal(0, deltaE, 6);
    }

    [Fact]
    public void CIE1976_LabColor_DifferentColor_ReturnsPositive()
    {
        var lab1 = new LabColor(50, 20, 30);
        var lab2 = new LabColor(60, 25, 35);

        double deltaE = ColorDeltaE.CIE1976(lab1, lab2);

        Assert.True(deltaE > 0);
    }

    [Fact]
    public void CIE1994_LabColor_SameColor_ReturnsZero()
    {
        var lab1 = new LabColor(50, 20, 30);
        var lab2 = new LabColor(50, 20, 30);

        double deltaE = ColorDeltaE.CIE1994(lab1, lab2);

        Assert.Equal(0, deltaE, 6);
    }

    [Fact]
    public void CIE1994_LabColor_DifferentColor_ReturnsPositive()
    {
        var lab1 = new LabColor(50, 20, 30);
        var lab2 = new LabColor(60, 25, 35);

        double deltaE = ColorDeltaE.CIE1994(lab1, lab2);

        Assert.True(deltaE > 0);
    }

    [Fact]
    public void CIE1994_LabColor_TextileParameter()
    {
        var lab1 = new LabColor(50, 20, 30);
        var lab2 = new LabColor(60, 25, 35);

        double deltaE1 = ColorDeltaE.CIE1994(lab1, lab2, textile: false);
        double deltaE2 = ColorDeltaE.CIE1994(lab1, lab2, textile: true);

        Assert.NotEqual(deltaE1, deltaE2);
    }

    [Fact]
    public void CIE2000_LabColor_SameColor_ReturnsZero()
    {
        var lab1 = new LabColor(50, 20, 30);
        var lab2 = new LabColor(50, 20, 30);

        double deltaE = ColorDeltaE.CIE2000(lab1, lab2);

        Assert.Equal(0, deltaE, 6);
    }

    [Fact]
    public void CIE2000_LabColor_DifferentColor_ReturnsPositive()
    {
        var lab1 = new LabColor(50, 20, 30);
        var lab2 = new LabColor(60, 25, 35);

        double deltaE = ColorDeltaE.CIE2000(lab1, lab2);

        Assert.True(deltaE > 0);
    }

    [Fact]
    public void CMC_LabColor_SameColor_ReturnsZero()
    {
        var lab1 = new LabColor(50, 20, 30);
        var lab2 = new LabColor(50, 20, 30);

        double deltaE = ColorDeltaE.CMC(lab1, lab2);

        Assert.Equal(0, deltaE, 6);
    }

    [Fact]
    public void CMC_LabColor_DifferentColor_ReturnsPositive()
    {
        var lab1 = new LabColor(50, 20, 30);
        var lab2 = new LabColor(60, 25, 35);

        double deltaE = ColorDeltaE.CMC(lab1, lab2);

        Assert.True(deltaE > 0);
    }

    // 可选：为其它颜色空间（如CMYColor、CMYKColor、HSLColor等）补充类似测试
    // 这里只演示LabColor，因其它重载最终都转为LabColor处理
}