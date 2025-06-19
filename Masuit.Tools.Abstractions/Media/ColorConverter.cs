using System.Drawing;

namespace Masuit.Tools.Media;

internal static class ColorConverter
{
    // RGB转换器
    public static LabColor ToLab(this Color color)
    {
        // 第一步：将RGB转换为0-1范围
        double rLinear = color.R / 255.0;
        double gLinear = color.G / 255.0;
        double bLinear = color.B / 255.0;

        // 第二步：应用逆伽马校正
        rLinear = (rLinear <= 0.04045) ? rLinear / 12.92 : Math.Pow((rLinear + 0.055) / 1.055, 2.4);
        gLinear = (gLinear <= 0.04045) ? gLinear / 12.92 : Math.Pow((gLinear + 0.055) / 1.055, 2.4);
        bLinear = (bLinear <= 0.04045) ? bLinear / 12.92 : Math.Pow((bLinear + 0.055) / 1.055, 2.4);

        // 第三步：转换为XYZ（D65白点）
        double x = rLinear * 0.4124564 + gLinear * 0.3575761 + bLinear * 0.1804375;
        double y = rLinear * 0.2126729 + gLinear * 0.7151522 + bLinear * 0.0721750;
        double z = rLinear * 0.0193339 + gLinear * 0.1191920 + bLinear * 0.9503041;

        // 第四步：XYZ转Lab（使用D65参考白）
        const double xn = 0.95047;
        const double yn = 1.00000;
        const double zn = 1.08883;

        double xRatio = x / xn;
        double yRatio = y / yn;
        double zRatio = z / zn;

        double fx = (xRatio > 0.008856) ? Math.Pow(xRatio, 1.0 / 3.0) : (903.3 * xRatio + 16) / 116.0;
        double fy = (yRatio > 0.008856) ? Math.Pow(yRatio, 1.0 / 3.0) : (903.3 * yRatio + 16) / 116.0;
        double fz = (zRatio > 0.008856) ? Math.Pow(zRatio, 1.0 / 3.0) : (903.3 * zRatio + 16) / 116.0;

        double l = (yRatio > 0.008856) ? (116 * fy - 16) : (903.3 * yRatio);
        double a = 500 * (fx - fy);
        double bVal = 200 * (fy - fz);

        return new LabColor(l, a, bVal);
    }

    // CMY转换器
    public static LabColor ToLab(this CMYColor cmy)
    {
        // CMY转RGB (0-1范围)
        double r = 1 - cmy.C;
        double g = 1 - cmy.M;
        double b = 1 - cmy.Y;

        // RGB转Lab
        return ToLab(Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255)));
    }

    // CMYK转换器
    public static LabColor ToLab(this CMYKColor cmyk)
    {
        // CMYK转RGB
        double r = (1 - cmyk.C) * (1 - cmyk.K);
        double g = (1 - cmyk.M) * (1 - cmyk.K);
        double b = (1 - cmyk.Y) * (1 - cmyk.K);

        // RGB转Lab
        return ToLab(Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255)));
    }

    // HSL转换器
    public static LabColor ToLab(this HSLColor hsl)
    {
        // HSL转RGB
        Color rgb = ToRgb(hsl);
        return ToLab(rgb);
    }

    private static Color ToRgb(HSLColor hsl)
    {
        double h = hsl.H / 360.0;
        double s = hsl.S;
        double l = hsl.L;
        double r, g, b;
        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;

            r = HueToRgb(p, q, h + 1.0 / 3);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1.0 / 3);
        }

        return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;

        if (t < 1.0 / 6) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2) return q;
        if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;

        return p;
    }

    // LCH转换器
    public static LabColor ToLab(this LCHColor lch)
    {
        // LCH转Lab
        double rad = lch.H * Math.PI / 180.0;
        double a = lch.C * Math.Cos(rad);
        double b = lch.C * Math.Sin(rad);
        return new LabColor(lch.L, a, b);
    }

    // XYZ转换器
    public static LabColor ToLab(this XYZColor xyz)
    {
        // 使用D65参考白点
        const double xn = 0.95047;
        const double yn = 1.00000;
        const double zn = 1.08883;

        double xRatio = xyz.X / xn;
        double yRatio = xyz.Y / yn;
        double zRatio = xyz.Z / zn;

        double fx = Fxyz(xRatio);
        double fy = Fxyz(yRatio);
        double fz = Fxyz(zRatio);

        double L = 116 * fy - 16;
        double a = 500 * (fx - fy);
        double b = 200 * (fy - fz);

        return new LabColor(L, a, b);
    }

    private static double Fxyz(double t)
    {
        const double delta = 6.0 / 29.0;
        const double delta3 = delta * delta * delta;
        return (t > delta3) ? Math.Pow(t, 1.0 / 3.0) : t / (3 * delta * delta) + 4.0 / 29.0;
    }

    // YXZ转换器 (假设YXZ是Y,X,Z顺序)
    public static LabColor YxzToLab(this YXZColor yxz)
    {
        // 转换为标准XYZ顺序
        return ToLab(new XYZColor(yxz.X, yxz.Y, yxz.Z));
    }
}