using System;
using System.Diagnostics;

namespace Masuit.Tools.Systems
{
    /// <summary>
    /// stopwatch扩展
    /// </summary>
    public static class StopwatchExtension
    {
        /// <summary>
        /// 检测方法执行时间
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static double Execute(Action action)
        {
            Stopwatch sw = Stopwatch.StartNew();
            action();
            return sw.ElapsedMilliseconds;
        }
    }
}