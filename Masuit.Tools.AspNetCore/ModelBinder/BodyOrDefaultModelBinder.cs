using System.Collections;
using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace Masuit.Tools.AspNetCore.ModelBinder;

public class BodyOrDefaultModelBinder : IModelBinder
{
	private static readonly List<BindType> AutoTypes;

	static BodyOrDefaultModelBinder()
	{
		AutoTypes = Enum.GetNames(typeof(BindType)).Select(t => Enum.Parse<BindType>(t)).Where(t => t != BindType.None && t != BindType.Services).ToList();
	}

	private readonly ILogger<BodyOrDefaultModelBinder> _logger;

	public BodyOrDefaultModelBinder(ILogger<BodyOrDefaultModelBinder> logger)
	{
		_logger = logger;
	}

	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		var context = bindingContext.HttpContext;
		var attr = bindingContext.GetAttribute<FromBodyOrDefaultAttribute>();
		var fieldName = attr?.FieldName ?? bindingContext.FieldName;
		var modelType = bindingContext.ModelType;
		object targetValue = null;
		if (attr != null)
		{
			if (modelType.IsSimpleType() || modelType.IsSimpleArrayType() || modelType.IsSimpleListType())
			{
				if (attr.Type == BindType.None)
				{
					foreach (var type in AutoTypes)
					{
						targetValue = GetBindingValue(bindingContext, type, fieldName, modelType);
						if (targetValue != null)
						{
							break;
						}
					}
				}
				else
				{
					targetValue = GetBindingValue(bindingContext, attr.Type, fieldName, modelType);
				}
			}
			else
			{
				if (BodyOrDefaultModelBinderMiddleware.JsonObject != null)
				{
					if (modelType.IsArray || modelType.IsGenericType && modelType.GenericTypeArguments.Length == 1)
					{
						if (BodyOrDefaultModelBinderMiddleware.JsonObject.TryGetValue(fieldName, StringComparison.OrdinalIgnoreCase, out var jtoken))
						{
							targetValue = jtoken.ToObject(modelType);
						}
						else
						{
							_logger.LogWarning($"TraceIdentifier：{context.TraceIdentifier}，AutoBinderMiddleware从{BodyOrDefaultModelBinderMiddleware.JsonObject}中获取{fieldName}失败！");
						}
					}
					else
					{
						// 可能是 字典或者实体 类型,尝试将modeltype 当初整个请求参数对象
						try
						{
							targetValue = BodyOrDefaultModelBinderMiddleware.JsonObject.ToObject(modelType);
						}
						catch (Exception e)
						{
							_logger.LogError(e, e.Message, BodyOrDefaultModelBinderMiddleware.JsonObject.ToString());
						}
					}
				}

				if (targetValue == null)
				{
					var (requestData, keys) = GetRequestData(bindingContext, modelType);
					if (keys.Any())
					{
						var setObj = Activator.CreateInstance(modelType);
						switch (requestData)
						{
							case IEnumerable<KeyValuePair<string, StringValues>> stringValues:
								{
									foreach (var item in stringValues)
									{
										var prop = modelType.GetProperty(item.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
										if (prop != null)
										{
											prop.SetValue(setObj, item.Value.ConvertObject(prop.PropertyType));
										}
									}

									break;
								}
							case IEnumerable<KeyValuePair<string, string>> strs:
								{
									//处理Cookie
									foreach (var item in strs)
									{
										var prop = modelType.GetProperty(item.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
										if (prop != null)
										{
											prop.SetValue(setObj, item.Value.ConvertObject(prop.PropertyType));
										}
									}

									break;
								}
							case IEnumerable<KeyValuePair<string, object>> objects:
								{
									//处理路由
									foreach (var item in objects)
									{
										var prop = modelType.GetProperty(item.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
										if (prop != null)
										{
											prop.SetValue(setObj, item.Value.ConvertObject(prop.PropertyType));
										}
									}

									break;
								}
						}

						targetValue = setObj;
					}
				}
			}

			if (targetValue == null && attr.DefaultValue != null)
			{
				targetValue = attr.DefaultValue.ChangeType(modelType);
			}
		}

		if (targetValue != null)
		{
			bindingContext.Result = ModelBindingResult.Success(targetValue);
		}

		return Task.CompletedTask;
	}

	private static (IEnumerable data, List<string> keys) GetRequestData(ModelBindingContext bindingContext, Type type)
	{
		var request = bindingContext.HttpContext.Request;
		var props = type.GetProperties().Select(t => t.Name).ToList();
		var query = props.Except(request.Query.Keys, StringComparer.OrdinalIgnoreCase).ToList();
		var headers = props.Except(request.Headers.Keys, StringComparer.OrdinalIgnoreCase).ToList();
		var cookies = props.Except(request.Cookies.Keys, StringComparer.OrdinalIgnoreCase).ToList();
		var routes = props.Except(bindingContext.ActionContext.RouteData.Values.Keys, StringComparer.OrdinalIgnoreCase).ToList();
		var list = new List<KeyValuePair<List<string>, IEnumerable>>()
		{
			new(query, request.Query),
			new(headers, request.Headers),
			new(cookies, request.Cookies),
			new(routes, bindingContext.ActionContext.RouteData.Values),
		};

		if (request.HasFormContentType && request.Form.Count > 0)
		{
			var forms = props.Except(request.Form.Keys, StringComparer.OrdinalIgnoreCase).ToList();
			list.Add(new KeyValuePair<List<string>, IEnumerable>(forms, request.Form));
		}

		var kv = list.OrderBy(t => t.Key.Count).FirstOrDefault();
		return (kv.Value, props.Except(kv.Key).ToList());
	}

	/// <summary>
	/// 获取要绑定的值
	/// </summary>
	/// <param name="bindingContext"></param>
	/// <param name="bindType"></param>
	/// <param name="fieldName"></param>
	/// <param name="modelType"></param>
	/// <returns></returns>
	private object GetBindingValue(ModelBindingContext bindingContext, BindType bindType, string fieldName, Type modelType)
	{
		var context = bindingContext.HttpContext;
		var mediaType = string.Empty;
		if (!string.IsNullOrWhiteSpace(context.Request.ContentType))
		{
			try
			{
				var cttType = new ContentType(context.Request.ContentType);
				mediaType = cttType.MediaType.ToLower();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message, context.Request.ContentType);
			}
		}

		object targetValue = null;
		switch (bindType)
		{
			case BindType.Body:
				switch (mediaType)
				{
					case "application/json":
						{
							var jsonObj = BodyOrDefaultModelBinderMiddleware.JsonObject;
							if (jsonObj != null && jsonObj.TryGetValue(fieldName, StringComparison.OrdinalIgnoreCase, out var values))
							{
								targetValue = values.ConvertObject(modelType);
							}
						}

						break;

					case "application/xml":

						//var xmlObj = AutoBinderMiddleware.XmlObject;

						//if (xmlObj != null)
						//{
						//    var xmlElt = xmlObj.Element(fieldName);

						//    if (xmlElt != null)
						//    {
						//    }
						//}
						break;
				}

				break;

			case BindType.Query:
				{
					if (context.Request.Query != null && context.Request.Query.Count > 0 && context.Request.Query.TryGetValue(fieldName, out var values))
					{
						targetValue = values.ConvertObject(modelType);
					}
				}

				break;

			case BindType.Form:
				{
					if (context.Request.HasFormContentType && context.Request.Form.Count > 0 && context.Request.Form.TryGetValue(fieldName, out StringValues values))
					{
						targetValue = values.ConvertObject(modelType);
					}
				}

				break;

			case BindType.Header:
				{
					if (context.Request.Headers != null && context.Request.Headers.Count > 0 && context.Request.Headers.TryGetValue(fieldName, out var values))
					{
						targetValue = values.ConvertObject(modelType);
					}
				}

				break;

			case BindType.Cookie:
				{
					if (context.Request.Cookies != null && context.Request.Cookies.Count > 0 && context.Request.Cookies.TryGetValue(fieldName, out var values))
					{
						targetValue = values.ConvertObject(modelType);
					}
				}

				break;

			case BindType.Route:
				{
					if (bindingContext.ActionContext.RouteData.Values != null && bindingContext.ActionContext.RouteData.Values.Count > 0 && bindingContext.ActionContext.RouteData.Values.TryGetValue(fieldName, out var values))
					{
						targetValue = values.ConvertObject(modelType);
					}
				}

				break;
		}

		return targetValue;
	}
}
