using System;
using System.Data;
using System.Reflection;

namespace Masuit.Tools.Database
{
    /// <summary>
    /// SqlDataReader扩展类
    /// </summary>
    public static class DataExt
    {
        /// <summary>
        /// 根据DataRow映射到实体模型
        /// </summary>
        /// <typeparam name="T">实体模型</typeparam>
        /// <param name="row">数据行</param>
        /// <returns>映射后的实体模型</returns>
        public static T MapEntity<T>(this DataRow row) where T : class
        {
            T obj = Assembly.GetAssembly(typeof(T)).CreateInstance(typeof(T).FullName) as T;
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo p in properties)
            {
                p.SetValue(obj, row[p.Name]);
            }

            return obj;
        }

        /// <summary>
        /// 根据DataReader映射到实体模型
        /// </summary>
        /// <typeparam name="T">实体模型</typeparam>
        /// <param name="dr">IDataReader</param>
        /// <returns>映射后的实体模型</returns>
        public static T MapEntity<T>(this IDataReader dr) where T : class
        {
            T obj = Assembly.GetAssembly(typeof(T)).CreateInstance(typeof(T).FullName) as T;
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo p in properties)
            {
                p.SetValue(obj, dr[p.Name]);
            }

            return obj;
        }
    }
}