using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Masuit.Tools.Strings
{
    /// <summary>
    /// Json帮助类
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// 将对象转换成json数据
        /// </summary>
        /// <param name="obj">实例对象</param>
        /// <returns>json</returns>
        public static string GetJsonFromObject(this object obj)
        {
            string str;
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            Type type = obj.GetType();
            PropertyInfo property = type.GetProperty("Values");
            PropertyInfo info2 = type.GetProperty("Keys");
            if ((property != null) || (info2 != null))
            {
                ICollection is2 = (ICollection)property.GetValue(obj, null);
                ICollection is3 = (ICollection)info2.GetValue(obj, null);
                str = string.Empty;
                List<string> list = new List<string>();
                foreach (object obj2 in is3)
                {
                    list.Add(obj2.ToString());
                }
                int num = 0;
                str = string.Empty;
                foreach (object obj2 in is2)
                {
                    Type type2 = obj2.GetType();
                    if (((type2.ToString() == "System.String") || (type2.ToString() == "System.Int32")) || (type2.ToString() == "System.Boolean"))
                    {
                        if (obj2 is string)
                        {
                            str = str + ",\"" + list[num] + "\":\"" + obj2.ToString().Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", @"\n").Replace("\r", @"\r") + "\"";
                        }
                        else
                        {
                            str = str + ",\"" + list[num] + "\":" + obj2.ToString();
                        }
                    }
                    else
                    {
                        str = str + ",\"" + list[num] + "\":{" + tojson(obj2) + "}";
                    }
                    num++;
                }
                if (str.Length > 0)
                {
                    str = str.Substring(1);
                    builder.Append(str);
                }
            }
            else
            {
                str = string.Empty;
                if ((obj is int) || (obj is bool))
                {
                    str = str + ",\"" + type.Name + "\":" + obj.ToString();
                }
                else if (obj is string)
                {
                    str = str + ",\"" + type.Name + "\":\"" + obj.ToString().Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", @"\n").Replace("\r", @"\r") + "\"";
                }
                else
                {
                    foreach (PropertyInfo info3 in type.GetProperties())
                    {
                        object obj3 = info3.GetValue(obj, null);
                        Type type3 = obj3.GetType();
                        if ((obj3 is int) || (obj3 is bool))
                        {
                            str = str + ",\"" + info3.Name + "\":" + obj3.ToString();
                        }
                        else if (obj3 is string)
                        {
                            str = str + ",\"" + info3.Name + "\":\"" + obj3.ToString().Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", @"\n").Replace("\r", @"\r") + "\"";
                        }
                        else
                        {
                            str = str + ",\"" + info3.Name + "\":{" + tojson(obj3) + "}";
                        }
                    }
                }
                if (str.Length > 0)
                {
                    builder.Append(str.Substring(1));
                }
            }
            builder.Append("}");
            return builder.ToString();
        }

        private static string tojson(object obj)
        {
            string str;
            Type type = obj.GetType();
            string str2 = string.Empty;
            PropertyInfo property = type.GetProperty("Values");
            PropertyInfo info2 = type.GetProperty("Keys");
            if ((property != null) && (info2 != null))
            {
                ICollection is2 = (ICollection)info2.GetValue(obj, null);
                ICollection is3 = (ICollection)property.GetValue(obj, null);
                str = string.Empty;
                List<string> list = new List<string>();
                foreach (object obj2 in is2)
                {
                    list.Add(obj2.ToString());
                }
                int num = 0;
                str = string.Empty;
                foreach (object obj2 in is3)
                {
                    Type type2 = obj2.GetType();
                    if (((type2.ToString() == "System.String") || (type2.ToString() == "System.Int32")) || (type2.ToString() == "System.Boolean"))
                    {
                        if (obj2 is string)
                        {
                            str = str + ",\"" + list[num] + "\":\"" + obj2.ToString().Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", @"\n").Replace("\r", @"\r") + "\"";
                        }
                        else
                        {
                            str = str + ",\"" + list[num] + "\":" + obj2.ToString();
                        }
                    }
                    else
                    {
                        str = str + ",\"" + list[num] + "\":{" + tojson(obj2) + "}";
                    }
                    num++;
                }
                if (str.Length > 0)
                {
                    str = str.Substring(1);
                    str2 += str;
                }
                return str2;
            }
            str = string.Empty;
            if ((obj is int) || (obj is bool))
            {
                str = str + ",\"" + type.Name + "\":" + obj.ToString();
            }
            else if (obj is string)
            {
                str = str + ",\"" + type.Name + "\":\"" + obj.ToString().Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", @"\n").Replace("\r", @"\r") + "\"";
            }
            else
            {
                foreach (PropertyInfo info3 in type.GetProperties())
                {
                    object obj3 = info3.GetValue(obj, null);
                    Type type3 = obj3.GetType();
                    if ((obj3 is int) || (obj3 is bool))
                    {
                        str = str + ",\"" + info3.Name + "\":" + obj3.ToString();
                    }
                    else if (obj3 is string)
                    {
                        str = str + ",\"" + info3.Name + "\":\"" + obj3.ToString().Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", @"\n").Replace("\r", @"\r") + "\"";
                    }
                    else
                    {
                        str = str + ",\"" + info3.Name + "\":{" + tojson(obj3) + "}";
                    }
                }
            }
            if (str.Length > 0)
            {
                str2 += str.Substring(1);
            }
            return str2;
        }
    }
}
