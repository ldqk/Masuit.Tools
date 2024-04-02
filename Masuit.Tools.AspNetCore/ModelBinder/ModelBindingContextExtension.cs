using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Masuit.Tools.AspNetCore.ModelBinder;

internal static class ModelBindingContextExtension
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
    public static bool IsJsonContent(this HttpContext httpContext, out string charSet)
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
    /// 转换为对应类型
    /// </summary>
    /// <param name="this"></param>
    /// <param name="type"></param>
    public static object ConvertObject(this object @this, Type type)
    {
        object value;
        if (@this is string str)
        {
            str = str.Trim();
            if ((str.StartsWith('[') && str.EndsWith(']')) || str.StartsWith('{') && str.EndsWith('}'))
            {
                value = JsonConvert.DeserializeObject(str, type);
            }
            else if ((str.StartsWith("\"[") && str.EndsWith("]\"")) || str.StartsWith("\"{") && str.EndsWith("}\""))
            {
                // json字符串 又被 json序列化 的情况
                var objects = JsonConvert.DeserializeObject(str);
                value = JsonConvert.SerializeObject(objects).ConvertObject(type);
            }
            else
            {
                var text = JsonConvert.SerializeObject(@this);
                value = JsonConvert.DeserializeObject(text, type);
            }
        }
        else if (@this is StringValues values)
        {
            var text = values.ToString();
            if (type.IsSimpleArrayType() || type.IsSimpleListType())
            {
                text = JsonConvert.SerializeObject(values);
                value = JsonConvert.DeserializeObject(text, type);
            }
            else
            {
                text = JsonConvert.SerializeObject(text);
                value = JsonConvert.DeserializeObject(text, type);
            }
        }
        else
        {
            var text = JsonConvert.SerializeObject(@this);
            value = JsonConvert.DeserializeObject(text, type);
        }

        return value;
    }
}