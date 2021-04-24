using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Masuit.Tools.Database
{
    /// <summary>
    /// DataTable帮助类
    /// </summary>
    public static class DataTableHelper
    {
        /// <summary>
        /// 给DataTable增加一个自增列
        /// 如果DataTable 存在 identityid 字段  则 直接返回DataTable 不做任何处理
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <returns>返回Datatable 增加字段 identityid </returns>
        public static DataTable AddIdentityColumn(this DataTable dt)
        {
            if (!dt.Columns.Contains("identityid"))
            {
                dt.Columns.Add("identityid");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["identityid"] = (i + 1).ToString();
                }
            }

            return dt;
        }

        /// <summary>
        /// 检查DataTable 是否有数据行
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <returns>是否有数据行</returns>
        public static bool HasRows(this DataTable dt)
        {
            return dt.Rows.Count > 0;
        }

        /// <summary>
        /// datatable转List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this DataTable dt) where T : class, new()
        {
            var list = new List<T>();
            if (dt == null || dt.Rows.Count == 0)
            {
                return list;
            }

            list.AddRange(dt.Rows.Cast<DataRow>().Select(info => DataTableBuilder<T>.CreateBuilder(dt.Rows[0]).Build(info)));
            return list;
        }

        /// <summary>
        /// 将泛型集合类转换成DataTable
        /// </summary>
        /// <typeparam name="T">集合项类型</typeparam>
        /// <param name="list">集合</param>
        /// <param name="tableName">表名</param>
        /// <returns>数据集(表)</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> list, string tableName = null)
        {
            return ToDataTable(list.ToList(), tableName);
        }

        /// <summary>
        /// 将泛型集合类转换成DataTable
        /// </summary>
        /// <typeparam name="T">集合项类型</typeparam>
        /// <param name="list">集合</param>
        /// <param name="tableName">表名</param>
        /// <returns>数据集(表)</returns>
        public static DataTable ToDataTable<T>(this IList<T> list, string tableName = null)
        {
            var result = new DataTable(tableName);
            if (list.Count <= 0)
            {
                return result;
            }

            var properties = list[0].GetType().GetProperties();
            result.Columns.AddRange(properties.Select(p =>
            {
                if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return new DataColumn(p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? p.Name, Nullable.GetUnderlyingType(p.PropertyType));
                }

                return new DataColumn(p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? p.Name, p.PropertyType);
            }).ToArray());
            list.ForEach(item => result.LoadDataRow(properties.Select(p => p.GetValue(item)).ToArray(), true));
            return result;
        }

        /// <summary>
        /// 根据nameList里面的字段创建一个表格,返回该表格的DataTable
        /// </summary>
        /// <param name="nameList">包含字段信息的列表</param>
        /// <returns>DataTable</returns>
        public static DataTable CreateTable(this List<string> nameList)
        {
            if (nameList.Count <= 0)
            {
                return null;
            }

            var myDataTable = new DataTable();
            foreach (string columnName in nameList)
            {
                myDataTable.Columns.Add(columnName, typeof(string));
            }

            return myDataTable;
        }

        /// <summary>
        /// 通过字符列表创建表字段，字段格式可以是：<br/>
        /// 1) a,b,c,d,e<br/>
        /// 2) a|int,b|string,c|bool,d|decimal<br/>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="nameString">字符列表</param>
        /// <returns>内存表</returns>
        public static DataTable CreateTable(this DataTable dt, string nameString)
        {
            string[] nameArray = nameString.Split(',', ';');
            foreach (string item in nameArray)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                string[] subItems = item.Split('|');
                if (subItems.Length == 2)
                {
                    dt.Columns.Add(subItems[0], ConvertType(subItems[1]));
                }
                else
                {
                    dt.Columns.Add(subItems[0]);
                }
            }

            return dt;
        }

        /// <summary>
        /// 根据类型名返回一个Type类型
        /// </summary>
        /// <param name="typeName">类型的名称</param>
        /// <returns>Type对象</returns>
        private static Type ConvertType(string typeName) => typeName.ToLower().Replace("system.", "") switch
        {
            "boolean" => typeof(bool),
            "bool" => typeof(bool),
            "int16" => typeof(short),
            "short" => typeof(short),
            "int32" => typeof(int),
            "int" => typeof(int),
            "long" => typeof(long),
            "int64" => typeof(long),
            "uint16" => typeof(ushort),
            "ushort" => typeof(ushort),
            "uint32" => typeof(uint),
            "uint" => typeof(uint),
            "uint64" => typeof(ulong),
            "ulong" => typeof(ulong),
            "single" => typeof(float),
            "float" => typeof(float),
            "string" => typeof(string),
            "guid" => typeof(Guid),
            "decimal" => typeof(decimal),
            "double" => typeof(double),
            "datetime" => typeof(DateTime),
            "byte" => typeof(byte),
            "char" => typeof(char),
            _ => typeof(string)
        };

        /// <summary>
        /// 获得从DataRowCollection转换成的DataRow数组
        /// </summary>
        /// <param name="drc">DataRowCollection</param>
        /// <returns>DataRow数组</returns>
        public static DataRow[] GetDataRowArray(this DataRowCollection drc)
        {
            int count = drc.Count;
            var drs = new DataRow[count];
            for (int i = 0; i < count; i++)
            {
                drs[i] = drc[i];
            }

            return drs;
        }

        /// <summary>
        /// 将DataRow数组转换成DataTable，注意行数组的每个元素须具有相同的数据结构，
        /// 否则当有元素长度大于第一个元素时，抛出异常
        /// </summary>
        /// <param name="rows">行数组</param>
        /// <returns>将内存行组装成内存表</returns>
        public static DataTable GetTableFromRows(this DataRow[] rows)
        {
            if (rows.Length <= 0)
            {
                return new DataTable();
            }

            var dt = rows[0].Table.Clone();
            dt.DefaultView.Sort = rows[0].Table.DefaultView.Sort;
            foreach (var t in rows)
            {
                dt.LoadDataRow(t.ItemArray, true);
            }

            return dt;
        }
    }
}