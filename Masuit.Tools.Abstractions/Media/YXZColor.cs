namespace Masuit.Tools.Media;

public struct YXZColor
{
    public double Y { get; }
    public double X { get; }
    public double Z { get; }

    public YXZColor(double y, double x, double z)
    {
        Y = y;
        X = x;
        Z = z;
    }
}