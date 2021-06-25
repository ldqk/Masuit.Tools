using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Masuit.Tools.Core
{
    public class ChangePropertyValue
    {
        /// <summary>
        /// 属性
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// 原始值
        /// </summary>
        public object OriginalValue { get; set; }

        /// <summary>
        /// 新值
        /// </summary>
        public object CurrentValue { get; set; }
    }

    public static class DbContextExt
    {
        /// <summary>
        /// 获取变化的实体信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<ChangePropertyValue> GetChanges<T>(this DbContext db)
        {
            return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified && e.Entity is T).SelectMany(e =>
            {
                var originalObject = e.OriginalValues.ToObject();
                var currentObject = e.CurrentValues.ToObject();
                return e.OriginalValues.Properties.Select(p => (p.PropertyInfo, Value: p.PropertyInfo.GetValue(originalObject))).Zip(e.CurrentValues.Properties.Select(p => (p.PropertyInfo, Value: p.PropertyInfo.GetValue(currentObject))), (t1, t2) => new ChangePropertyValue()
                {
                    PropertyInfo = t1.PropertyInfo,
                    OriginalValue = t1.Value,
                    CurrentValue = t2.Value
                }).Where(t => Comparer.Default.Compare(t.OriginalValue, t.CurrentValue) != 0);
            });
        }

        /// <summary>
        /// 获取添加的实体信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<ChangePropertyValue> GetAdded<T>(this DbContext db)
        {
            return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added && e.Entity is T).SelectMany(e =>
            {
                var currentObject = e.CurrentValues.ToObject();
                return e.CurrentValues.Properties.Select(p => (p.PropertyInfo, Value: p.PropertyInfo.GetValue(currentObject))).Select(t => new ChangePropertyValue
                {
                    PropertyInfo = t.PropertyInfo,
                    CurrentValue = t.Value
                });
            });
        }

        /// <summary>
        /// 获取移除的实体信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<ChangePropertyValue> GetRemoved<T>(this DbContext db)
        {
            return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted && e.Entity is T).SelectMany(e =>
            {
                var originalObject = e.OriginalValues.ToObject();
                return e.OriginalValues.Properties.Select(p => (p.PropertyInfo, Value: p.PropertyInfo.GetValue(originalObject))).Select(t => new ChangePropertyValue
                {
                    PropertyInfo = t.PropertyInfo,
                    OriginalValue = t.Value
                });
            });
        }
    }
}