using Masuit.Tools.Maths;
using System;

namespace Masuit.Tools.Models
{
    /// <summary>
    /// 圆形
    /// </summary>
    public class Circle
    {
        /// <summary>
        /// 圆形
        /// </summary>
        /// <param name="center">圆心坐标</param>
        /// <param name="radius">半径</param>
        public Circle(Point2D center, double radius)
        {
            Center = center;
            Radius = radius;
            if (radius < 0)
            {
                throw new ArgumentException("半径不能为负数", nameof(radius));
            }
        }

        /// <summary>
        /// 圆心坐标
        /// </summary>
        public Point2D Center { get; }

        /// <summary>
        /// 半径
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// 是否相交
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public bool IsCrossWith(Circle that)
        {
            var dis = Math.Sqrt(Math.Pow(that.Center.X - Center.X, 2) + Math.Pow(that.Center.Y - Center.Y, 2));
            return that.Radius - Radius < dis && dis < that.Radius + Radius;
        }

        /// <summary>
        /// 是否相切
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public bool IsIntersectWith(Circle that)
        {
            var dis = Math.Sqrt(Math.Pow(that.Center.X - Center.X, 2) + Math.Pow(that.Center.Y - Center.Y, 2));
            return Math.Abs(that.Radius - Radius - dis) < 1e-7 || Math.Abs(dis - (that.Radius + Radius)) < 1e-7;
        }

        /// <summary>
        /// 是否相离
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public bool IsSeparateWith(Circle that)
        {
            return !IsCrossWith(that) && !IsIntersectWith(that);
        }
    }
}