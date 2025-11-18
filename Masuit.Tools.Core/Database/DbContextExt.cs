using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq.Expressions;
using System.Text;
using System.Transactions;
using Masuit.Tools.Systems;
using Masuit.Tools.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query;

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
#if NET10_0_OR_GREATER
                EntityType = e.OriginalValues.StructuralType.ClrType,
#else
                EntityType = e.OriginalValues.EntityType.ClrType,
#endif
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
#if NET10_0_OR_GREATER
                EntityType = e.OriginalValues.StructuralType.ClrType,
#else
                EntityType = e.OriginalValues.EntityType.ClrType,
#endif
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
#if NET10_0_OR_GREATER
                EntityType = e.OriginalValues.StructuralType.ClrType,
#else
                EntityType = e.OriginalValues.EntityType.ClrType,
#endif
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
#if NET10_0_OR_GREATER
                EntityType = e.OriginalValues.StructuralType.ClrType,
#else
                EntityType = e.OriginalValues.EntityType.ClrType,
#endif
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
#if NET10_0_OR_GREATER
                EntityType = e.OriginalValues.StructuralType.ClrType,
#else
                EntityType = e.OriginalValues.EntityType.ClrType,
#endif
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
#if NET10_0_OR_GREATER
                EntityType = e.OriginalValues.StructuralType.ClrType,
#else
                EntityType = e.OriginalValues.EntityType.ClrType,
#endif
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

    public static Task<List<T>> ToListWithNoLockAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        return ExecuteStrategyAsync(query, async (q) =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = await q.ToListAsync(cancellationToken);
            scope.Complete();
            return result;
        });
    }

    public static List<T> ToListWithNoLock<T>(this IQueryable<T> query)
    {
        return ExecuteStrategy(query, q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = q.ToList();
            scope.Complete();
            return result;
        });
    }

    public static Task<int> CountWithNoLockAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        return ExecuteStrategyAsync(query, async q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = await q.CountAsync(cancellationToken);
            scope.Complete();
            return result;
        });
    }

    public static int CountWithNoLock<T>(this IQueryable<T> query)
    {
        return ExecuteStrategy(query, q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = q.Count();
            scope.Complete();
            return result;
        });
    }

    public static Task<int> CountWithNoLockAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
    {
        return ExecuteStrategyAsync(query, async q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = await q.CountAsync(where, cancellationToken);
            scope.Complete();
            return result;
        });
    }

    public static int CountWithNoLock<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        return ExecuteStrategy(query, q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = q.Count(where);
            scope.Complete();
            return result;
        });
    }

    public static Task<bool> AnyWithNoLockAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        return ExecuteStrategyAsync(query, async q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = await q.AnyAsync(cancellationToken);
            scope.Complete();
            return result;
        });
    }

    public static bool AnyWithNoLock<T>(this IQueryable<T> query)
    {
        return ExecuteStrategy(query, q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = q.Any();
            scope.Complete();
            return result;
        });
    }

    public static Task<bool> AnyWithNoLockAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
    {
        return ExecuteStrategyAsync(query, async q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = await q.AnyAsync(where, cancellationToken);
            scope.Complete();
            return result;
        });
    }

    public static bool AnyWithNoLock<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        return ExecuteStrategy(query, q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = q.Any(where);
            scope.Complete();
            return result;
        });
    }

    public static Task<T> FirstOrDefaultWithNoLockAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
    {
        return ExecuteStrategyAsync(query, async q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = await q.FirstOrDefaultAsync(where, cancellationToken);
            scope.Complete();
            return result;
        });
    }

    public static T FirstOrDefaultWithNoLock<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        return ExecuteStrategy(query, q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = q.FirstOrDefault(where);
            scope.Complete();
            return result;
        });
    }

    public static Task<T> FirstOrDefaultWithNoLockAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        return ExecuteStrategyAsync(query, async q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = await q.FirstOrDefaultAsync(cancellationToken);
            scope.Complete();
            return result;
        });
    }

    public static T FirstOrDefaultWithNoLock<T>(this IQueryable<T> query)
    {
        return ExecuteStrategy(query, q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = q.FirstOrDefault();
            scope.Complete();
            return result;
        });
    }

    public static Task<T> SingleOrDefaultWithNoLockAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
    {
        return ExecuteStrategyAsync(query, async q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = await q.SingleOrDefaultAsync(where, cancellationToken);
            scope.Complete();
            return result;
        });
    }

    public static T SingleOrDefaultWithNoLock<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        return ExecuteStrategy(query, q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = q.SingleOrDefault(where);
            scope.Complete();
            return result;
        });
    }

    public static Task<T> SingleOrDefaultWithNoLockAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        return ExecuteStrategyAsync(query, async q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = await q.SingleOrDefaultAsync(cancellationToken);
            scope.Complete();
            return result;
        });
    }

    public static T SingleOrDefaultWithNoLock<T>(this IQueryable<T> query)
    {
        return ExecuteStrategy(query, q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = q.SingleOrDefault();
            scope.Complete();
            return result;
        });
    }

    public static Task<bool> AllWithNoLockAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
    {
        return ExecuteStrategyAsync(query, async q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = await q.AllAsync(where, cancellationToken);
            scope.Complete();
            return result;
        });
    }

    public static bool AllWithNoLock<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
    {
        return ExecuteStrategy(query, q =>
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = query.All(where);
            scope.Complete();
            return result;
        });
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

    public static T ExecuteStrategy<T, TDbContext>(this TDbContext dbContext, Func<TDbContext, T> func) where TDbContext : DbContext
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.Execute(() => func(dbContext));
    }

    public static Task<T> ExecuteStrategy<T, TDbContext>(this TDbContext dbContext, Func<TDbContext, Task<T>> func) where TDbContext : DbContext
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(() => func(dbContext));
    }

    public static TResult ExecuteStrategy<T, TResult>(this IQueryable<T> query, Func<IQueryable<T>, TResult> func)
    {
        if (query.Provider is not EntityQueryProvider)
        {
            return func(query);
        }

        var dependencies = query.Provider.GetField<QueryCompiler>("_queryCompiler").GetField<CompiledQueryCacheKeyGenerator>("_compiledQueryCacheKeyGenerator").GetField<CompiledQueryCacheKeyGeneratorDependencies>("Dependencies");
#if NETSTANDARD2_0
        var context = dependencies.Context.Context;
#else
        var context = dependencies.CurrentContext.Context;
#endif
        var strategy = context.Database.CreateExecutionStrategy();
        return strategy.Execute(() => func(query));
    }

    public static Task<TResult> ExecuteStrategyAsync<T, TResult>(this IQueryable<T> query, Func<IQueryable<T>, Task<TResult>> func)
    {
        if (query.Provider is not EntityQueryProvider)
        {
            return func(query);
        }

        var dependencies = query.Provider.GetField<QueryCompiler>("_queryCompiler").GetField<CompiledQueryCacheKeyGenerator>("_compiledQueryCacheKeyGenerator").GetField<CompiledQueryCacheKeyGeneratorDependencies>("Dependencies");
#if NETSTANDARD2_0
        var context = dependencies.Context.Context;
#else
        var context = dependencies.CurrentContext.Context;
#endif
        var strategy = context.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(() => func(query));
    }
}