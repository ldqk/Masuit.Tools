using System;
using System.Collections.Generic;

namespace Masuit.Tools.Maths
{
    public class RadarChart
    {
        /// <summary>
        /// 向量长度集合
        /// </summary>
        public List<double> Data { get; }

        /// <summary>
        /// 起始弧度
        /// </summary>
        public double StartAngle { get; } // 弧度

        /// <summary>
        /// 多边形
        /// </summary>
        /// <param name="data">向量长度集合</param>
        /// <param name="startAngle">起始弧度</param>
        public RadarChart(List<double> data, double startAngle = 0)
        {
            Data = new List<double>(data);
            StartAngle = startAngle;
        }

        /// <summary>
        /// 获取每个点的坐标
        /// </summary>
        /// <returns></returns>
        public List<Point2D> GetPoints()
        {
            int count = Data.Count;
            List<Point2D> result = new List<Point2D>();
            for (int i = 0; i < count; i++)
            {
                double length = Data[i];
                double angle = StartAngle + Math.PI * 2 / count * i;
                double x = length * Math.Cos(angle);
                double y = length * Math.Sin(angle);
                result.Add(new Point2D(x, y));
            }

            return result;
        }
    }
}