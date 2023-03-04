using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Masuit.Tools.Reflection
{
    /// <summary>
    /// 反射操作辅助类，如获取或设置字段、属性的值等反射信息。
    /// </summary>
    public static class ReflectionUtil
    {
        #region 属性字段设置

        public static BindingFlags bf = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <param name="methodName">方法名，区分大小写</param>
        /// <param name="args">方法参数</param>
        /// <typeparam name="T">约束返回的T必须是引用类型</typeparam>
        /// <returns>T类型</returns>
        public static T InvokeMethod<T>(this object obj, string methodName, object[] args)
        {
            return (T)obj.GetType().GetMethod(methodName, args.Select(o => o.GetType()).ToArray()).Invoke(obj, args);
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <param name="methodName">方法名，区分大小写</param>
        /// <param name="args">方法参数</param>
        /// <returns>T类型</returns>
        public static void InvokeMethod(this object obj, string methodName, object[] args)
        {
            var type = obj.GetType();
            type.GetMethod(methodName, args.Select(o => o.GetType()).ToArray()).Invoke(obj, args);
        }

        /// <summary>
        /// 设置字段
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <param name="name">字段名</param>
        /// <param name="value">值</param>
        public static void SetField<T>(this T obj, string name, object value) where T : class
        {
            SetProperty(obj, name, value);
        }

        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <param name="name">字段名</param>
        /// <typeparam name="T">约束返回的T必须是引用类型</typeparam>
        /// <returns>T类型</returns>
        public static T GetField<T>(this object obj, string name)
        {
            return GetProperty<T>(obj, name);
        }

        /// <summary>
        /// 获取所有的字段信息
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <returns>字段信息</returns>
        public static FieldInfo[] GetFields(this object obj)
        {
            FieldInfo[] fieldInfos = obj.GetType().GetFields(bf);
            return fieldInfos;
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <param name="name">属性名</param>
        /// <param name="value">值</param>
        public static string SetProperty<T>(this T obj, string name, object value) where T : class
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.PropertyOrField(parameter, name);
            var before = Expression.Lambda(property, parameter).Compile().DynamicInvoke(obj);
            if (value == before)
            {
                return value?.ToString();
            }

            if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                typeof(T).GetProperty(name)?.SetValue(obj, value);
            }
            else
            {
                var valueExpression = Expression.Parameter(property.Type, "v");
                var assign = Expression.Assign(property, valueExpression);
                Expression.Lambda(assign, parameter, valueExpression).Compile().DynamicInvoke(obj, value);
            }

            return before.ToJsonString();
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <param name="name">属性名</param>
        /// <typeparam name="T">约束返回的T必须是引用类型</typeparam>
        /// <returns>T类型</returns>
        public static T GetProperty<T>(this object obj, string name)
        {
            var parameter = Expression.Parameter(obj.GetType(), "e");
            var property = Expression.PropertyOrField(parameter, name);
            return (T)Expression.Lambda(property, parameter).Compile().DynamicInvoke(obj);
        }

        /// <summary>
        /// 获取所有的属性信息
        /// </summary>
        /// <param name="obj">反射对象</param>
        /// <returns>属性信息</returns>
        public static PropertyInfo[] GetProperties(this object obj)
        {
            PropertyInfo[] propertyInfos = obj.GetType().GetProperties(bf);
            return propertyInfos;
        }

        #endregion 属性字段设置

        #region 获取Description

        /// <summary>
        /// 获取枚举值的Description信息
        /// </summary>
        /// <param name ="value">枚举值</param>
        /// <param name ="args">要格式化的对象</param>
        /// <returns>如果未找到DescriptionAttribute则返回null或返回类型描述</returns>
        public static string GetDescription(this Enum value, params object[] args)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var type = value.GetType();
            if (!Enum.IsDefined(type, value))
            {
                return value.ToString();
            }

            FieldInfo fi = type.GetField(value.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            var text = attributes.Length > 0 ? attributes[0].Description : value.ToString();
            if (args is { Length: > 0 })
            {
                return string.Format(text, args);
            }

            return text;
        }

        /// <summary>
        ///	根据成员信息获取Description信息
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <returns>如果未找到DescriptionAttribute则返回null或返回类型描述</returns>
        public static string GetDescription(this MemberInfo member)
        {
            return GetDescription(member, null);
        }

        /// <summary>
        /// 根据成员信息获取Description信息
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <param name="args">格式化占位对象</param>
        /// <returns>如果未找到DescriptionAttribute则返回null或返回类型描述</returns>
        public static string GetDescription(this MemberInfo member, params object[] args)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            return member.IsDefined(typeof(DescriptionAttribute), false) ? member.GetAttribute<DescriptionAttribute>().Description : string.Empty;
        }

        #endregion 获取Description

        /// <summary>
        /// 获取对象的Attribute
        /// </summary>
        /// <returns></returns>
        public static T GetAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute
        {
            var attributes = provider.GetCustomAttributes(typeof(T), true);
            return attributes.Length > 0 ? attributes[0] as T : null;
        }

        #region 资源获取

        /// <summary>
        /// 根据资源名称获取图片资源流
        /// </summary>
        /// <param name="_"></param>
        /// <param name="resourceName">资源的名称</param>
        /// <returns>数据流</returns>
        public static Stream GetImageResource(this Assembly _, string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            return asm.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        ///  获取程序集资源的文本资源
        /// </summary>
        /// <param name="assemblyType">程序集中的某一对象类型</param>
        /// <param name="resName">资源项名称</param>
        /// <param name="resourceHolder">资源的根名称。例如，名为“MyResource.en-US.resources”的资源文件的根名称为“MyResource”。</param>
        public static string GetStringRes(this Type assemblyType, string resName, string resourceHolder)
        {
            Assembly thisAssembly = Assembly.GetAssembly(assemblyType);
            ResourceManager rm = new ResourceManager(resourceHolder, thisAssembly);
            return rm.GetString(resName);
        }

        /// <summary>
        /// 获取程序集嵌入资源的文本形式
        /// </summary>
        /// <param name="assemblyType">程序集中的某一对象类型</param>
        /// <param name="charset">字符集编码</param>
        /// <param name="resName">嵌入资源相对路径</param>
        /// <returns>如没找到该资源则返回空字符</returns>
        public static string GetManifestString(this Type assemblyType, string charset, string resName)
        {
            Assembly asm = Assembly.GetAssembly(assemblyType);
            Stream st = asm.GetManifestResourceStream(string.Concat(assemblyType.Namespace, ".", resName.Replace("/", ".")));
            if (st == null)
            {
                return "";
            }

            int iLen = (int)st.Length;
            byte[] bytes = new byte[iLen];
            st.Read(bytes, 0, iLen);
            return Encoding.GetEncoding(charset).GetString(bytes);
        }

        #endregion 资源获取

        #region 创建实例

        /// <summary>
        /// 获取默认实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static object GetInstance(this Type type)
        {
            return GetInstance<TypeToIgnore, object>(type, null);
        }

        /// <summary>
        /// 获取默认实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static T GetInstance<T>(this Type type) where T : class, new()
        {
            return GetInstance<TypeToIgnore, T>(type, null);
        }

        /// <summary>
        /// 获取默认实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static T GetInstance<T>(string type) where T : class, new()
        {
            return GetInstance<TypeToIgnore, T>(Type.GetType(type), null);
        }

        /// <summary>
        /// 获取默认实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static object GetInstance(string type)
        {
            return GetInstance<TypeToIgnore, object>(Type.GetType(type), null);
        }

        /// <summary>
        /// 获取一个构造参数的实例
        /// </summary>
        /// <typeparam name="TArg">参数类型</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">实例类型</param>
        /// <param name="argument">参数值</param>
        /// <returns></returns>
        public static T GetInstance<TArg, T>(this Type type, TArg argument) where T : class, new()
        {
            return GetInstance<TArg, TypeToIgnore, T>(type, argument, null);
        }

        /// <summary>
        /// 获取一个构造参数的实例
        /// </summary>
        /// <typeparam name="TArg">参数类型</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">实例类型</param>
        /// <param name="argument">参数值</param>
        /// <returns></returns>
        public static T GetInstance<TArg, T>(string type, TArg argument) where T : class, new()
        {
            return GetInstance<TArg, TypeToIgnore, T>(Type.GetType(type), argument, null);
        }

        /// <summary>
        /// 获取2个构造参数的实例
        /// </summary>
        /// <typeparam name="TArg1">参数类型</typeparam>
        /// <typeparam name="TArg2">参数类型</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">实例类型</param>
        /// <param name="argument1">参数值</param>
        /// <param name="argument2">参数值</param>
        /// <returns></returns>
        public static T GetInstance<TArg1, TArg2, T>(this Type type, TArg1 argument1, TArg2 argument2) where T : class, new()
        {
            return GetInstance<TArg1, TArg2, TypeToIgnore, T>(type, argument1, argument2, null);
        }

        /// <summary>
        /// 获取2个构造参数的实例
        /// </summary>
        /// <typeparam name="TArg1">参数类型</typeparam>
        /// <typeparam name="TArg2">参数类型</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">实例类型</param>
        /// <param name="argument1">参数值</param>
        /// <param name="argument2">参数值</param>
        /// <returns></returns>
        public static T GetInstance<TArg1, TArg2, T>(string type, TArg1 argument1, TArg2 argument2) where T : class, new()
        {
            return GetInstance<TArg1, TArg2, TypeToIgnore, T>(Type.GetType(type), argument1, argument2, null);
        }

        /// <summary>
        /// 获取3个构造参数的实例
        /// </summary>
        /// <typeparam name="TArg1">参数类型</typeparam>
        /// <typeparam name="TArg2">参数类型</typeparam>
        /// <typeparam name="TArg3">参数类型</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">实例类型</param>
        /// <param name="argument1">参数值</param>
        /// <param name="argument2">参数值</param>
        /// <param name="argument3">参数值</param>
        /// <returns></returns>
        public static T GetInstance<TArg1, TArg2, TArg3, T>(this Type type, TArg1 argument1, TArg2 argument2, TArg3 argument3) where T : class, new()
        {
            return InstanceCreationFactory<TArg1, TArg2, TArg3, T>.CreateInstanceOf(type, argument1, argument2, argument3);
        }

        /// <summary>
        /// 获取3个构造参数的实例
        /// </summary>
        /// <typeparam name="TArg1">参数类型</typeparam>
        /// <typeparam name="TArg2">参数类型</typeparam>
        /// <typeparam name="TArg3">参数类型</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">实例类型</param>
        /// <param name="argument1">参数值</param>
        /// <param name="argument2">参数值</param>
        /// <param name="argument3">参数值</param>
        /// <returns></returns>
        public static T GetInstance<TArg1, TArg2, TArg3, T>(string type, TArg1 argument1, TArg2 argument2, TArg3 argument3) where T : class, new()
        {
            return InstanceCreationFactory<TArg1, TArg2, TArg3, T>.CreateInstanceOf(Type.GetType(type), argument1, argument2, argument3);
        }

        private class TypeToIgnore
        {
        }

        private static class InstanceCreationFactory<TArg1, TArg2, TArg3, TObject> where TObject : class, new()
        {
            private static readonly Dictionary<Type, Func<TArg1, TArg2, TArg3, TObject>> InstanceCreationMethods = new Dictionary<Type, Func<TArg1, TArg2, TArg3, TObject>>();

            public static TObject CreateInstanceOf(Type type, TArg1 arg1, TArg2 arg2, TArg3 arg3)
            {
                CacheInstanceCreationMethodIfRequired(type);

                return InstanceCreationMethods[type](arg1, arg2, arg3);
            }

            private static void CacheInstanceCreationMethodIfRequired(Type type)
            {
                if (InstanceCreationMethods.ContainsKey(type))
                {
                    return;
                }

                var argumentTypes = new[]
                {
                    typeof(TArg1),
                    typeof(TArg2),
                    typeof(TArg3)
                };

                Type[] constructorArgumentTypes = argumentTypes.Where(t => t != typeof(TypeToIgnore)).ToArray();
                var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, constructorArgumentTypes, new ParameterModifier[0]);

                var lamdaParameterExpressions = new[]
                {
                    Expression.Parameter(typeof(TArg1), "param1"),
                    Expression.Parameter(typeof(TArg2), "param2"),
                    Expression.Parameter(typeof(TArg3), "param3")
                };

                var constructorParameterExpressions = lamdaParameterExpressions.Take(constructorArgumentTypes.Length).ToArray();
                var constructorCallExpression = Expression.New(constructor, constructorParameterExpressions);
                var constructorCallingLambda = Expression.Lambda<Func<TArg1, TArg2, TArg3, TObject>>(constructorCallExpression, lamdaParameterExpressions).Compile();
                InstanceCreationMethods[type] = constructorCallingLambda;
            }
        }

        #endregion 创建实例
    }
}
