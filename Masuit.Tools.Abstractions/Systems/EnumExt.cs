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
        /// 获取枚举类型
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetEnumType(this Assembly assembly, string typeName)
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
        /// 获取枚举值的Description信息
        /// </summary>
        /// <param name ="value">枚举值</param>
        /// <param name ="args">要格式化的对象</param>
        /// <returns>如果未找到DescriptionAttribute则返回null或返回类型描述</returns>
        public static string GetDescription(this Enum value, params object[] args)
        {
            var type = value.GetType();
            if (!Enum.IsDefined(type, value))
            {
                return Enum.GetValues(type).OfType<Enum>().Where(value.HasFlag).Select(e =>
                {
                    var member = type.GetField(e.ToString());
                    var description = member.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attrs && attrs.Length != 0 ? attrs[0].Description : e.ToString();
                    return args.Length > 0 ? string.Format(description, args) : description;
                }).Join(",");
            }

            var member = type.GetField(value.ToString());
            var description = member.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Length != 0 ? attributes[0].Description : value.ToString();
            return args.Length > 0 ? string.Format(description, args) : description;
        }

        /// <summary>
        /// 获取枚举值的Description信息
        /// </summary>
        /// <param name ="value">枚举值</param>
        /// <param name ="args">要格式化的对象</param>
        /// <returns>如果未找到DescriptionAttribute则返回null或返回类型描述</returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var type = value.GetType();
            if (!Enum.IsDefined(type, value))
            {
                return Enum.GetValues(type).OfType<Enum>().Where(value.HasFlag).SelectMany(e => type.GetField(e.ToString()).GetCustomAttributes<TAttribute>(false));
            }

            return type.GetField(value.ToString()).GetCustomAttributes<TAttribute>(false);
        }

        /// <summary>
        /// 拆分枚举值
        /// </summary>
        /// <param name ="value">枚举值</param>
        public static IEnumerable<TEnum> Split<TEnum>(this TEnum value) where TEnum : Enum
        {
            var type = typeof(TEnum);
            return Enum.IsDefined(type, value) ? new[] { value } : Enum.GetValues(type).Cast<TEnum>().Where(e => value.HasFlag(e));
        }

        /// <summary>
        /// 获取枚举值的Description信息
        /// </summary>
        /// <param name ="value">枚举值</param>
        /// <returns>如果未找到DescriptionAttribute则返回null或返回类型描述</returns>
        public static EnumDescriptionAttribute GetEnumDescription(this Enum value)
        {
            return GetEnumDescriptions(value).FirstOrDefault();
        }

        /// <summary>
        /// 获取枚举值的Description信息
        /// </summary>
        /// <param name ="value">枚举值</param>
        /// <returns>如果未找到DescriptionAttribute则返回null或返回类型描述</returns>
        public static NullableDictionary<string, (string Description, string Display)> GetTypedEnumDescriptions(this Enum value)
        {
            return GetEnumDescriptions(value).ToDictionarySafety(a => a.Language, a => (a.Description, a.Display));
        }

        /// <summary>
        /// 获取枚举值的Description信息
        /// </summary>
        /// <param name ="value">枚举值</param>
        /// <returns>如果未找到DescriptionAttribute则返回null或返回类型描述</returns>
        public static IEnumerable<EnumDescriptionAttribute> GetEnumDescriptions(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var type = value.GetType();
            if (!Enum.IsDefined(type, value))
            {
                return Enum.GetValues(type).OfType<Enum>().Where(value.HasFlag).SelectMany(e => type.GetField(e.ToString()).GetCustomAttributes(typeof(EnumDescriptionAttribute), false).OfType<EnumDescriptionAttribute>());
            }

            return type.GetField(value.ToString()).GetCustomAttributes(typeof(EnumDescriptionAttribute), false).OfType<EnumDescriptionAttribute>();
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
