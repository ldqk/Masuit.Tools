using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Masuit.Tools.Reflection;

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
	/// 获取字段
	/// </summary>
	/// <param name="obj">反射对象</param>
	/// <param name="name">字段名</param>
	/// <typeparam name="T">约束返回的T必须是引用类型</typeparam>
	/// <returns>T类型</returns>
	public static object GetField(this object obj, string name)
	{
		return GetProperty(obj, name);
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
	/// <returns>旧值</returns>
	public static string SetProperty<T>(this T obj, string name, object value) where T : class
	{
		var type = obj.GetType();
		var parameter = Expression.Parameter(type, "e");
		var property = Expression.PropertyOrField(parameter, name);
		var before = Expression.Lambda(property, parameter).Compile().DynamicInvoke(obj);
		if (value == before)
		{
			return value?.ToString();
		}

		if (property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			if (value is IConvertible x && x.TryConvertTo(property.Type.GenericTypeArguments[0], out var v))
			{
				type.GetProperty(name)?.SetValue(obj, v);
			}
			else
			{
				type.GetProperty(name)?.SetValue(obj, value);
			}
		}
		else
		{
			var valueExpression = Expression.Parameter(property.Type, "v");
			var assign = Expression.Assign(property, valueExpression);
			if (value is IConvertible x && x.TryConvertTo(property.Type, out var v))
			{
				Expression.Lambda(assign, parameter, valueExpression).Compile().DynamicInvoke(obj, v);
			}
			else
			{
				Expression.Lambda(assign, parameter, valueExpression).Compile().DynamicInvoke(obj, value);
			}
		}

		return before.ToJsonString();
	}

	private static readonly ConcurrentDictionary<string, Delegate> DelegateCache = new();

	/// <summary>
	/// 获取属性
	/// </summary>
	/// <param name="obj">反射对象</param>
	/// <param name="name">属性名</param>
	/// <typeparam name="T">约束返回的T必须是引用类型</typeparam>
	/// <returns>T类型</returns>
	public static T GetProperty<T>(this object obj, string name)
	{
		return (T)GetProperty(obj, name);
	}

	/// <summary>
	/// 获取属性
	/// </summary>
	/// <param name="obj">反射对象</param>
	/// <param name="name">属性名</param>
	/// <returns>T类型</returns>
	public static object GetProperty(this object obj, string name)
	{
		var type = obj.GetType();
		if (DelegateCache.TryGetValue(type.Name + "." + name, out var func))
		{
			return func.DynamicInvoke(obj);
		}
		var parameter = Expression.Parameter(type, "e");
		var property = Expression.PropertyOrField(parameter, name);
		func = Expression.Lambda(property, parameter).Compile();
		DelegateCache.TryAdd(type.Name + "." + name, func);
		return func.DynamicInvoke(obj);
	}

	/// <summary>
	/// 获取所有的属性信息
	/// </summary>
	/// <param name="obj">反射对象</param>
	/// <returns>属性信息</returns>
	public static PropertyInfo[] GetProperties(this object obj)
	{
		return obj.GetType().GetProperties(bf);
	}

	#endregion 属性字段设置

	#region 获取Description

	/// <summary>
	///	根据成员信息获取Description信息
	/// </summary>
	/// <param name="member">成员信息</param>
	/// <returns>如果未找到DescriptionAttribute则返回null或返回类型描述</returns>
	public static string GetDescription(this MemberInfo member)
	{
		return GetDescription(member, new object[0]);
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

		var description = member.IsDefined(typeof(DescriptionAttribute), false) ? member.GetAttribute<DescriptionAttribute>().Description : string.Empty;
		return args.Length > 0 ? string.Format(description, args) : description;
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

	/// <summary>
	/// 获取对象的Attributes
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider provider) where T : Attribute
	{
		return provider.GetCustomAttributes(typeof(T), true).OfType<T>();
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
		var asm = Assembly.GetAssembly(assemblyType);
		using var stream = asm.GetManifestResourceStream(string.Concat(assemblyType.Namespace, ".", resName.Replace("/", ".")));
		if (stream == null)
		{
			return "";
		}

		int iLen = (int)stream.Length;
		byte[] bytes = new byte[iLen];
		_ = stream.Read(bytes, 0, iLen);
		return Encoding.GetEncoding(charset).GetString(bytes);
	}

	/// <summary>
	/// 判断指定的类型 <paramref name="this"/> 是否是指定泛型类型的子类型，或实现了指定泛型接口。
	/// </summary>
	/// <param name="this">需要测试的类型。</param>
	/// <param name="type">泛型接口类型，传入 typeof(IXxx&lt;&gt;)</param>
	/// <returns>如果是泛型接口的子类型，则返回 true，否则返回 false。</returns>
	public static bool IsImplementsOf(this Type @this, Type type)
	{
		if (@this == null) throw new ArgumentNullException(nameof(@this));
		if (type == null) throw new ArgumentNullException(nameof(type));

		// 测试接口。
		var isTheRawGenericType = @this.GetInterfaces().Any(IsTheRawGenericType);
		if (isTheRawGenericType) return true;

		// 测试类型。
		while (@this != null && @this != typeof(object))
		{
			isTheRawGenericType = IsTheRawGenericType(@this);
			if (isTheRawGenericType) return true;
			@this = @this.BaseType;
		}

		// 没有找到任何匹配的接口或类型。
		return false;

		// 测试某个类型是否是指定的原始接口。
		bool IsTheRawGenericType(Type test) => type == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
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
		private static readonly Dictionary<Type, Func<TArg1, TArg2, TArg3, TObject>> InstanceCreationMethods = new();

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
			var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, constructorArgumentTypes, []);
			var lamdaParameterExpressions = new[]
			{
				Expression.Parameter(typeof(TArg1), "param1"),
				Expression.Parameter(typeof(TArg2), "param2"),
				Expression.Parameter(typeof(TArg3), "param3")
			};

			var constructorParameterExpressions = lamdaParameterExpressions.Take(constructorArgumentTypes.Length).ToArray();
			var constructorCallExpression = Expression.New(constructor, constructorParameterExpressions);
			InstanceCreationMethods[type] = Expression.Lambda<Func<TArg1, TArg2, TArg3, TObject>>(constructorCallExpression, lamdaParameterExpressions).Compile();
		}
	}

	#endregion 创建实例

	/// <summary>
	/// 获取可加载的程序集类型信息
	/// </summary>
	/// <param name="assembly"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
	{
		if (assembly == null) throw new ArgumentNullException(nameof(assembly));
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException e)
		{
			return e.Types.Where(t => t != null);
		}
	}

	/// <summary>
	/// 获取可加载的程序集类型信息
	/// </summary>
	/// <param name="assembly"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static IEnumerable<Type> GetLoadableExportedTypes(this Assembly assembly)
	{
		if (assembly == null) throw new ArgumentNullException(nameof(assembly));
		try
		{
			return assembly.GetExportedTypes();
		}
		catch (ReflectionTypeLoadException e)
		{
			return e.Types.Where(t => t != null);
		}
	}
}