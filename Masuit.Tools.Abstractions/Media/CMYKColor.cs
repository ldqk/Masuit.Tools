namespace Masuit.Tools.Media;

public struct CMYKColor
{
    public double C { get; }
    public double M { get; }
    public double Y { get; }
    public double K { get; }

    public CMYKColor(double c, double m, double y, double k)
    {
        C = c;
        M = m;
        Y = y;
        K = k;
    }
}