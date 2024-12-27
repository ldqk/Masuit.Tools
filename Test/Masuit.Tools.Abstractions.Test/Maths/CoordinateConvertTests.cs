using Masuit.Tools.Maths;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Maths;

public class CoordinateConvertTests
{
    [Theory]
    [InlineData(116.403981, 39.915292, 116.39762729119315, 39.9089)]
    [InlineData(120.153576, 30.287459, 120.1471, 30.2814)]
    public void BD09ToGCJ02_ShouldConvertCorrectly(double bdLon, double bdLat, double expectedGcjLon, double expectedGcjLat)
    {
        CoordinateConvert.BD09ToGCJ02(bdLon, bdLat, out double gcjLon, out double gcjLat);
        Assert.Equal(expectedGcjLon, gcjLon, 4);
        Assert.Equal(expectedGcjLat, gcjLat, 4);
    }

    [Theory]
    [InlineData(116.39762729119315, 39.91974545536095, 116.403981, 39.9261)]
    [InlineData(120.147748, 30.2866, 120.1542, 30.29259)]
    public void GCJ02ToBD09_ShouldConvertCorrectly(double gcjLon, double gcjLat, double expectedBdLon, double expectedBdLat)
    {
        CoordinateConvert.GCJ02ToBD09(gcjLon, gcjLat, out double bdLon, out double bdLat);
        Assert.Equal(expectedBdLon, bdLon, 4);
        Assert.Equal(expectedBdLat, bdLat, 4);
    }
}