using FastExpressionCompiler;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
namespace Masuit.Tools.Core.AspNetCore;

public static class DbSetExtensions
{
    /// <summary>
    /// 添加或更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey">按哪个字段更新</typeparam>
    /// <param name="dbSet"></param>
    /// <param name="keySelector">按哪个字段更新</param>
    /// <param name="entities"></param>
    public static void AddOrUpdate<T, TKey>(this DbSet<T> dbSet, Expression<Func<T, TKey>> keySelector, params T[] entities) where T : class
    {
        AddOrUpdate(dbSet, keySelector, entities.AsEnumerable());
    }

    /// <summary>
    /// 添加或更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey">按哪个字段更新</typeparam>
    /// <param name="dbSet"></param>
    /// <param name="keySelector">按哪个字段更新</param>
    /// <param name="entities"></param>
    /// <param name="ignoreNavigationProperty">是否忽略导航属性</param>
    public static void AddOrUpdate<T, TKey>(this DbSet<T> dbSet, Expression<Func<T, TKey>> keySelector, IEnumerable<T> entities, bool ignoreNavigationProperty = false) where T : class
    {
        if (keySelector == null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        if (entities is not ICollection<T> collection)
        {
            collection = entities.ToList();
        }

        var func = keySelector.CompileFast();
        var keyObjects = collection.Select(s => func(s)).ToList();
        var parameter = keySelector.Parameters[0];
        var array = Expression.Constant(keyObjects);
        var call = Expression.Call(array, typeof(List<TKey>).GetMethod(nameof(List<TKey>.Contains)), keySelector.Body);
        var lambda = Expression.Lambda<Func<T, bool>>(call, parameter);
        var items = dbSet.Where(lambda).ToDictionary(t => func(t));
        foreach (var entity in collection)
        {
            var key = func(entity);
            if (items.ContainsKey(key))
            {
                // 获取主键字段
                var dataType = typeof(T);
                var keyIgnoreFields = dataType.GetProperties().Where(p => p.GetCustomAttribute<KeyAttribute>() != null || p.GetCustomAttribute<UpdateIgnoreAttribute>() != null).ToList();
                if (!keyIgnoreFields.Any())
                {
                    string idName = dataType.Name + "Id";
                    keyIgnoreFields.AddRange(dataType.GetProperties().Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) || p.Name.Equals(idName, StringComparison.OrdinalIgnoreCase)));
                }

                if (ignoreNavigationProperty)
                {
                    keyIgnoreFields.AddRange(dataType.GetProperties().Where(p => p.PropertyType.Namespace == "System.Collections.Generic"));
                }

                // 更新所有非主键属性
                foreach (var p in typeof(T).GetProperties().Where(p => p.GetSetMethod() != null && p.GetGetMethod() != null))
                {
                    // 忽略主键和被忽略的字段
                    if (keyIgnoreFields.Any(x => x.Name == p.Name))
                    {
                        continue;
                    }

                    var existingValue = p.GetValue(entity);
                    if (p.GetValue(items[key]) != existingValue)
                    {
                        p.SetValue(items[key], existingValue);
                    }
                }

                foreach (var idField in keyIgnoreFields.Where(p => p.SetMethod != null && p.GetMethod != null))
                {
                    var existingValue = idField.GetValue(items[key]);
                    if (idField.GetValue(entity) != existingValue)
                    {
                        idField.SetValue(entity, existingValue);
                    }
                }
            }
            else
            {
                dbSet.Add(entity);
            }
        }
    }

    /// <summary>
    /// 添加或更新
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey">按哪个字段更新</typeparam>
    /// <param name="dbSet"></param>
    /// <param name="keySelector">按哪个字段更新</param>
    /// <param name="entity"></param>
    /// <param name="ignoreNavigationProperty">是否忽略导航属性</param>
    public static void AddOrUpdate<T, TKey>(this DbSet<T> dbSet, Expression<Func<T, TKey>> keySelector, T entity, bool ignoreNavigationProperty = false) where T : class
    {
        if (keySelector == null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        var keyObject = keySelector.CompileFast()(entity);
        var parameter = keySelector.Parameters[0];
        var lambda = Expression.Lambda<Func<T, bool>>(Expression.Equal(keySelector.Body, Expression.Constant(keyObject)), parameter);
        var item = dbSet.FirstOrDefault(lambda);
        if (item == null)
        {
            dbSet.Add(entity);
        }
        else
        {
            // 获取主键字段
            var dataType = typeof(T);
            var keyIgnoreFields = dataType.GetProperties().Where(p => p.GetCustomAttribute<KeyAttribute>() != null || p.GetCustomAttribute<UpdateIgnoreAttribute>() != null).ToList();
            if (!keyIgnoreFields.Any())
            {
                string idName = dataType.Name + "Id";
                keyIgnoreFields.AddRange(dataType.GetProperties().Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) || p.Name.Equals(idName, StringComparison.OrdinalIgnoreCase)));
            }

            if (ignoreNavigationProperty)
            {
                keyIgnoreFields.AddRange(dataType.GetProperties().Where(p => p.PropertyType.Namespace == "System.Collections.Generic"));
            }

            // 更新所有非主键属性
            foreach (var p in typeof(T).GetProperties().Where(p => p.GetSetMethod() != null && p.GetGetMethod() != null))
            {
                // 忽略主键和被忽略的字段
                if (keyIgnoreFields.Any(x => x.Name == p.Name))
                {
                    continue;
                }

                var existingValue = p.GetValue(entity);
                if (p.GetValue(item) != existingValue)
                {
                    p.SetValue(item, existingValue);
                }
            }

            foreach (var idField in keyIgnoreFields.Where(p => p.SetMethod != null && p.GetMethod != null))
            {
                var existingValue = idField.GetValue(item);
                if (idField.GetValue(entity) != existingValue)
                {
                    idField.SetValue(entity, existingValue);
                }
            }
        }
    }

#if NET6_0_OR_GREATER

    /// <summary>
    /// 随机排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    public static IOrderedQueryable<T> OrderByRandom<T>(this IQueryable<T> query)
    {
        return query.OrderBy(_ => EF.Functions.Random());
    }
#endif
}