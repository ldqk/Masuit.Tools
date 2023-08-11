using System.Collections;
using System.Net.Mime;
using System.Reflection;
using System.Xml.Linq;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace Masuit.Tools.AspNetCore.ModelBinder;

public class FromBodyOrDefaultModelBinder : IModelBinder
{
	private static readonly List<BindType> BindTypes;

	static FromBodyOrDefaultModelBinder()
	{
		BindTypes = Enum.GetNames(typeof(BindType)).Select(Enum.Parse<BindType>).Where(t => t != BindType.Default).ToList();
	}

	private readonly ILogger<FromBodyOrDefaultModelBinder> _logger;

	public FromBodyOrDefaultModelBinder(ILogger<FromBodyOrDefaultModelBinder> logger)
	{
		_logger = logger;
	}

	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		var context = bindingContext.HttpContext;
		var attr = bindingContext.GetAttribute<FromBodyOrDefaultAttribute>();
		var field = attr?.FieldName ?? bindingContext.FieldName;
		var modelType = bindingContext.ModelType;
		object value = null;
		if (attr != null)
		{
			if (modelType.IsSimpleType() || modelType.IsSimpleArrayType() || modelType.IsSimpleListType())
			{
				if (attr.Type == BindType.Default)
				{
					foreach (var type in BindTypes)
					{
						value = GetBindingValue(bindingContext, type, field, modelType);
						if (value != null)
						{
							break;
						}
					}
				}
				else
				{
					foreach (var type in attr.Type.Split())
					{
						value = GetBindingValue(bindingContext, type, field, modelType);
						if (value != null)
						{
							break;
						}
					}
				}
			}
			else
			{
				if (bindingContext.HttpContext.Items.TryGetValue("BodyOrDefaultModelBinder@JsonBody", out var obj) && obj is JObject json)
				{
					if (modelType.IsArray || modelType.IsGenericType && modelType.GenericTypeArguments.Length == 1)
					{
						if (json.TryGetValue(field, StringComparison.OrdinalIgnoreCase, out var jtoken))
						{
							value = jtoken.ToObject(modelType);
						}
						else
						{
							_logger.LogWarning($"TraceIdentifier：{context.TraceIdentifier}，BodyOrDefaultModelBinder从{json}中获取{field}失败！");
						}
					}
					else
					{
						// 可能是 字典或者实体 类型,尝试将modeltype 当初整个请求参数对象
						try
						{
							value = json.ToObject(modelType);
						}
						catch (Exception e)
						{
							_logger.LogError(e, e.Message, json.ToString());
						}
					}
				}

				if (value == null)
				{
					var (requestData, keys) = GetRequestData(bindingContext, modelType);
					if (keys.Any())
					{
						var instance = Activator.CreateInstance(modelType);
						switch (requestData)
						{
							case IEnumerable<KeyValuePair<string, StringValues>> stringValues:
								{
									foreach (var item in stringValues)
									{
										var property = modelType.GetProperty(item.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
										if (property != null)
										{
											property.SetValue(instance, item.Value.ConvertObject(property.PropertyType));
										}
									}

									break;
								}
							case IEnumerable<KeyValuePair<string, string>> strs:
								{
									//处理Cookie
									foreach (var item in strs)
									{
										var property = modelType.GetProperty(item.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
										if (property != null)
										{
											property.SetValue(instance, item.Value.ConvertObject(property.PropertyType));
										}
									}

									break;
								}
							case IEnumerable<KeyValuePair<string, object>> objects:
								{
									//处理路由
									foreach (var item in objects)
									{
										var property = modelType.GetProperty(item.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
										if (property != null)
										{
											property.SetValue(instance, item.Value.ConvertObject(property.PropertyType));
										}
									}

									break;
								}
						}

						value = instance;
					}
				}
			}

			if (value == null && attr.DefaultValue != null)
			{
				value = attr.DefaultValue.ChangeType(modelType);
			}
		}

		if (value != null)
		{
			bindingContext.Result = ModelBindingResult.Success(value);
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
	private object GetBindingValue(ModelBindingContext bindingContext, BindType bindType, string fieldName, Type modelType)
	{
		var context = bindingContext.HttpContext;
		var mediaType = string.Empty;
		if (!string.IsNullOrWhiteSpace(context.Request.ContentType))
		{
			try
			{
				var contentType = new ContentType(context.Request.ContentType);
				mediaType = contentType.MediaType.ToLower();
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
							if (bindingContext.HttpContext.Items.TryGetValue("BodyOrDefaultModelBinder@JsonBody", out var obj) && obj is JObject json && json.TryGetValue(fieldName, StringComparison.OrdinalIgnoreCase, out var values))
							{
								targetValue = values.ConvertObject(modelType);
							}
						}

						break;

					case "application/xml":
						{
							if (bindingContext.HttpContext.Items.TryGetValue("BodyOrDefaultModelBinder@XmlBody", out var obj) && obj is XDocument xml)
							{
								var xmlElt = xml.Element(fieldName);
								if (xmlElt != null)
								{
									targetValue = xmlElt.Value.ConvertObject(modelType);
								}
							}
							break;
						}
				}

				break;

			case BindType.Query:
				{
					if (context.Request.Query is { Count: > 0 } && context.Request.Query.TryGetValue(fieldName, out var values))
					{
						targetValue = values.ConvertObject(modelType);
					}
				}

				break;

			case BindType.Form:
				{
					if (context.Request is { HasFormContentType: true, Form.Count: > 0 } && context.Request.Form.TryGetValue(fieldName, out var values))
					{
						targetValue = values.ConvertObject(modelType);
					}
				}

				break;

			case BindType.Header:
				{
					if (context.Request.Headers is { Count: > 0 } && context.Request.Headers.TryGetValue(fieldName, out var values))
					{
						targetValue = values.ConvertObject(modelType);
					}
				}

				break;

			case BindType.Cookie:
				{
					if (context.Request.Cookies is { Count: > 0 } && context.Request.Cookies.TryGetValue(fieldName, out var values))
					{
						targetValue = values.ConvertObject(modelType);
					}
				}

				break;

			case BindType.Route:
				{
					if (bindingContext.ActionContext.RouteData.Values is { Count: > 0 } && bindingContext.ActionContext.RouteData.Values.TryGetValue(fieldName, out var values))
					{
						targetValue = values.ConvertObject(modelType);
					}
				}

				break;

			case BindType.Services:
				targetValue = bindingContext.ActionContext.HttpContext.RequestServices.GetRequiredService(modelType);
				break;
		}

		return targetValue;
	}
}
