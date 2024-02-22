using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Masuit.Tools.Systems;

/// <summary>
/// 高精度定时器，1ms精度
/// </summary>
public class HighAccurateTimer : Disposable
{
    private readonly long _clockFrequency; // result of QueryPerformanceFrequency()
    private bool _running;
    private Thread _timerThread;

    private int _intervalMs; // interval in mimliseccond;

    ///
    /// Timer inteval in milisecond
    ///
    public int Interval
    {
        get => _intervalMs;
        set
        {
            _intervalMs = value;
            _intevalTicks = (long)(value * (double)_clockFrequency / 1000);
        }
    }

    private long _intevalTicks;

    [DllImport("Kernel32.dll")]
    private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

    [DllImport("Kernel32.dll")]
    private static extern bool QueryPerformanceFrequency(out long lpFrequency);

    public HighAccurateTimer(int interval = 1)
    {
        if (QueryPerformanceFrequency(out _clockFrequency) == false)
        {
            // Frequency not supported
            throw new Win32Exception("QueryPerformanceFrequency() function is not supported");
        }
        Interval = interval;
    }

    public bool GetTick(out long currentTickCount)
    {
        if (QueryPerformanceCounter(out currentTickCount) == false)
            throw new Win32Exception("QueryPerformanceCounter() failed!");
        else
            return true;
    }

    public void Start(Action func)
    {
        _running = true;
        _timerThread = new Thread(() =>
        {
            GetTick(out var currTime);
            var nextTriggerTime = currTime + _intevalTicks; // the time when next task will be executed
            while (_running)
            {
                while (currTime < nextTriggerTime)
                {
                    GetTick(out currTime);
                } // wailt an interval

                nextTriggerTime = currTime + _intevalTicks;
                func();
            }
        })
        {
            Name = "HighAccuracyTimer",
            Priority = ThreadPriority.Highest
        };

        _timerThread.Start();
    }

    public void Stop()
    {
        _running = false;
    }

    ~HighAccurateTimer()
    {
        _running = false;
    }

    /// <summary>
    /// 释放
    /// </summary>
    /// <param name="disposing"></param>
    public override void Dispose(bool disposing)
    {
        Stop();
    }
}