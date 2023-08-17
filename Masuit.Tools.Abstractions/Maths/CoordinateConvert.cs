/*
* ## 当前互联网地图的坐标系现状
* ### 地球坐标(WGS84)
*  - 国际标准,从 GPS 设备中取出的数据的坐标系
*  - 国际地图提供商使用的坐标系

* ### 火星坐标(GCJ-02)也叫国测局坐标系
*  - 中国标准,从国行移动设备中定位获取的坐标数据使用这个坐标系
*  - 国家规定: 国内出版的各种地图系统(包括电子形式),必须至少采用GCJ-02对地理位置进行首次加密.
*
* ### 百度坐标(BD-09)
*  - 百度标准,百度 SDK,百度地图,GeoCoding 使用
*  -(本来就乱了,百度又在火星坐标上来个二次加密)
*
* ## 开发过程需要注意的事
*  - 从设备获取经纬度(GPS)坐标
*
* 如果使用的是百度sdk那么可以获得百度坐标(bd09)或者火星坐标(GCJ02),默认是bd09
* 如果使用的是ios的原生定位库,那么获得的坐标是WGS84
* 如果使用的是高德sdk,那么获取的坐标是GCJ02
*  - 互联网在线地图使用的坐标系
*
* 火星坐标系:
* iOS 地图(其实是高德)
* Google国内地图(.cn域名下)
* 搜搜、阿里云、高德地图、腾讯
* 百度坐标系:
* 当然只有百度地图
* WGS84坐标系:
* 国际标准,谷歌国外地图、osm地图等国外的地图一般都是这个
* 从JavaScript版本转到C#版本
* Created by Wander gis on 2015/7/8.
* 提供了百度坐标(BD09)、国测局坐标(火星坐标,GCJ02)、和WGS84坐标系之间的转换
* 修复了,中国范围的经纬度.
*/

using System;

namespace Masuit.Tools.Maths;

/// <summary>
/// UMD魔法代码,地图坐标转换偏移代码
/// </summary>
public static class CoordinateConvert
{
    private const double x_Pi = 3.14159265358979324 * 3000.0 / 180.0;
    private const double a = 6378245.0;
    private const double ee = 0.00669342162296594323;
    private const double Pi = 3.1415926535897932384626;

    /// <summary>
    /// WGS84转GCJ02
    /// </summary>
    /// <param name="wgsLon"></param>
    /// <param name="wgsLat"></param>
    /// <param name="gcjLon"></param>
    /// <param name="gcjLat"></param>
    public static void WGS84ToGCJ02(double wgsLon, double wgsLat, out double gcjLon, out double gcjLat)
    {
        if (OutOfChina(wgsLon, wgsLat))
        {
            gcjLon = wgsLon;
            gcjLat = wgsLat;
            return;
        }
        var dLat = TransformLat(wgsLon - 105.0, wgsLat - 35.0);
        var dLon = TransformLon(wgsLon - 105.0, wgsLat - 35.0);
        var radLat = wgsLat / 180.0 * Pi;
        var magic = Math.Sin(radLat);
        magic = 1 - magic * ee * magic;
        var sqrtMagic = Math.Sqrt(magic);
        dLat = dLat * 180.0 / (a * (1 - ee) / (magic * sqrtMagic) * Pi);
        dLon = dLon * 180.0 / (a / sqrtMagic * Math.Cos(radLat) * Pi);
        gcjLat = wgsLat + dLat;
        gcjLon = wgsLon + dLon;
    }

    /// <summary>
    /// GCJ02 转换为 WGS84
    /// </summary>
    /// <param name="gcjLon"></param>
    /// <param name="gcjLat"></param>
    /// <param name="wgsLon"></param>
    /// <param name="wgsLat"></param>
    public static void GCJ02ToWGS84(double gcjLon, double gcjLat, out double wgsLon, out double wgsLat)
    {
        if (OutOfChina(gcjLon, gcjLat))
        {
            wgsLon = gcjLon;
            wgsLat = gcjLat;
            return;
        }
        var dLat = TransformLat(gcjLon - 105.0, gcjLat - 35.0);
        var dLon = TransformLon(gcjLon - 105.0, gcjLat - 35.0);
        var radLat = gcjLat / 180.0 * Pi;
        var magic = Math.Sin(radLat);
        magic = 1 - magic * magic * ee;
        var sqrtMagic = Math.Sqrt(magic);
        dLat = dLat * 180.0 / (a * (1 - ee) / (magic * sqrtMagic) * Pi);
        dLon = dLon * 180.0 / (a / sqrtMagic * Math.Cos(radLat) * Pi);
        wgsLat = gcjLat - dLat;
        wgsLon = gcjLon - dLon;
    }

