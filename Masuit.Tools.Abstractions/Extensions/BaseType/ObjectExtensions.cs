using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Dynamics;
using Masuit.Tools.Reflection;
using Newtonsoft.Json.Linq;

#if NETSTANDARD2_1_OR_GREATER

using System.Text.Json;
using Masuit.Tools.Systems;
using JsonSerializer = System.Text.Json.JsonSerializer;

#endif

#if NET5_0_OR_GREATER

using System.Text.Json;
using Masuit.Tools.Systems;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

#endif

namespace Masuit.Tools;

/// <summary>
/// 对象扩展
/// </summary>
public static class ObjectExtensions
{
    private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

#if NET5_0_OR_GREATER
    /// <summary>
    /// System.Text.Json 默认配置 支持中文
    /// </summary>
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// System.Text.Json 支持中文/忽略null值
    /// </summary>
    private static readonly JsonSerializerOptions IgnoreNullJsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

#endif
    /// <summary>
    /// 是否是基本数据类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsPrimitive(this Type type)
    {
        if (type == typeof(string))
        {
            return true;
        }

        return type.IsValueType && type.IsPrimitive;
    }

    /// <summary>
    /// 判断类型是否是常见的简单类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsSimpleType(this Type type)
    {
        //IsPrimitive 判断是否为基础类型。
        //基元类型为 Boolean、 Byte、 SByte、 Int16、 UInt16、 Int32、 UInt32、 Int64、 UInt64、 IntPtr、 UIntPtr、 Char、 Double 和 Single。
        var t = Nullable.GetUnderlyingType(type) ?? type;
        return t.IsPrimitive || t.IsEnum || t == typeof(decimal) || t == typeof(string) || t == typeof(Guid) || t == typeof(TimeSpan) || t == typeof(Uri);
    }

    /// <summary>
    /// 是否是常见类型的 数组形式 类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsSimpleArrayType(this Type type)
    {
        return type.IsArray && Type.GetType(type.FullName!.Trim('[', ']')).IsSimpleType();
    }

    /// <summary>
    /// 是否是常见类型的 泛型形式 类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsSimpleListType(this Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsGenericType && type.GetGenericArguments().Length == 1 && type.GetGenericArguments().FirstOrDefault().IsSimpleType();
    }

    /// <summary>
    /// 是否是默认值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsDefaultValue(this object value)
    {
        if (value == default)
        {
            return true;
        }

        return value switch
        {
            byte s => s == 0,
            sbyte s => s == 0,
            short s => s == 0,
            char s => s == 0,
            bool s => s == false,
            ushort s => s == 0,
            int s => s == 0,
            uint s => s == 0,
            long s => s == 0,
            ulong s => s == 0,
            decimal s => s == 0,
            float s => s == 0,
            double s => s == 0,
            Enum s => Equals(s, Enum.GetValues(value.GetType()).GetValue(0)),
            DateTime s => s == DateTime.MinValue,
            DateTimeOffset s => s == DateTimeOffset.MinValue,
            Guid g => g == Guid.Empty,
            ValueType => Activator.CreateInstance(value.GetType()).Equals(value),
            _ => false
        };
    }

    /// <summary>
    /// 深克隆
    /// </summary>
    /// <param name="originalObject"></param>
    /// <param name="useJson">使用json方式</param>
    /// <returns></returns>
    public static object DeepClone(this object originalObject, bool useJson = false)
    {
        return useJson
            ? InternalJsonCopy(originalObject)
            : InternalCopy(originalObject, new Dictionary<object, object>(new ReferenceEqualityComparer()));
    }

    /// <summary>
    /// 深克隆
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="original"></param>
    /// <param name="useJson">使用json方式</param>
    /// <returns></returns>
    public static T DeepClone<T>(this T original, bool useJson = false)
    {
        return useJson ? InternalJsonCopy(original) : (T)InternalCopy(original, new Dictionary<object, object>(new ReferenceEqualityComparer()));
    }

#if NETSTANDARD2_1_OR_GREATER
    private static T InternalJsonCopy<T>(T obj)
    {
        using var stream = new PooledMemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        JsonSerializer.Serialize(writer, obj);
        writer.Flush();
        var reader = new Utf8JsonReader(stream.ToArray());
        return JsonSerializer.Deserialize<T>(ref reader);
    }
#else

    private static T InternalJsonCopy<T>(T obj)
    {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
    }

