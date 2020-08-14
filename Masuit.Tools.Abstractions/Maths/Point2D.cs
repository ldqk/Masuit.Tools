namespace Masuit.Tools.Maths
{
    public class Point2D
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector2D operator -(Point2D first, Point2D second)
        {
            return new Vector2D(first.X - second.X, first.Y - second.Y);
        }

        public static Point2D operator +(Point2D pt, Vector2D vec)
        {
            return new Point2D(pt.X + vec.X, pt.Y + vec.Y);
        }
    }
}