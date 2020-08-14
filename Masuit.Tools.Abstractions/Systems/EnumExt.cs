using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Masuit.Tools.Systems
{
    /// <summary>
    /// 枚举扩展类
    /// </summary>
    public static partial class EnumExt
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<int, string>> EnumNameValueDict = new ConcurrentDictionary<Type, Dictionary<int, string>>();
        private static readonly ConcurrentDictionary<Type, Dictionary<string, int>> EnumValueNameDict = new ConcurrentDictionary<Type, Dictionary<string, int>>();
        private static ConcurrentDictionary<string, Type> _enumTypeDict;

        /// <summary>
        /// 获取枚举对象Key与显示名称的字典
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static Dictionary<int, string> GetDictionary(this Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new Exception("给定的类型不是枚举类型");
            }

            var names = EnumNameValueDict.ContainsKey(enumType) ? EnumNameValueDict[enumType] : new Dictionary<int, string>();
            if (names.Count == 0)
            {
                names = GetDictionaryItems(enumType);
                EnumNameValueDict[enumType] = names;
            }

            return names;
        }

        private static Dictionary<int, string> GetDictionaryItems(Type enumType)
        {
            var enumItems = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            var names = new Dictionary<int, string>(enumItems.Length);
            foreach (FieldInfo enumItem in enumItems)
            {
                int intValue = (int)enumItem.GetValue(enumType);
                names[intValue] = enumItem.Name;
            }

            return names;
        }

        /// <summary>
        /// 获取枚举对象显示名称与Key的字典
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static Dictionary<string, int> GetValueItems(this Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new Exception("给定的类型不是枚举类型");
            }

            var values = EnumValueNameDict.ContainsKey(enumType) ? EnumValueNameDict[enumType] : new Dictionary<string, int>();
            if (values.Count == 0)
            {
                values = GetValueNameItems(enumType);
                EnumValueNameDict[enumType] = values;
            }

            return values;
        }

        private static Dictionary<string, int> GetValueNameItems(Type enumType)
        {
            var enumItems = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            var values = new Dictionary<string, int>(enumItems.Length);
            foreach (var enumItem in enumItems)
            {
                values[enumItem.Name] = (int)enumItem.GetValue(enumType);
            }

            return values;
        }

        /// <summary>
        /// 获取枚举对象的值内容
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetValue(this Type enumType, string name)
        {
            if (!enumType.IsEnum)
            {
                throw new Exception("给定的类型不是枚举类型");
            }

            Dictionary<string, int> enumDict = GetValueNameItems(enumType);
            return enumDict.ContainsKey(name) ? enumDict[name] : enumDict.Select(d => d.Value).FirstOrDefault();
        }

        /// <summary>
        /// 获取枚举类型
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetEnumType(Assembly assembly, string typeName)
        {
            _enumTypeDict ??= LoadEnumTypeDict(assembly);
            return _enumTypeDict.ContainsKey(typeName) ? _enumTypeDict[typeName] : null;
        }

        /// <summary>
        /// 根据枚举成员获取Display的属性Name
        /// </summary>
        /// <returns></returns>
        public static string GetDisplay(this Enum en)
        {
            var type = en.GetType(); //获取类型
            var memberInfos = type.GetMember(en.ToString()); //获取成员
            if (memberInfos.Any())
            {
                if (memberInfos[0].GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] attrs && attrs.Length > 0)
                {
                    return attrs[0].Name; //返回当前描述
                }
            }

            return en.ToString();
        }

        private static ConcurrentDictionary<string, Type> LoadEnumTypeDict(Assembly assembly)
        {
            return new ConcurrentDictionary<string, Type>(assembly.GetTypes().Where(o => o.IsEnum).ToDictionary(o => o.Name, o => o));
        }

        /// <summary>
        /// 根据枚举成员获取自定义属性EnumDisplayNameAttribute的属性DisplayName
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, int> GetDescriptionAndValue(this Type enumType)
        {
            return Enum.GetValues(enumType).Cast<object>().ToDictionary(e => GetDescription(e as Enum), e => (int)e);
        }

        /// <summary>
        /// 根据枚举成员获取DescriptionAttribute的属性Description
        /// </summary>
        /// <returns></returns>
        public static string GetDescription(this Enum en)
        {
            var type = en.GetType(); //获取类型
            var memberInfos = type.GetMember(en.ToString()); //获取成员
            if (memberInfos.Any())
            {
                if (memberInfos[0].GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attrs && attrs.Length > 0)
                {
                    return attrs[0].Description; //返回当前描述
                }
            }

            return en.ToString();
        }

        /// <summary>
        /// 扩展方法：根据枚举值得到相应的枚举定义字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static String ToEnumString(this int value, Type enumType)
        {
            return GetEnumStringFromEnumValue(enumType)[value.ToString()];
        }

        /// <summary>
        /// 根据枚举类型得到其所有的 值 与 枚举定义字符串 的集合
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static NameValueCollection GetEnumStringFromEnumValue(Type enumType)
        {
            var nvc = new NameValueCollection();
            var fields = enumType.GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsEnum)
                {
                    var strValue = ((int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null)).ToString();
                    nvc.Add(strValue, field.Name);
                }
            }

            return nvc;
        }
    }
}