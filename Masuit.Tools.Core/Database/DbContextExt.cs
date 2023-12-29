using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq.Expressions;
using System.Text;
using System.Transactions;
using Masuit.Tools.Systems;

namespace Masuit.Tools.Core;

public static class DbContextExt
{
    /// <summary>
    /// 获取变化的实体信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <returns></returns>
    public static IEnumerable<ChangeEntry<T>> GetChanges<T>(this DbContext db)
    {
        return db.ChangeTracker.Entries().Where(e => e is { State: EntityState.Modified, Entity: T }).Select(e =>
        {
            NullableDictionary<string, object> originalObject = e.OriginalValues.ToObject() as Dictionary<string, object> ?? new NullableDictionary<string, object>();
            NullableDictionary<string, object> currentObject = e.CurrentValues.ToObject() as Dictionary<string, object> ?? new NullableDictionary<string, object>();
            return new ChangeEntry<T>
            {
                EntityState = e.State,
                Entity = (T)e.Entity,
                EntityType = e.OriginalValues.EntityType.ClrType,
                ChangeProperties = e.OriginalValues.Properties.Select(p => (Property: p, Value: originalObject[p.PropertyInfo?.Name])).Zip(e.CurrentValues.Properties.Select(p => (Property: p, Value: currentObject[p.PropertyInfo?.Name])), (t1, t2) => new ChangePropertyInfo
                {
                    PropertyInfo = t1.Property.PropertyInfo,
                    OriginalValue = t1.Value,
                    CurrentValue = t2.Value,
                    IsPrimaryKey = t1.Property.IsPrimaryKey(),
                    IsForeignKey = t1.Property.IsForeignKey()
                }).Where(t => Comparer.Default.Compare(t.OriginalValue, t.CurrentValue) != 0).ToList()
            };
        });
    }

