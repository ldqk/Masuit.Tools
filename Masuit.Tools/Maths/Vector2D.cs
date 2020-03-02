namespace Masuit.Tools.Maths
{
    public class Vector2D
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector2D operator *(Vector2D vec, double factor)
        {
            return new Vector2D(vec.X * factor, vec.Y * factor);
        }
    }
}