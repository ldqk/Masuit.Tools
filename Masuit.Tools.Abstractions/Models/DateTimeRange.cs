using System;
using System.Collections.Generic;
using System.Linq;
using Masuit.Tools.DateTimeExt;

namespace Masuit.Tools.Models;

/// <summary>
/// 时间段
/// </summary>
public class DateTimeRange : ITimePeriod
{
    public DateTimeRange(DateTime? start, DateTime? end)
    {
        if (start > end)
        {
            throw new Exception("开始时间不能大于结束时间");
        }

        Start = start;
        End = end;
    }

    public DateTimeRange()
    {
    }

    /// <summary>
    /// 起始时间
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// 是否相交
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public bool HasIntersect(DateTime? start, DateTime? end)
    {
        return HasIntersect(new DateTimeRange(start, end));
    }

    /// <summary>
    /// 是否相交
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool HasIntersect(DateTimeRange range)
    {
        return IsDatetimeRangeOverlap(Start, End, range.Start, range.End);
    }

    /// <summary>
    /// 相交时间段
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public (bool intersected, DateTimeRange range) Intersect(DateTimeRange range)
    {
        if (!HasIntersect(range.Start, range.End))
        {
            return (false, null);
        }

        var list = new HashSet<DateTime?> { Start, range.Start, End, range.End }.ToList();
        list.Sort();
        return (true, new DateTimeRange(list[1], list[2]));
    }

    /// <summary>
    /// 相交时间段
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public (bool intersected, DateTimeRange range) Intersect(DateTime start, DateTime end)
    {
        return Intersect(new DateTimeRange(start, end));
    }

    /// <summary>
    /// 是否包含时间段
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool Contains(DateTimeRange range)
    {
        return IsDatetimeRangeOverlap(Start, End, range.Start, range.End);
    }

    /// <summary>
    /// 是否包含时间段
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public bool Contains(DateTime? start, DateTime? end)
    {
        return Contains(new DateTimeRange(start, end));
    }

    /// <summary>
    /// 是否在时间段内
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool In(DateTimeRange range)
    {
        return IsDatetimeRangeOverlap(Start, End, range.Start, range.End);
    }

    /// <summary>
    /// 是否在时间段内
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public bool In(DateTime start, DateTime end)
    {
        return In(new DateTimeRange(start, end));
    }

    /// <summary>
    /// 合并时间段
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public DateTimeRange Union(DateTimeRange range)
    {
        if (!HasIntersect(range))
            throw new Exception("不相交的时间段不能合并");
        var list = new HashSet<DateTime?> { Start, range.Start, End, range.End }.ToList();
        list.Sort();
        return new DateTimeRange(list[0], list[3]);
    }

    /// <summary>
    /// 合并时间段
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public DateTimeRange Union(DateTime start, DateTime end)
    {
        return Union(new DateTimeRange(start, end));
    }

    /// <summary>返回一个表示当前对象的 string。</summary>
    /// <returns>表示当前对象的字符串。</returns>
    public override string ToString()
    {
        return $"{Start:yyyy-MM-dd HH:mm:ss}~{End:yyyy-MM-dd HH:mm:ss}";
    }

    public override bool Equals(object obj)
    {
        if (obj is DateTimeRange range)
        {
            return Start?.Date == range.Start?.Date && End?.Date == range.End?.Date;
        }

        return false;
    }

    private static bool IsDatetimeRangeOverlap(DateTime? startDateA, DateTime? endDateA, DateTime? startDateB, DateTime? endDateB)
    {
        return !(endDateA < startDateB || endDateB < startDateA);
    }
}