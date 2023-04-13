using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Transactions;

namespace Masuit.Tools.Core
{
    public static class DbContextExt
    {
        /// <summary>
        /// 获取变化的实体信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<ChangeEntry> GetChanges<T>(this DbContext db)
        {
            return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified && e.Entity is T).Select(e =>
            {
                var originalObject = e.OriginalValues.ToObject();
                var currentObject = e.CurrentValues.ToObject();
                return new ChangeEntry
                {
                    EntityState = e.State,
                    Entity = e.Entity,
                    EntityType = e.OriginalValues.EntityType.ClrType,
                    ChangeProperties = e.OriginalValues.Properties.Select(p => (Property: p, Value: p.PropertyInfo.GetValue(originalObject))).Zip(e.CurrentValues.Properties.Select(p => (Property: p, Value: p.PropertyInfo.GetValue(currentObject))), (t1, t2) => new ChangePropertyInfo()
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
                var originalObject = e.OriginalValues.ToObject();
                var currentObject = e.CurrentValues.ToObject();
                return new ChangeEntry()
                {
                    EntityState = e.State,
                    Entity = e.Entity,
                    EntityType = e.OriginalValues.EntityType.ClrType,
                    ChangeProperties = e.OriginalValues.Properties.Select(p => (Property: p, Value: p.PropertyInfo.GetValue(originalObject))).Zip(e.CurrentValues.Properties.Select(p => (Property: p, Value: p.PropertyInfo.GetValue(currentObject))), (t1, t2) => new ChangePropertyInfo()
                    {
                        PropertyInfo = t1.Property.PropertyInfo,
                        OriginalValue = t1.Value,
                        CurrentValue = t2.Value,
                        IsPrimaryKey = t1.Property.IsPrimaryKey(),
                        IsForeignKey = t1.Property.IsForeignKey(),
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
        public static IEnumerable<ChangeEntry> GetAdded<T>(this DbContext db)
        {
            return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added && e.Entity is T).Select(e =>
            {
                var currentObject = e.CurrentValues.ToObject();
                return new ChangeEntry
                {
                    EntityState = e.State,
                    Entity = e.Entity,
                    EntityType = e.CurrentValues.EntityType.ClrType,
                    ChangeProperties = e.CurrentValues.Properties.Select(p => new ChangePropertyInfo()
                    {
                        PropertyInfo = p.PropertyInfo,
                        CurrentValue = p.PropertyInfo.GetValue(currentObject),
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
                var currentObject = e.CurrentValues.ToObject();
                return new ChangeEntry
                {
                    EntityState = e.State,
                    Entity = e.Entity,
                    EntityType = e.CurrentValues.EntityType.ClrType,
                    ChangeProperties = e.CurrentValues.Properties.Select(p => new ChangePropertyInfo()
                    {
                        PropertyInfo = p.PropertyInfo,
                        CurrentValue = p.PropertyInfo.GetValue(currentObject),
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
        public static IEnumerable<ChangeEntry> GetRemoved<T>(this DbContext db)
        {
            return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted && e.Entity is T).Select(e =>
            {
                var originalObject = e.OriginalValues.ToObject();
                return new ChangeEntry
                {
                    EntityState = e.State,
                    Entity = e.Entity,
                    EntityType = e.OriginalValues.EntityType.ClrType,
                    ChangeProperties = e.OriginalValues.Properties.Select(p => new ChangePropertyInfo()
                    {
                        PropertyInfo = p.PropertyInfo,
                        OriginalValue = p.PropertyInfo.GetValue(originalObject),
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
                var originalObject = e.OriginalValues.ToObject();
                return new ChangeEntry
                {
                    EntityState = e.State,
                    Entity = e.Entity,
                    EntityType = e.OriginalValues.EntityType.ClrType,
                    ChangeProperties = e.OriginalValues.Properties.Select(p => new ChangePropertyInfo()
                    {
                        PropertyInfo = p.PropertyInfo,
                        OriginalValue = p.PropertyInfo.GetValue(originalObject),
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
        public static IEnumerable<ChangeEntry> GetAllChanges<T>(this DbContext db)
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

        public static async Task<T> FirstOrDefaultWithNoLockAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = await query.FirstOrDefaultAsync(cancellationToken);
            scope.Complete();
            return result;
        }

        public static async Task<T> SingleOrDefaultWithNoLockAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> where, CancellationToken cancellationToken = default)
        {
            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }, TransactionScopeAsyncFlowOption.Enabled);
            var result = await query.SingleOrDefaultAsync(where, cancellationToken);
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
}
