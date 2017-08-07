using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
        /// <exception cref="DuplicateNameException">The collection already has a column with the specified name. (The comparison is not case-sensitive.) </exception>
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
        public static bool IsHaveRows(this DataTable dt)
        {
            if (dt?.Rows.Count > 0)
                return true;

            return false;
        }

        /// <summary>
        /// DataTable转换成实体列表
        /// </summary>
        /// <typeparam name="T">实体 T </typeparam>
        /// <param name="table">datatable</param>
        /// <returns>强类型的数据集合</returns>
        public static IList<T> DataTableToList<T>(this DataTable table) where T : class
        {
            if (!IsHaveRows(table))
                return new List<T>();

            IList<T> list = new List<T>();
            foreach (DataRow dr in table.Rows)
            {
                var model = Activator.CreateInstance<T>();

                foreach (DataColumn dc in dr.Table.Columns)
                {
                    object drValue = dr[dc.ColumnName];
                    PropertyInfo pi = model.GetType().GetProperty(dc.ColumnName);

                    if (pi != null && pi.CanWrite && (drValue != null && !Convert.IsDBNull(drValue)))
                    {
                        pi.SetValue(model, drValue, null);
                    }
                }
                list.Add(model);
            }
            return list;
        }

        /// <summary>
        /// 实体列表转换成DataTable
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="list"> 实体列表</param>
        /// <returns>映射为数据表</returns>
        /// <exception cref="OverflowException">The array is multidimensional and contains more than <see cref="F:System.Int32.MaxValue" /> elements.</exception>
        public static DataTable ListToDataTable<T>(this IList<T> list) where T : class
        {
            if (list == null || list.Count <= 0)
            {
                return null;
            }
            var dt = new DataTable(typeof(T).Name);
            PropertyInfo[] myPropertyInfo = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int length = myPropertyInfo.Length;
            bool createColumn = true;
            foreach (T t in list)
            {
                if (t == null)
                {
                    continue;
                }
                var row = dt.NewRow();
                for (int i = 0; i < length; i++)
                {
                    PropertyInfo pi = myPropertyInfo[i];
                    string name = pi.Name;
                    if (createColumn)
                    {
                        var column = new DataColumn(name, pi.PropertyType);
                        dt.Columns.Add(column);
                    }
                    row[name] = pi.GetValue(t, null);
                }
                if (createColumn)
                {
                    createColumn = false;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        /// <summary>
        /// 将泛型集合类转换成DataTable
        /// </summary>
        /// <typeparam name="T">集合项类型</typeparam>
        /// <param name="list">集合</param>
        /// <returns>数据集(表)</returns>
        public static DataTable ToDataTable<T>(this IList<T> list)
        {
            return ToDataTable<T>(list, null);
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
                propertyNameList.AddRange(propertyName);
            DataTable result = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
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
            }
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
                return null;
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
            DataRow[] drs = new DataRow[count];
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
            for (int i = 0; i < rows.Length; i++)
            {
                dt.LoadDataRow(rows[i].ItemArray, true);
            }
            return dt;
        }

        /// <summary>
        /// 排序表的视图
        /// </summary>
        /// <param name="dt">原内存表</param>
        /// <param name="sorts">排序方式</param>
        /// <returns>排序后的内存表</returns>
        public static DataTable SortedTable(this DataTable dt, params string[] sorts)
        {
            if (dt.Rows.Count > 0)
            {
                string tmp = "";
                for (int i = 0; i < sorts.Length; i++)
                {
                    tmp += sorts[i] + ",";
                }
                dt.DefaultView.Sort = tmp.TrimEnd(',');
            }
            return dt;
        }

        /// <summary>
        /// 根据条件过滤表的内容
        /// </summary>
        /// <param name="dt">原内存表</param>
        /// <param name="condition">过滤条件</param>
        /// <returns>过滤后的内存表</returns>
        public static DataTable FilterDataTable(this DataTable dt, string condition)
        {
            if (condition.Trim().Length == 0)
            {
                return dt;
            }
            var newdt = dt.Clone();
            DataRow[] dr = dt.Select(condition);
            dr.ForEach(t => newdt.ImportRow(t));
            return newdt;
        }
    }
}
