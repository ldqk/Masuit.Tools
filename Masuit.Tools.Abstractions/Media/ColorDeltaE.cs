using System.Drawing;

namespace Masuit.Tools.Media;

public static class ColorDeltaE
{
    // CIE1976 (ΔE*ab)
    public static double CIE1976(this Color color1, Color color2) => CalculateDeltaE1976(color1.ToLab(), color2.ToLab());

    public static double CIE1976(this CMYColor color1, CMYColor color2) => CalculateDeltaE1976(color1.ToLab(), color2.ToLab());

    public static double CIE1976(this CMYKColor color1, CMYKColor color2) => CalculateDeltaE1976(color1.ToLab(), color2.ToLab());

    public static double CIE1976(this HSLColor color1, HSLColor color2) => CalculateDeltaE1976(color1.ToLab(), color2.ToLab());

    public static double CIE1976(this LabColor color1, LabColor color2) => CalculateDeltaE1976(color1, color2);

    public static double CIE1976(this LCHColor color1, LCHColor color2) => CalculateDeltaE1976(color1.ToLab(), color2.ToLab());

    public static double CIE1976(this XYZColor color1, XYZColor color2) => CalculateDeltaE1976(color1.ToLab(), color2.ToLab());

    public static double CIE1976(this YXZColor color1, YXZColor color2) => CalculateDeltaE1976(color1.YxzToLab(), color2.YxzToLab());

    // CIE1994 (ΔE94)
    public static double CIE1994(this Color color1, Color color2, bool textile = false) => CalculateDeltaE1994(color1.ToLab(), color2.ToLab(), textile);

    public static double CIE1994(this CMYColor color1, CMYColor color2, bool textile = false) => CalculateDeltaE1994(color1.ToLab(), color2.ToLab(), textile);

    public static double CIE1994(this CMYKColor color1, CMYKColor color2, bool textile = false) => CalculateDeltaE1994(color1.ToLab(), color2.ToLab(), textile);

    public static double CIE1994(this HSLColor color1, HSLColor color2, bool textile = false) => CalculateDeltaE1994(color1.ToLab(), color2.ToLab(), textile);

    public static double CIE1994(this LabColor color1, LabColor color2, bool textile = false) => CalculateDeltaE1994(color1, color2, textile);

    public static double CIE1994(this LCHColor color1, LCHColor color2, bool textile = false) => CalculateDeltaE1994(color1.ToLab(), color2.ToLab(), textile);

    public static double CIE1994(this XYZColor color1, XYZColor color2, bool textile = false) => CalculateDeltaE1994(color1.ToLab(), color2.ToLab(), textile);

    public static double CIE1994(this YXZColor color1, YXZColor color2, bool textile = false) => CalculateDeltaE1994(color1.YxzToLab(), color2.YxzToLab(), textile);

    // CIE2000 (ΔE00)
    public static double CIE2000(this Color color1, Color color2) => CalculateDeltaE2000(color1.ToLab(), color2.ToLab());

    public static double CIE2000(this CMYColor color1, CMYColor color2) => CalculateDeltaE2000(color1.ToLab(), color2.ToLab());

    public static double CIE2000(this CMYKColor color1, CMYKColor color2) => CalculateDeltaE2000(color1.ToLab(), color2.ToLab());

    public static double CIE2000(this HSLColor color1, HSLColor color2) => CalculateDeltaE2000(color1.ToLab(), color2.ToLab());

    public static double CIE2000(this LabColor color1, LabColor color2) => CalculateDeltaE2000(color1, color2);

    public static double CIE2000(this LCHColor color1, LCHColor color2) => CalculateDeltaE2000(color1.ToLab(), color2.ToLab());

    public static double CIE2000(this XYZColor color1, XYZColor color2) => CalculateDeltaE2000(color1.ToLab(), color2.ToLab());

    public static double CIE2000(this YXZColor color1, YXZColor color2) => CalculateDeltaE2000(color1.YxzToLab(), color2.YxzToLab());

    // CMC (ΔEcmc)
    public static double CMC(this Color color1, Color color2, double l = 2.0, double c = 1.0) => CalculateDeltaECMC(color1.ToLab(), color2.ToLab(), l, c);

