using System;
using System.Collections.Generic;

namespace Masuit.Tools.Maths
{
    /// <summary>
    /// 雷达图引擎
    /// </summary>
    public static class RadarChartEngine
    {
        /// <summary>
        /// 计算多边形面积的函数
        /// (以原点为基准点,分割为多个三角形)
        /// 定理：任意多边形的面积可由任意一点与多边形上依次两点连线构成的三角形矢量面积求和得出。矢量面积=三角形两边矢量的叉乘。
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static double ComputeArea(this List<Point2D> points)
        {
            double area = 0;
            var iCount = points.Count;

            for (var i = 0; i < iCount; i++)
            {
                area = area + (points[i].X * points[(i + 1) % iCount].Y - points[(i + 1) % iCount].X * points[i].Y);
            }

            return Math.Abs(0.5 * area);
        }

        /// <summary>
        /// 获取两个多边形的相交区域
        /// </summary>
        /// <param name="first">多边形1</param>
        /// <param name="second">多边形2</param>
        /// <returns></returns>
        public static List<Point2D> ComputeIntersection(this RadarChart first, RadarChart second)
        {
            double tol = 1e-6;
            if (null == first || null == second)
            {
                throw new ArgumentException();
            }

            if (Math.Abs(first.StartAngle - second.StartAngle) > tol || first.Data.Count != second.Data.Count)
            {
                throw new ArgumentException();
            }

            int count = first.Data.Count;
            List<Point2D> result = new List<Point2D>();
            List<Point2D> firstPoints = first.GetPoints();
            List<Point2D> secondPoints = second.GetPoints();
            for (int i = 0; i < count; i++)
            {
                Point2D tmp = (first.Data[i] > second.Data[i]) ? secondPoints[i] : firstPoints[i];
                result.Add(tmp);
            }

            for (int i = count; i > 0; i--)
            {
                int curIdx = i % count;
                int preIdx = i - 1;
                if (first.Data[curIdx] > second.Data[curIdx] == first.Data[preIdx] < second.Data[preIdx])
                {
                    var intersectPt = GetIntersect(firstPoints[preIdx], firstPoints[curIdx], secondPoints[preIdx], secondPoints[curIdx]);
                    result.Insert(i, intersectPt);
                }
            }

            return result;
        }

        private static Point2D GetIntersect(Point2D lineFirstStart, Point2D lineFirstEnd, Point2D lineSecondStart, Point2D lineSecondEnd)
        {
            Vector2D firstVec = lineFirstEnd - lineFirstStart;
            Vector2D secondVec = lineSecondEnd - lineSecondStart;
            double factor = firstVec.X * secondVec.Y - firstVec.Y * secondVec.X;
            double dis = secondVec.X * (lineFirstStart.Y - lineSecondStart.Y) - secondVec.Y * (lineFirstStart.X - lineSecondStart.X);
            double radio = dis / factor;

            return lineFirstStart + firstVec * radio;
        }
    }
}