    /// <summary>
    /// 获取变化的实体信息
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public static IEnumerable<ChangeEntry> GetChanges(this DbContext db)
    {
        return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).Select(e =>
        {
            NullableDictionary<string, object> originalObject = e.OriginalValues.ToObject() as Dictionary<string, object> ?? new NullableDictionary<string, object>();
            NullableDictionary<string, object> currentObject = e.CurrentValues.ToObject() as Dictionary<string, object> ?? new NullableDictionary<string, object>();
            return new ChangeEntry
            {
                EntityState = e.State,
                Entity = e.Entity,
                EntityType = e.OriginalValues.EntityType.ClrType,
                ChangeProperties = e.OriginalValues.Properties.Select(p => (Property: p, Value: originalObject[p.PropertyInfo?.Name])).Zip(e.CurrentValues.Properties.Select(p => (Property: p, Value: currentObject[p.PropertyInfo?.Name])), (t1, t2) => new ChangePropertyInfo
                {
                    PropertyInfo = t1.Property.PropertyInfo,
                    OriginalValue = t1.Value,
                    CurrentValue = t2.Value,
                    IsPrimaryKey = t1.Property.IsPrimaryKey(),
                    IsForeignKey = t1.Property.IsForeignKey()
                }).Where(t => Comparer.Default.Compare(t.OriginalValue, t.CurrentValue) != 0).ToList()
            };
        });
    }

    /// <summary>
    /// 获取添加的实体信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <returns></returns>
    public static IEnumerable<ChangeEntry<T>> GetAdded<T>(this DbContext db)
    {
        return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added && e.Entity is T).Select(e =>
        {
            NullableDictionary<string, object> currentObject = e.CurrentValues.ToObject() as Dictionary<string, object> ?? new NullableDictionary<string, object>();
            return new ChangeEntry<T>
            {
                EntityState = e.State,
                Entity = (T)e.Entity,
                EntityType = e.CurrentValues.EntityType.ClrType,
                ChangeProperties = e.CurrentValues.Properties.Select(p => new ChangePropertyInfo()
                {
                    PropertyInfo = p.PropertyInfo,
                    CurrentValue = currentObject[p.PropertyInfo?.Name],
                    IsPrimaryKey = p.IsPrimaryKey(),
                    IsForeignKey = p.IsForeignKey(),
                }).ToList()
            };
        });
    }

    /// <summary>
    /// 获取添加的实体信息
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public static IEnumerable<ChangeEntry> GetAdded(this DbContext db)
    {
        return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(e =>
        {
            NullableDictionary<string, object> currentObject = e.CurrentValues.ToObject() as Dictionary<string, object> ?? new NullableDictionary<string, object>();
            return new ChangeEntry
            {
                EntityState = e.State,
                Entity = e.Entity,
                EntityType = e.CurrentValues.EntityType.ClrType,
                ChangeProperties = e.CurrentValues.Properties.Select(p => new ChangePropertyInfo
                {
                    PropertyInfo = p.PropertyInfo,
                    CurrentValue = currentObject[p.PropertyInfo?.Name],
                    IsPrimaryKey = p.IsPrimaryKey(),
                    IsForeignKey = p.IsForeignKey(),
                }).ToList()
            };
        });
    }

    /// <summary>
    /// 获取移除的实体信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <returns></returns>
    public static IEnumerable<ChangeEntry<T>> GetRemoved<T>(this DbContext db)
    {
        return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted && e.Entity is T).Select(e =>
        {
            NullableDictionary<string, object> originalObject = e.OriginalValues.ToObject() as Dictionary<string, object> ?? new NullableDictionary<string, object>();
            return new ChangeEntry<T>
            {
                EntityState = e.State,
                Entity = (T)e.Entity,
                EntityType = e.OriginalValues.EntityType.ClrType,
                ChangeProperties = e.OriginalValues.Properties.Select(p => new ChangePropertyInfo
                {
                    PropertyInfo = p.PropertyInfo,
                    OriginalValue = originalObject[p.PropertyInfo?.Name],
                    IsPrimaryKey = p.IsPrimaryKey(),
                    IsForeignKey = p.IsForeignKey(),
                }).ToList()
            };
        });
    }

    /// <summary>
    /// 获取移除的实体信息
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public static IEnumerable<ChangeEntry> GetRemoved(this DbContext db)
    {
        return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).Select(e =>
        {
            NullableDictionary<string, object> originalObject = e.OriginalValues.ToObject() as Dictionary<string, object> ?? new NullableDictionary<string, object>();
            return new ChangeEntry
            {
                EntityState = e.State,
                Entity = e.Entity,
                EntityType = e.OriginalValues.EntityType.ClrType,
                ChangeProperties = e.OriginalValues.Properties.Select(p => new ChangePropertyInfo
                {
                    PropertyInfo = p.PropertyInfo,
                    OriginalValue = originalObject[p.PropertyInfo?.Name],
                    IsPrimaryKey = p.IsPrimaryKey(),
                    IsForeignKey = p.IsForeignKey(),
                }).ToList()
            };
        });
    }

    /// <summary>
    /// 获取所有的变更信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <returns></returns>
    public static IEnumerable<ChangeEntry<T>> GetAllChanges<T>(this DbContext db)
    {
        return GetChanges<T>(db).Union(GetAdded<T>(db)).Union(GetRemoved<T>(db));
    }

    /// <summary>
    /// 获取所有的变更信息
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public static IEnumerable<ChangeEntry> GetAllChanges(this DbContext db)
    {
        return GetChanges(db).Union(GetAdded(db)).Union(GetRemoved(db));
    }

    public static IQueryable<TEntity> IncludeRecursive<TEntity>(this IQueryable<TEntity> source, int levelIndex, Expression<Func<TEntity, ICollection<TEntity>>> expression) where TEntity : class
    {
        if (levelIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(levelIndex));
        var member = (MemberExpression)expression.Body;
        var property = member.Member.Name;
        var sb = new StringBuilder();
        for (int i = 0; i < levelIndex; i++)
        {
            if (i > 0)
                sb.Append(Type.Delimiter);
            sb.Append(property);
        }

        return source.Include(sb.ToString());
    }

    public static async Task<List<T>> ToListWithNoLockAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await query.ToListAsync(cancellationToken);
        scope.Complete();
        return result;
    }

    public static List<T> ToListWithNoLock<T>(this IQueryable<T> query)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = query.ToList();
        scope.Complete();
        return result;
    }

    public static async Task<int> CountWithNoLockAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await query.CountAsync(cancellationToken);
        scope.Complete();
        return result;
    }

    public static int CountWithNoLock<T>(this IQueryable<T> query)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = query.Count();
        scope.Complete();
        return result;
    }

    public static async Task<int> CountWithNoLockAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await query.CountAsync(where, cancellationToken);
        scope.Complete();
        return result;
    }

    public static int CountWithNoLock<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = query.Count(where);
        scope.Complete();
        return result;
    }

    public static async Task<bool> AnyWithNoLockAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await query.AnyAsync(cancellationToken);
        scope.Complete();
        return result;
    }

    public static bool AnyWithNoLock<T>(this IQueryable<T> query)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = query.Any();
        scope.Complete();
        return result;
    }

    public static async Task<bool> AnyWithNoLockAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await query.AnyAsync(where, cancellationToken);
        scope.Complete();
        return result;
    }

    public static bool AnyWithNoLock<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = query.Any(where);
        scope.Complete();
        return result;
    }

    public static async Task<T> FirstOrDefaultWithNoLockAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await query.FirstOrDefaultAsync(where, cancellationToken);
        scope.Complete();
        return result;
    }

    public static T FirstOrDefaultWithNoLock<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = query.FirstOrDefault(where);
        scope.Complete();
        return result;
    }

    public static async Task<T> FirstOrDefaultWithNoLockAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await query.FirstOrDefaultAsync(cancellationToken);
        scope.Complete();
        return result;
    }

    public static T FirstOrDefaultWithNoLock<T>(this IQueryable<T> query)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = query.FirstOrDefault();
        scope.Complete();
        return result;
    }

    public static async Task<T> SingleOrDefaultWithNoLockAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await query.SingleOrDefaultAsync(where, cancellationToken);
        scope.Complete();
        return result;
    }

    public static T SingleOrDefaultWithNoLock<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = query.SingleOrDefault(where);
        scope.Complete();
        return result;
    }

    public static async Task<T> SingleOrDefaultWithNoLockAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await query.SingleOrDefaultAsync(cancellationToken);
        scope.Complete();
        return result;
    }

    public static T SingleOrDefaultWithNoLock<T>(this IQueryable<T> query)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = query.SingleOrDefault();
        scope.Complete();
        return result;
    }

    public static async Task<bool> AllWithNoLockAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = await query.AllAsync(where, cancellationToken);
        scope.Complete();
        return result;
    }

    public static bool AllWithNoLock<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
        {
            IsolationLevel = IsolationLevel.ReadUncommitted
        }, TransactionScopeAsyncFlowOption.Enabled);
        var result = query.All(where);
        scope.Complete();
        return result;
    }

    public static T NoLock<T, TDbContext>(this TDbContext dbContext, Func<TDbContext, T> func) where TDbContext : DbContext
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.Execute(() =>
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            };
            using var scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
            var result = func(dbContext);
            scope.Complete();
            return result;
        });
    }

    public static Task<T> NoLock<T, TDbContext>(this TDbContext dbContext, Func<TDbContext, Task<T>> func) where TDbContext : DbContext
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            };
            using var scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
            var result = await func(dbContext);
            scope.Complete();
            return result;
        });
    }

    public static T ExecutionStrategy<T, TDbContext>(this TDbContext dbContext, Func<TDbContext, T> func) where TDbContext : DbContext
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.Execute(() => func(dbContext));
    }

    public static Task<T> ExecutionStrategy<T, TDbContext>(this TDbContext dbContext, Func<TDbContext, Task<T>> func) where TDbContext : DbContext
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(() => func(dbContext));
    }
}