    public static double CMC(this CMYColor color1, CMYColor color2, double l = 2.0, double c = 1.0) => CalculateDeltaECMC(color1.ToLab(), color2.ToLab(), l, c);

    public static double CMC(this CMYKColor color1, CMYKColor color2, double l = 2.0, double c = 1.0) => CalculateDeltaECMC(color1.ToLab(), color2.ToLab(), l, c);

    public static double CMC(this HSLColor color1, HSLColor color2, double l = 2.0, double c = 1.0) => CalculateDeltaECMC(color1.ToLab(), color2.ToLab(), l, c);

    public static double CMC(this LabColor color1, LabColor color2, double l = 2.0, double c = 1.0) => CalculateDeltaECMC(color1, color2, l, c);

    public static double CMC(this LCHColor color1, LCHColor color2, double l = 2.0, double c = 1.0) => CalculateDeltaECMC(color1.ToLab(), color2.ToLab(), l, c);

    public static double CMC(this XYZColor color1, XYZColor color2, double l = 2.0, double c = 1.0) => CalculateDeltaECMC(color1.ToLab(), color2.ToLab(), l, c);

    public static double CMC(this YXZColor color1, YXZColor color2, double l = 2.0, double c = 1.0) => CalculateDeltaECMC(color1.YxzToLab(), color2.YxzToLab(), l, c);

    #region Core Calculation Methods

    private static double CalculateDeltaE1976(LabColor lab1, LabColor lab2)
    {
        double deltaL = lab1.L - lab2.L;
        double deltaA = lab1.a - lab2.a;
        double deltaB = lab1.b - lab2.b;
        return Math.Sqrt(deltaL * deltaL + deltaA * deltaA + deltaB * deltaB);
    }

    private static double CalculateDeltaE1994(LabColor lab1, LabColor lab2, bool textile)
    {
        double kL = textile ? 2.0 : 1.0;
        double kC = 1.0;
        double kH = 1.0;
        double k1 = textile ? 0.048 : 0.045;
        double k2 = textile ? 0.014 : 0.015;

        double deltaL = lab1.L - lab2.L;
        double c1 = Math.Sqrt(lab1.a * lab1.a + lab1.b * lab1.b);
        double c2 = Math.Sqrt(lab2.a * lab2.a + lab2.b * lab2.b);
        double deltaC = c1 - c2;

        double deltaA = lab1.a - lab2.a;
        double deltaB = lab1.b - lab2.b;
        double deltaH = Math.Sqrt(Math.Max(0, deltaA * deltaA + deltaB * deltaB - deltaC * deltaC));

        double sl = 1.0;
        double sc = 1.0 + k1 * c1;
        double sh = 1.0 + k2 * c1;

        double termL = deltaL / (kL * sl);
        double termC = deltaC / (kC * sc);
        double termH = deltaH / (kH * sh);

        return Math.Sqrt(termL * termL + termC * termC + termH * termH);
    }

