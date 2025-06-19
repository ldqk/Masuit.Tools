namespace Masuit.Tools.Media;

public struct LCHColor
{
    public double L { get; }
    public double C { get; }
    public double H { get; }

    public LCHColor(double l, double c, double h)
    {
        L = l;
        C = c;
        H = h;
    }
}