using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Excel
{
    /// <summary>
    /// 表格列设置项
    /// </summary>
    public class ColumnSettings
    {
        internal readonly Dictionary<int, string> ColumnTypes = new();
        private readonly NumberFormater _numberFormater = new("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 1);

        /// <summary>
        /// 设置列格式
        /// </summary>
        /// <param name="index">列索引，从1开始</param>
        /// <param name="format">格式字符串，例如："¥"#,##0.00;"¥"\-#,##0.00</param>
        /// <returns></returns>
        public ColumnSettings SetColumnFormat(int index, string format)
        {
            ColumnTypes.Add(index, format);
            return this;
        }

        /// <summary>
        /// 设置列格式
        /// </summary>
        /// <param name="column">列索引，从A开始</param>
        /// <param name="format">格式字符串，例如："¥"#,##0.00;"¥"\-#,##0.00</param>
        /// <returns></returns>
        public ColumnSettings SetColumnFormat(string column, string format)
        {
            if (string.IsNullOrEmpty(column))
            {
                throw new ArgumentException("列索引不能为空");
            }

            column = string.Intern(column.ToUpper());
            if (column.Except(_numberFormater.Characters).Any())
            {
                throw new ArgumentException("列索引非法：" + column);
            }

            ColumnTypes.Add((int)_numberFormater.FromString(column), format);
            return this;
        }

        /// <summary>
        /// 设置列格式
        /// </summary>
        /// <param name="index">列索引，从1开始</param>
        /// <param name="format">格式字符串，例如："¥"#,##0.00;"¥"\-#,##0.00</param>
        /// <returns></returns>
        public ColumnSettings SetColumnFormat(int[] index, string format)
        {
            foreach (var i in index)
            {
                SetColumnFormat(i, format);
            }

            return this;
        }

        /// <summary>
        /// 设置列格式
        /// </summary>
        /// <param name="columns">列索引，从A开始</param>
        /// <param name="format">格式字符串，例如："¥"#,##0.00;"¥"\-#,##0.00</param>
        /// <returns></returns>
        public ColumnSettings SetColumnFormat(string[] columns, string format)
        {
            foreach (var i in columns)
            {
                SetColumnFormat(i, format);
            }

            return this;
        }
    }
}