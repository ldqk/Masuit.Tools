namespace Masuit.Tools.Media;

public struct CMYColor
{
    public double C { get; }
    public double M { get; }
    public double Y { get; }

    public CMYColor(double c, double m, double y)
    {
        C = c;
        M = m;
        Y = y;
    }
}