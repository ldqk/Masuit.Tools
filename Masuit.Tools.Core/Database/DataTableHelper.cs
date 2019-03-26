using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Masuit.Tools.Core.Database
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
            List<T> list = new List<T>();
            using (dt)
            {
                if (dt == null || dt.Rows.Count == 0)
                {
                    return list;
                }

                DataTableBuilder<T> eblist = DataTableBuilder<T>.CreateBuilder(dt.Rows[0]);
                foreach (DataRow info in dt.Rows)
                {
                    list.Add(eblist.Build(info));
                }

                return list;
            }
        }

        /// <summary>
        /// 将泛型集合类转换成DataTable
        /// </summary>
        /// <typeparam name="T">集合项类型</typeparam>
        /// <param name="list">集合</param>
        /// <returns>数据集(表)</returns>
        public static DataTable ToDataTable<T>(this IList<T> list)
        {
            return ToDataTable(list, null);
        }

        /// <summary>
        /// 将泛型集合类转换成DataTable
        /// </summary>
        /// <typeparam name="T">集合项类型</typeparam>
        /// <param name="list">集合</param>
        /// <param name="propertyName">需要返回的列的列名</param>
        /// <returns>数据集(表)</returns>
        public static DataTable ToDataTable<T>(this IList<T> list, params string[] propertyName)
        {
            List<string> propertyNameList = new List<string>();
            if (propertyName != null)
            {
                propertyNameList.AddRange(propertyName);
            }

            DataTable result = new DataTable();
            if (list.Count <= 0)
            {
                return result;
            }

            var propertys = list[0].GetType().GetProperties();
            propertys.ForEach(pi =>
            {
                if (propertyNameList.Count == 0)
                {
                    result.Columns.Add(pi.Name, pi.PropertyType);
                }
                else
                {
                    if (propertyNameList.Contains(pi.Name))
                    {
                        result.Columns.Add(pi.Name, pi.PropertyType);
                    }
                }
            });
            list.ForEach(item =>
            {
                ArrayList tempList = new ArrayList();
                foreach (PropertyInfo pi in propertys)
                {
                    if (propertyNameList.Count == 0)
                    {
                        object obj = pi.GetValue(item, null);
                        tempList.Add(obj);
                    }
                    else
                    {
                        if (propertyNameList.Contains(pi.Name))
                        {
                            object obj = pi.GetValue(item, null);
                            tempList.Add(obj);
                        }
                    }
                }

                object[] array = tempList.ToArray();
                result.LoadDataRow(array, true);
            });

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
                if (!string.IsNullOrEmpty(item))
                {
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
            }

            return dt;
        }

        /// <summary>
        /// 根据类型名返回一个Type类型
        /// </summary>
        /// <param name="typeName">类型的名称</param>
        /// <returns>Type对象</returns>
        private static Type ConvertType(string typeName)
        {
            typeName = typeName.ToLower().Replace("system.", "");
            Type newType = typeof(string);
            switch (typeName)
            {
                case "boolean":
                case "bool":
                    newType = typeof(bool);
                    break;
                case "int16":
                case "short":
                    newType = typeof(short);
                    break;
                case "int32":
                case "int":
                    newType = typeof(int);
                    break;
                case "long":
                case "int64":
                    newType = typeof(long);
                    break;
                case "uint16":
                case "ushort":
                    newType = typeof(ushort);
                    break;
                case "uint32":
                case "uint":
                    newType = typeof(uint);
                    break;
                case "uint64":
                case "ulong":
                    newType = typeof(ulong);
                    break;
                case "single":
                case "float":
                    newType = typeof(float);
                    break;
                case "string":
                    newType = typeof(string);
                    break;
                case "guid":
                    newType = typeof(Guid);
                    break;
                case "decimal":
                    newType = typeof(decimal);
                    break;
                case "double":
                    newType = typeof(double);
                    break;
                case "datetime":
                    newType = typeof(DateTime);
                    break;
                case "byte":
                    newType = typeof(byte);
                    break;
                case "char":
                    newType = typeof(char);
                    break;
            }

            return newType;
        }

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

            DataTable dt = rows[0].Table.Clone();
            dt.DefaultView.Sort = rows[0].Table.DefaultView.Sort;
            foreach (var t in rows)
            {
                dt.LoadDataRow(t.ItemArray, true);
            }

            return dt;
        }
    }
}