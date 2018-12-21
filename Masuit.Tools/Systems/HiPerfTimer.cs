using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Masuit.Tools.Systems
{
    public class HiPerfTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private long _startTime;
        private long _stopTime;
        private readonly long _freq;

        // 构造函数 
        public HiPerfTimer()
        {
            _startTime = 0;
            _stopTime = 0;

            if (QueryPerformanceFrequency(out _freq) == false)
            {
                // 不支持高性能计数器 
                throw new Win32Exception();
            }
        }

        // 开始计时器 
        public void Start()
        {
            // 来让等待线程工作 
            Thread.Sleep(0);
            QueryPerformanceCounter(out _startTime);
        }

        // 开始计时器 
        public static HiPerfTimer StartNew()
        {
            HiPerfTimer timer = new HiPerfTimer();
            timer.Start();
            return timer;
        }

        // 停止计时器 
        public void Stop()
        {
            QueryPerformanceCounter(out _stopTime);
        }

        // 返回计时器经过时间(单位：秒) 
        public double Duration => (_stopTime - _startTime) / (double)_freq;

        /// <summary>
        /// 执行一个方法并测试执行时间
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static double Execute(Action action)
        {
            var timer = new HiPerfTimer();
            timer.Start();
            action();
            timer.Stop();
            return timer.Duration;
        }
    }
}