    private static double CalculateDeltaE2000(LabColor lab1, LabColor lab2)
    {
        const double kL = 1.0;
        const double kC = 1.0;
        const double kH = 1.0;

        double l1 = lab1.L, a1 = lab1.a, b1 = lab1.b;
        double l2 = lab2.L, a2 = lab2.a, b2 = lab2.b;

        double c1 = Math.Sqrt(a1 * a1 + b1 * b1);
        double c2 = Math.Sqrt(a2 * a2 + b2 * b2);
        double meanC = (c1 + c2) / 2.0;

        double meanC7 = Math.Pow(meanC, 7);
        double g = 0.5 * (1 - Math.Sqrt(meanC7 / (meanC7 + 6103515625.0)));

        double a1Prime = a1 * (1 + g);
        double a2Prime = a2 * (1 + g);

        double c1Prime = Math.Sqrt(a1Prime * a1Prime + b1 * b1);
        double c2Prime = Math.Sqrt(a2Prime * a2Prime + b2 * b2);

        double h1Prime = (Math.Atan2(b1, a1Prime) * 180.0 / Math.PI + 360) % 360;
        double h2Prime = (Math.Atan2(b2, a2Prime) * 180.0 / Math.PI + 360) % 360;

        double deltaLPrime = l2 - l1;
        double deltaCPrime = c2Prime - c1Prime;

        double deltaHPrime;
        if (Math.Abs(h2Prime - h1Prime) <= 180)
        {
            deltaHPrime = h2Prime - h1Prime;
        }
        else if (h2Prime <= h1Prime)
        {
            deltaHPrime = h2Prime - h1Prime + 360;
        }
        else
        {
            deltaHPrime = h2Prime - h1Prime - 360;
        }

        double deltaH = 2 * Math.Sqrt(c1Prime * c2Prime) * Math.Sin(deltaHPrime * Math.PI / 360.0);
        double meanLPrime = (l1 + l2) / 2.0;
        double meanCPrime = (c1Prime + c2Prime) / 2.0;
        double meanHPrime;
        if (Math.Abs(h1Prime - h2Prime) > 180)
        {
            meanHPrime = (h1Prime + h2Prime + 360) / 2.0;
        }
        else
        {
            meanHPrime = (h1Prime + h2Prime) / 2.0;
        }

        meanHPrime %= 360;

        double t = 1 - 0.17 * Math.Cos((meanHPrime - 30) * Math.PI / 180) + 0.24 * Math.Cos(2 * meanHPrime * Math.PI / 180) + 0.32 * Math.Cos((3 * meanHPrime + 6) * Math.PI / 180) - 0.20 * Math.Cos((4 * meanHPrime - 63) * Math.PI / 180);

        double sl = 1 + (0.015 * Math.Pow(meanLPrime - 50, 2)) / Math.Sqrt(20 + Math.Pow(meanLPrime - 50, 2));
        double sc = 1 + 0.045 * meanCPrime;
        double sh = 1 + 0.015 * meanCPrime * t;

        double meanCPrime7 = Math.Pow(meanCPrime, 7);
        double rc = 2 * Math.Sqrt(meanCPrime7 / (meanCPrime7 + 6103515625.0));
        double deltaTheta = 30 * Math.Exp(-Math.Pow((meanHPrime - 275) / 25, 2));
        double rt = -Math.Sin(2 * deltaTheta * Math.PI / 180) * rc;

        double term1 = deltaLPrime / (kL * sl);
        double term2 = deltaCPrime / (kC * sc);
        double term3 = deltaH / (kH * sh);

        return Math.Sqrt(term1 * term1 + term2 * term2 + term3 * term3 + rt * term2 * term3);
    }

    private static double CalculateDeltaECMC(LabColor lab1, LabColor lab2, double l, double c)
    {
        double deltaL = lab1.L - lab2.L;
        double c1 = Math.Sqrt(lab1.a * lab1.a + lab1.b * lab1.b);
        double c2 = Math.Sqrt(lab2.a * lab2.a + lab2.b * lab2.b);
        double deltaC = c1 - c2;
        double deltaA = lab1.a - lab2.a;
        double deltaB = lab1.b - lab2.b;
        double deltaH = Math.Sqrt(Math.Max(0, deltaA * deltaA + deltaB * deltaB - deltaC * deltaC));
        double h1 = (Math.Atan2(lab1.b, lab1.a) * 180.0 / Math.PI + 360) % 360;
        double f = Math.Sqrt(Math.Pow(c1, 4) / (Math.Pow(c1, 4) + 1900));
        double t;
        if (h1 is >= 164 and <= 345)
        {
            t = 0.56 + Math.Abs(0.2 * Math.Cos((h1 + 168) * Math.PI / 180));
        }
        else
        {
            t = 0.36 + Math.Abs(0.4 * Math.Cos((h1 + 35) * Math.PI / 180));
        }

        double sl = (lab1.L < 16) ? 0.511 : (0.040975 * lab1.L) / (1 + 0.01765 * lab1.L);
        double sc = (0.0638 * c1) / (1 + 0.0131 * c1) + 0.638;
        double sh = sc * (t * f + 1 - f);
        double termL = deltaL / (l * sl);
        double termC = deltaC / (c * sc);
        double termH = deltaH / sh;
        return Math.Sqrt(termL * termL + termC * termC + termH * termH);
    }

    #endregion Core Calculation Methods
}