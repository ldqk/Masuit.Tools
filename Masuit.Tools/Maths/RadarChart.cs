using System;
using System.Collections.Generic;

namespace Masuit.Tools.Maths
{
    public class RadarChart
    {
        public List<double> Data { get; set; }
        public double StartAngle { get; set; } // 弧度

        public RadarChart(List<double> data, double startAngle)
        {
            Data = new List<double>(data);
            StartAngle = startAngle;
        }

        public List<Point2D> GetPoints()
        {
            int count = Data.Count;
            List<Point2D> result = new List<Point2D>();
            for (int ii = 0; ii < count; ii++)
            {
                double length = Data[ii];
                double angle = StartAngle + Math.PI * 2 / count * ii;
                double x = length * Math.Cos(angle);
                double y = length * Math.Sin(angle);
                result.Add(new Point2D(x, y));
            }

            return result;
        }
    }
}