#endif

    private static object InternalCopy(object originalObject, IDictionary<object, object> visited)
    {
        if (originalObject == null)
        {
            return null;
        }

        var typeToReflect = originalObject.GetType();
        if (IsPrimitive(typeToReflect))
        {
            return originalObject;
        }

        if (visited.TryGetValue(originalObject, out var copy))
        {
            return copy;
        }

        if (typeof(Delegate).IsAssignableFrom(typeToReflect))
        {
            return null;
        }

        var cloneObject = CloneMethod.Invoke(originalObject, null);
        if (typeToReflect.IsArray)
        {
            var arrayType = typeToReflect.GetElementType();
            if (!IsPrimitive(arrayType))
            {
                Array clonedArray = (Array)cloneObject;
                clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
            }
        }

        visited.Add(originalObject, cloneObject);
        CopyFields(originalObject, visited, cloneObject, typeToReflect);
        RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
        return cloneObject;
    }

    private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
    {
        if (typeToReflect.BaseType != null)
        {
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
            CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
        }
    }

    private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, IReflect typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
    {
        foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
        {
            if (filter != null && !filter(fieldInfo))
            {
                continue;
            }

            if (IsPrimitive(fieldInfo.FieldType) || fieldInfo.IsInitOnly)
            {
                continue;
            }

            var originalFieldValue = fieldInfo.GetValue(originalObject);
            var clonedFieldValue = InternalCopy(originalFieldValue, visited);
            fieldInfo.SetValue(cloneObject, clonedFieldValue);
        }
    }

    /// <summary>
    /// 判断是否为null，null或0长度都返回true
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty<T>(this T value)
        where T : class
    {
        return value switch
        {
            null => true,
            string s => string.IsNullOrWhiteSpace(s),
            IEnumerable list => !list.GetEnumerator().MoveNext(),
            _ => false
        };
    }

    /// <summary>
    /// 转成非null
    /// </summary>
    /// <param name="s"></param>
    /// <param name="value">为空时的替换值</param>
    /// <returns></returns>
    public static T IfNull<T>(this T s, in T value)
    {
        return s ?? value;
    }

    /// <summary>
    /// 转换成json字符串
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="setting"></param>
    /// <returns></returns>
    public static string ToJsonString(this object obj, JsonSerializerSettings setting = null)
    {
        if (obj == null) return string.Empty;
        return JsonConvert.SerializeObject(obj, setting);
    }

    /// <summary>
    /// json反序列化成对象
    /// </summary>
    public static T FromJson<T>(this string json, JsonSerializerSettings setting = null)
    {
        return string.IsNullOrEmpty(json) ? default : JsonConvert.DeserializeObject<T>(json, setting);
    }

    #region System.Text.Json

    #if NET5_0_OR_GREATER
    
    /// <summary>
    /// 转换成json字符串
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="setting"></param>
    /// <returns></returns>
    public static string ToJson(this object obj, JsonSerializerOptions setting = null)
    {
        if (obj == null) return string.Empty;
        setting ??= DefaultJsonSerializerOptions;
        return JsonSerializer.Serialize(obj,setting);
    }
    
    /// <summary>
    /// 转换成json字符串并忽略Null值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="setting"></param>
    /// <returns></returns>
    public static string ToJsonIgnoreNull(this object obj)
    {
        if (obj == null) return string.Empty;
        return JsonSerializer.Serialize(obj,IgnoreNullJsonSerializerOptions);
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="json"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static T ToObject<T>(this string json, JsonSerializerOptions settings = null)
    {
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, settings);
    }

    #endif

    #endregion


    /// <summary>
    /// 链式操作
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    public static T2 Next<T1, T2>(this T1 source, Func<T1, T2> action)
    {
        return action(source);
    }

    /// <summary>
    /// 将对象转换成字典
    /// </summary>
    /// <param name="value"></param>
    public static Dictionary<string, object> ToDictionary(this object value)
    {
        var dictionary = new Dictionary<string, object>();
        if (value != null)
        {
            if (value is IDictionary dic)
            {
                foreach (DictionaryEntry e in dic)
                {
                    dictionary.Add(e.Key.ToString(), e.Value);
                }
                return dictionary;
            }

            foreach (var property in value.GetType().GetProperties())
            {
                var obj = property.GetValue(value, null);
                dictionary.Add(property.Name, obj);
            }
        }

        return dictionary;
    }

    /// <summary>
    /// 将对象转换成字典
    /// </summary>
    /// <param name="value"></param>
    public static Dictionary<string, string> ToDictionary(this JObject value)
    {
        var dictionary = new Dictionary<string, string>();
        if (value != null)
        {
            using var enumerator = value.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var obj = enumerator.Current.Value ?? string.Empty;
                dictionary.Add(enumerator.Current.Key, obj + string.Empty);
            }
        }

        return dictionary;
    }

    /// <summary>
    /// 对象转换成动态类型
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static dynamic ToDynamic(this object obj)
    {
        return DynamicFactory.WithObject(obj);
    }

    /// <summary>
    /// 多个对象的属性值合并
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="others"></param>
    public static T Merge<T>(this T a, T b, params T[] others) where T : class
    {
        foreach (var item in new[] { b }.Concat(others))
        {
            var dic = item.ToDictionary();
            foreach (var p in dic.Where(p => a.GetProperty(p.Key).IsDefaultValue()))
            {
                a.SetProperty(p.Key, p.Value);
            }
        }

        return a;
    }

    /// <summary>
    /// 多个对象的属性值合并
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static T Merge<T>(this IEnumerable<T> objects) where T : class
    {
        var list = objects as List<T> ?? objects.ToList();
        switch (list.Count)
        {
            case 0:
                return null;

            case 1:
                return list[0];
        }

        foreach (var item in list.Skip(1))
        {
            var dic = item.ToDictionary();
            foreach (var p in dic.Where(p => list[0].GetProperty(p.Key).IsDefaultValue()))
            {
                list[0].SetProperty(p.Key, p.Value);
            }
        }

        return list[0];
    }
}

internal class ReferenceEqualityComparer : EqualityComparer<object>
{
    public override bool Equals(object x, object y)
    {
        return ReferenceEquals(x, y);
    }

    public override int GetHashCode(object obj)
    {
        return obj is null ? 0 : obj.GetHashCode();
    }
}

internal static class ArrayExtensions
{
    public static void ForEach(this Array array, Action<Array, int[]> action)
    {
        if (array.LongLength == 0)
        {
            return;
        }

        ArrayTraverse walker = new ArrayTraverse(array);
        do action(array, walker.Position);
        while (walker.Step());
    }

    internal class ArrayTraverse
    {
        public int[] Position;
        private readonly int[] _maxLengths;

        public ArrayTraverse(Array array)
        {
            _maxLengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; ++i)
            {
                _maxLengths[i] = array.GetLength(i) - 1;
            }
            Position = new int[array.Rank];
        }

        public bool Step()
        {
            for (int i = 0; i < Position.Length; ++i)
            {
                if (Position[i] < _maxLengths[i])
                {
                    Position[i]++;
                    for (int j = 0; j < i; j++)
                    {
                        Position[j] = 0;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
