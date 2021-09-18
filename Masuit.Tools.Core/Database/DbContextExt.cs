using Microsoft.EntityFrameworkCore;
using System;
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

        /// <summary>
        /// 所属实体
        /// </summary>
        public object Entity { get; set; }

        /// <summary>
        /// 实体类型
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// 变更类型
        /// </summary>
        public EntityState EntityState { get; set; }

        /// <summary>
        /// 是否是主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// 是否是外键
        /// </summary>
        public bool IsForeignKey { get; set; }

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
                return e.OriginalValues.Properties.Select(p => (Property: p, Value: p.PropertyInfo.GetValue(originalObject))).Zip(e.CurrentValues.Properties.Select(p => (Property: p, Value: p.PropertyInfo.GetValue(currentObject))), (t1, t2) => new ChangePropertyValue()
                {
                    PropertyInfo = t1.Property.PropertyInfo,
                    OriginalValue = t1.Value,
                    CurrentValue = t2.Value,
                    EntityState = e.State,
                    Entity = e.Entity,
                    IsPrimaryKey = t1.Property.IsPrimaryKey(),
                    IsForeignKey = t1.Property.IsForeignKey(),
                    EntityType = e.OriginalValues.EntityType.ClrType
                }).Where(t => Comparer.Default.Compare(t.OriginalValue, t.CurrentValue) != 0);
            });
        }

        /// <summary>
        /// 获取变化的实体信息
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<ChangePropertyValue> GetChanges(this DbContext db)
        {
            return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).SelectMany(e =>
            {
                var originalObject = e.OriginalValues.ToObject();
                var currentObject = e.CurrentValues.ToObject();
                return e.OriginalValues.Properties.Select(p => (Property: p, Value: p.PropertyInfo.GetValue(originalObject))).Zip(e.CurrentValues.Properties.Select(p => (Property: p, Value: p.PropertyInfo.GetValue(currentObject))), (t1, t2) => new ChangePropertyValue()
                {
                    PropertyInfo = t1.Property.PropertyInfo,
                    OriginalValue = t1.Value,
                    CurrentValue = t2.Value,
                    EntityState = e.State,
                    Entity = e.Entity,
                    IsPrimaryKey = t1.Property.IsPrimaryKey(),
                    IsForeignKey = t1.Property.IsForeignKey(),
                    EntityType = e.OriginalValues.EntityType.ClrType
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
                return e.CurrentValues.Properties.Select(p => (Property: p, Value: p.PropertyInfo.GetValue(currentObject))).Select(t => new ChangePropertyValue
                {
                    PropertyInfo = t.Property.PropertyInfo,
                    CurrentValue = t.Value,
                    EntityState = e.State,
                    Entity = e.Entity,
                    IsPrimaryKey = t.Property.IsPrimaryKey(),
                    IsForeignKey = t.Property.IsForeignKey(),
                    EntityType = e.OriginalValues.EntityType.ClrType
                });
            });
        }

        /// <summary>
        /// 获取添加的实体信息
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<ChangePropertyValue> GetAdded(this DbContext db)
        {
            return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).SelectMany(e =>
            {
                var currentObject = e.CurrentValues.ToObject();
                return e.CurrentValues.Properties.Select(p => (Property: p, Value: p.PropertyInfo.GetValue(currentObject))).Select(t => new ChangePropertyValue
                {
                    PropertyInfo = t.Property.PropertyInfo,
                    CurrentValue = t.Value,
                    EntityState = e.State,
                    Entity = e.Entity,
                    IsPrimaryKey = t.Property.IsPrimaryKey(),
                    IsForeignKey = t.Property.IsForeignKey(),
                    EntityType = e.OriginalValues.EntityType.ClrType
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
                return e.OriginalValues.Properties.Select(p => (Property: p, Value: p.PropertyInfo.GetValue(originalObject))).Select(t => new ChangePropertyValue
                {
                    PropertyInfo = t.Property.PropertyInfo,
                    OriginalValue = t.Value,
                    EntityState = e.State,
                    Entity = e.Entity,
                    IsPrimaryKey = t.Property.IsPrimaryKey(),
                    IsForeignKey = t.Property.IsForeignKey(),
                    EntityType = e.OriginalValues.EntityType.ClrType
                });
            });
        }

        /// <summary>
        /// 获取移除的实体信息
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<ChangePropertyValue> GetRemoved(this DbContext db)
        {
            return db.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).SelectMany(e =>
            {
                var originalObject = e.OriginalValues.ToObject();
                return e.OriginalValues.Properties.Select(p => (Property: p, Value: p.PropertyInfo.GetValue(originalObject))).Select(t => new ChangePropertyValue
                {
                    PropertyInfo = t.Property.PropertyInfo,
                    OriginalValue = t.Value,
                    EntityState = e.State,
                    Entity = e.Entity,
                    IsPrimaryKey = t.Property.IsPrimaryKey(),
                    IsForeignKey = t.Property.IsForeignKey(),
                    EntityType = e.OriginalValues.EntityType.ClrType
                });
            });
        }

        /// <summary>
        /// 获取所有的变更信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<ChangePropertyValue> GetAllChanges<T>(this DbContext db)
        {
            return GetChanges<T>(db).Union(GetAdded<T>(db)).Union(GetRemoved<T>(db));
        }

        /// <summary>
        /// 获取所有的变更信息
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<ChangePropertyValue> GetAllChanges(this DbContext db)
        {
            return GetChanges(db).Union(GetAdded(db)).Union(GetRemoved(db));
        }
    }
}