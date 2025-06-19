namespace Masuit.Tools.Media;

public struct HSLColor
{
    public double H { get; }
    public double S { get; }
    public double L { get; }

    public HSLColor(double h, double s, double l)
    {
        H = h;
        S = s;
        L = l;
    }
}