    /// <summary>
    /// 百度坐标系(BD-09) 与 火星坐标系(GCJ-02)的转换,即 百度 转 谷歌、高德
    /// </summary>
    /// <param name="bdLon"></param>
    /// <param name="bdLat"></param>
    /// <param name="gcjLon"></param>
    /// <param name="gcjLat"></param>
    public static void BD09ToGCJ02(double bdLon, double bdLat, out double gcjLon, out double gcjLat)
    {
        var x = bdLon - 0.0065;
        var y = bdLat - 0.006;
        var z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_Pi);
        var theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_Pi);
        gcjLon = z * Math.Cos(theta);
        gcjLat = z * Math.Sin(theta);
    }

    /// <summary>
    /// 火星坐标系(GCJ-02) 与百度坐标系(BD-09) 的转换,即谷歌、高德 转 百度
    /// </summary>
    /// <param name="gcjLon"></param>
    /// <param name="gcjLat"></param>
    /// <param name="bdLon"></param>
    /// <param name="bdLat"></param>
    public static void GCJ02ToBD09(double gcjLon, double gcjLat, out double bdLon, out double bdLat)
    {
        var z = Math.Sqrt(gcjLon * gcjLon + gcjLat * gcjLat) + 0.00002 * Math.Sin(gcjLat * x_Pi);
        var theta = Math.Atan2(gcjLat, gcjLon) + 0.000003 * Math.Cos(gcjLon * x_Pi);
        bdLon = Math.Cos(theta) * z + 0.0065;
        bdLat = Math.Sin(theta) * z + 0.006;
    }

    /// <summary>
    /// 判断是否在国内,不在国内则不做偏移
    /// </summary>
    /// <param name="lon"></param>
    /// <param name="lat"></param>
    /// <returns></returns>
    private static bool OutOfChina(double lon, double lat) =>

        // https://cn.bing.com/search?q=%E4%B8%AD%E5%9B%BD%E7%BB%8F%E7%BA%AC%E5%BA%A6%E8%8C%83%E5%9B%B4&qs=n&form=QBRE&sp=-1&pq=%E4%B8%AD%E5%9B%BD%E7%BB%8F%E7%BA%AC%E5%BA%A6%E8%8C%83%E5%9B%B4&sc=5-7&sk=&cvid=A687C2BEA56F4B08BE0913ADDA0C6674&ghsh=0&ghacc=0&ghpl=
        // 经度范围:73°33′E至135°05′E; 纬度范围:3°51′N至53°33′N
        lon is < 73.33 or > 135.05 || lat is < 3.51 or > 53.33;

    /// <summary>
    /// 转换纬度
    /// </summary>
    /// <param name="lon">经度</param>
    /// <param name="lat">纬度</param>
    /// <returns></returns>
    private static double TransformLat(double lon, double lat)
    {
        var ret = -100.0 + 2.0 * lon + 3.0 * lat + 0.2 * lat * lat + 0.1 * lon * lat + 0.2 * Math.Sqrt(Math.Abs(lon));
        ret += 20.0 * Math.Sin(6.0 * lon * Pi) + 20.0 * Math.Sin(2.0 * lon * Pi) * 2.0 / 3.0;
        ret += 20.0 * Math.Sin(lat * Pi) + 40.0 * Math.Sin(lat / 3.0 * Pi) * 2.0 / 3.0;
        ret += (160.0 * Math.Sin(lat / 12.0 * Pi) + 320 * Math.Sin(lat * Pi / 30.0) * 2.0) / 3.0;
        return ret;
    }

    /// <summary>
    /// 转换经度
    /// </summary>
    /// <param name="lon">经度</param>
    /// <param name="lat">纬度</param>
    /// <returns></returns>
    private static double TransformLon(double lon, double lat)
    {
        var ret = 300.0 + lon + 2.0 * lat + 0.1 * lon * lon + 0.1 * lon * lat + 0.1 * Math.Sqrt(Math.Abs(lon));
        ret += (20.0 * Math.Sin(6.0 * lon * Pi) + 20.0 * Math.Sin(2.0 * lon * Pi)) * 2.0 / 3.0;
        ret += (20.0 * Math.Sin(lon * Pi) + 40.0 * Math.Sin(lon / 3.0 * Pi)) * 2.0 / 3.0;
        ret += (150.0 * Math.Sin(lon / 12.0 * Pi) + 300.0 * Math.Sin(lon / 30.0 * Pi)) * 2.0 / 3.0;
        return ret;
    }
}
