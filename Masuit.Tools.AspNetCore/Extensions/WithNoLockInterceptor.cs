using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Masuit.Tools.Core;

/// <summary>
/// WITH (NOLOCK)全局拦截器，仅限SQL Server使用
/// </summary>
/// <param name="enableGlobalNolock">全局启用，无需手动调用WithNolock扩展</param>
public class WithNoLockInterceptor(bool enableGlobalNolock = false) : DbCommandInterceptor
{
    private static readonly Regex TableRegex = new Regex(@"(FROM|JOIN) \[[a-zA-Z]\w*\] AS \[[a-zA-Z]\w*\](?! WITH \(NOLOCK\))", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        AddWithNoLock(command);
        return base.ScalarExecuting(command, eventData, result);
    }

#if NETCOREAPP3_1

    public override Task<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
CancellationToken cancellationToken = new CancellationToken())
    {
        AddWithNoLock(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

#else

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
CancellationToken cancellationToken = new CancellationToken())
    {
        AddWithNoLock(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

#endif

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        AddWithNoLock(command);
        return result;
    }

#if NETCOREAPP3_1

    public override Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
CancellationToken cancellationToken = new CancellationToken())
    {
        AddWithNoLock(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

#else

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
CancellationToken cancellationToken = new CancellationToken())
    {
        AddWithNoLock(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

#endif

    private void AddWithNoLock(DbCommand command)
    {
        // 检查查询是否有标记
        if (enableGlobalNolock || command.CommandText.StartsWith("-- NOLOCK"))
        {
            command.CommandText = TableRegex.Replace(command.CommandText, "$0 WITH (NOLOCK)");
        }
    }
}

public static class WithNoLockExt
{
    public static IQueryable<T> WithNolock<T>(this IQueryable<T> queryable)
    {
        return queryable.TagWith("-- NOLOCK");
    }
}

[Obsolete("请使用WithNoLockInterceptor替代")]
public class QueryWithNoLockDbCommandInterceptor : WithNoLockInterceptor
{ }