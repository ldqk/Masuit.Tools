using System;
using System.Diagnostics;

namespace Masuit.Tools
{
    public static class RandomExtensions
    {
        /// <summary>
        /// 生成真正的随机数
        /// </summary>
        /// <param name="r"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static int StrictNext(this Random r, int seed = int.MaxValue)
        {
            return new Random((int)Stopwatch.GetTimestamp()).Next(seed);
        }

        /// <summary>
        /// 产生正态分布的随机数
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="mean">均值</param>
        /// <param name="stdDev">方差</param>
        /// <returns></returns>
        public static double NextGauss(this Random rand, double mean, double stdDev)
        {
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }
    }
}