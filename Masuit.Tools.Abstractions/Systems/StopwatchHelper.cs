using System;
using System.Diagnostics;

namespace Masuit.Tools.Systems
{
    /// <summary>
    /// 计数器帮助类
    /// </summary>
    public static class StopwatchHelper
    {
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static double Execute(Action action)
        {
            var sw = Stopwatch.StartNew();
            action();
            return sw.ElapsedMilliseconds;
        }
    }
}