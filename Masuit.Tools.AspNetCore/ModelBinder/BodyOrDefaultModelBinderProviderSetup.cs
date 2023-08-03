using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Masuit.Tools.AspNetCore.ModelBinder;

internal static class BodyOrDefaultModelBinderProviderSetup
{
	/// <summary>
	/// 获取绑定参数对应的 Attribute
	/// </summary>
	/// <typeparam name="T">Attribute对象</typeparam>
	/// <param name="bindingContext">绑定参数上下文</param>
	/// <param name="parameterName">参数名称</param>
	/// <returns></returns>
	public static T GetAttribute<T>(this ModelBindingContext bindingContext, string parameterName = null) where T : Attribute
	{
		var fieldName = parameterName ?? bindingContext.FieldName;
		var ctrlActionDesc = bindingContext.ActionContext.ActionDescriptor as ControllerActionDescriptor;
		var fieldParameter = ctrlActionDesc!.MethodInfo.GetParameters().Single(p => p.Name == fieldName);
		return fieldParameter.GetCustomAttributes(typeof(T), false).Single() as T;
	}

	/// <summary>
	/// 判断该次请求体Body是否是Json内容类型
	/// </summary>
	/// <param name="httpContext"></param>
	/// <param name="charSet"></param>
	/// <returns></returns>
	public static bool ContentTypeIsJson(this HttpContext httpContext, out string charSet)
	{
		string strContentType = httpContext.Request.ContentType;
		if (string.IsNullOrEmpty(strContentType))
		{
			charSet = null;
			return false;
		}

		var contentType = new ContentType(strContentType);
		charSet = contentType.CharSet;
		return contentType.MediaType.ToLower() == "application/json";
	}

	/// <summary>
	/// 获取请求体Body字符串内容
	/// </summary>
	/// <param name="context"></param>
	/// <param name="encoding"></param>
	/// <returns></returns>
	public static string GetBodyString(this HttpContext context, Encoding encoding)
	{
		context.Request.EnableBuffering(); //Ensure the HttpRequest.Body can be read multipletimes
		int contentLen = 255;
		if (context.Request.ContentLength != null)
		{
			contentLen = (int)context.Request.ContentLength;
		}

		var body = context.Request.Body;
		string bodyText;
		if (contentLen <= 0)
		{
			bodyText = "";
		}
		else
		{
			using var reader = new StreamReader(body, encoding, true, contentLen, true);
			bodyText = reader.ReadToEndAsync().Result;
		}

		body.Position = 0;
		return bodyText;
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
	/// 转换为对应类型
	/// </summary>
	/// <param name="this"></param>
	/// <param name="toType"></param>
	/// <returns></returns>
	public static object ConvertObject(this object @this, Type toType)
	{
		object targetValue;
		if (@this is string strValue)
		{
			strValue = strValue.Trim();
			if ((strValue.StartsWith("[") && strValue.EndsWith("]")) || strValue.StartsWith("{") && strValue.EndsWith("}"))
			{
				targetValue = JsonConvert.DeserializeObject(strValue, toType);
			}
			else if ((strValue.StartsWith("\"[") && strValue.EndsWith("]\"")) || strValue.StartsWith("\"{") && strValue.EndsWith("}\""))
			{
				// json字符串 又被 json序列化 的情况
				var objects = JsonConvert.DeserializeObject(strValue);
				targetValue = JsonConvert.SerializeObject(objects).ConvertObject(toType);
			}
			else
			{
				var text = JsonConvert.SerializeObject(@this);
				targetValue = JsonConvert.DeserializeObject(text, toType);
			}
		}
		else if (@this is StringValues values)
		{
			var text = values.ToString();
			if (toType.IsSimpleArrayType() || toType.IsSimpleListType())
			{
				text = JsonConvert.SerializeObject(values);
				targetValue = JsonConvert.DeserializeObject(text, toType);
			}
			else
			{
				text = JsonConvert.SerializeObject(text);
				targetValue = JsonConvert.DeserializeObject(text, toType);
			}
		}
		else if (@this is JToken)
		{
			var text = JsonConvert.SerializeObject(@this);
			targetValue = JsonConvert.DeserializeObject(text, toType);
		}
		else
		{
			var text = JsonConvert.SerializeObject(@this);
			targetValue = JsonConvert.DeserializeObject(text, toType);
		}

		return targetValue;
	}

	/// <summary>
	/// 尝试设置默认值
	/// </summary>
	/// <param name="bindingContext"></param>
	public static bool TrySetDefaultValue(this ModelBindingContext bindingContext)
	{
		var attr = bindingContext.GetAttribute<FromBodyOrDefaultAttribute>();

		if (attr.DefaultValue != null)
		{
			var targetValue = attr.DefaultValue.ChangeType(bindingContext.ModelType);
			bindingContext.Result = ModelBindingResult.Success(targetValue);
			return true;
		}

		return false;
	}

	/// <summary>
	/// 对象类型转换
	/// </summary>
	/// <param name="this">当前值</param>
	/// <param name="toType">指定类型的类型</param>
	/// <returns>转换后的对象</returns>
	public static object ChangeType(this object @this, Type toType)
	{
		var currType = Nullable.GetUnderlyingType(@this.GetType()) ?? @this.GetType();
		toType = Nullable.GetUnderlyingType(toType) ?? toType;
		if (@this == DBNull.Value)
		{
			if (!toType.IsValueType)
			{
				return null;
			}

			throw new Exception("不能将null值转换为" + toType.Name + "类型!");
		}

		if (currType == toType)
		{
			return @this;
		}

		if (toType.IsAssignableFrom(typeof(string)))
		{
			return @this.ToString();
		}

		if (toType.IsEnum)
		{
			return Enum.Parse(toType, @this.ToString(), true);
		}

		if (toType.IsAssignableFrom(typeof(Guid)))
		{
			return Guid.Parse(@this.ToString());
		}

		if (!toType.IsArray || !currType.IsArray)
		{
			return Convert.ChangeType(@this, toType);
		}

		var length = ((Array)@this).Length;
		var targetType = Type.GetType(toType.FullName.Trim('[', ']'));
		var targetArr = Array.CreateInstance(targetType, length);
		for (int j = 0; j < length; j++)
		{
			var tmp = ((Array)@this).GetValue(j);
			targetArr.SetValue(ChangeType(tmp, targetType), j);
		}

		return targetArr;
	}
}
