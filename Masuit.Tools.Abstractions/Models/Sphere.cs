using System;

namespace Masuit.Tools.Models
{
    /// <summary>
    /// 球体
    /// </summary>
    public class Sphere
    {
        /// <summary>
        /// 地球
        /// </summary>
        public static Sphere Earth => new(6371.393);

        public Sphere(double radius)
        {
            Radius = radius;
        }

        /// <summary>
        /// 半径
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="d">角度</param>
        /// <returns></returns>
        private static double Angle2Radian(double d)
        {
            return d / 180.0 * Math.PI;
        }

        /// <summary>
        /// 计算球体上两点的弧长
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns></returns>
        public double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            var rad1 = Angle2Radian(lat1);
            var rad2 = Angle2Radian(lat2);
            return 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin((rad1 - rad2) / 2), 2) + Math.Cos(rad1) * Math.Cos(rad2) * Math.Pow(Math.Sin((Angle2Radian(lng1) - Angle2Radian(lng2)) / 2), 2))) * Radius;
        }

        /// <summary>
        /// 是否相交
        /// </summary>
        /// <param name="circle1"></param>
        /// <param name="circle2"></param>
        /// <returns></returns>
        public bool IsCrossWith(Circle circle1, Circle circle2)
        {
            var dis = GetDistance(circle1.Center.X, circle1.Center.Y, circle2.Center.X, circle2.Center.Y);
            return circle1.Radius - circle2.Radius < dis && dis < circle1.Radius + circle2.Radius;
        }

        /// <summary>
        /// 是否相切
        /// </summary>
        /// <param name="circle1"></param>
        /// <param name="circle2"></param>
        /// <returns></returns>
        public bool IsIntersectWith(Circle circle1, Circle circle2)
        {
            var dis = GetDistance(circle1.Center.X, circle1.Center.Y, circle2.Center.X, circle2.Center.Y);
            return Math.Abs(circle1.Radius - circle2.Radius - dis) < 1e-7 || Math.Abs(dis - (circle1.Radius + circle2.Radius)) < 1e-7;
        }

        /// <summary>
        /// 是否相离
        /// </summary>
        /// <param name="circle1"></param>
        /// <param name="circle2"></param>
        /// <returns></returns>
        public bool IsSeparateWith(Circle circle1, Circle circle2)
        {
            return !IsCrossWith(circle1, circle2) && !IsIntersectWith(circle1, circle2);
        }
    }
}