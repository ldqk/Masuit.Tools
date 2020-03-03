using Masuit.Tools.Maths;
using System;
using System.Collections.Generic;

namespace Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            var c1 = new RadarChart(new List<double>() { 2, 2, 2, 2 });
            var c2 = new RadarChart(new List<double>() { 3, 3, 3, 3 });
            var area = c1.ComputeIntersection(c2).ComputeArea();
            Console.WriteLine(area);
        }
    }
}