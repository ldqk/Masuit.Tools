namespace Masuit.Tools.Media;

public struct LabColor
{
    public double L { get; }
    public double a { get; }
    public double b { get; }

    public LabColor(double l, double a, double b)
    {
        L = l;
        this.a = a;
        this.b = b;
    }
}