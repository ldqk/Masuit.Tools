using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Masuit.Tools.AspNetCore.ModelBinder;

public sealed class BodyOrDefaultModelBinderMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<BodyOrDefaultModelBinderMiddleware> _logger;

	public BodyOrDefaultModelBinderMiddleware(RequestDelegate next, ILogger<BodyOrDefaultModelBinderMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public static JObject JsonObject { get; private set; }

	public static XDocument XmlObject { get; private set; }

	public Task Invoke(HttpContext context)
	{
		JsonObject = null;
		XmlObject = null;

		var contentType = context.Request.ContentType;
		string mediaType;
		var charSet = "utf-8";

		if (string.IsNullOrWhiteSpace(contentType))
		{
			//表单提交
			mediaType = "application/x-www-form-urlencoded";
		}
		else
		{
			var cttType = new ContentType(contentType);
			if (!string.IsNullOrWhiteSpace(cttType.CharSet))
			{
				charSet = cttType.CharSet;
			}
			mediaType = cttType.MediaType.ToLower();
		}

		Encoding encoding = Encoding.GetEncoding(charSet);

		if (mediaType == "application/x-www-form-urlencoded")
		{
			//普通表单提交
		}
		else if (mediaType == "multipart/form-data")
		{
			//带有文件的表单提交
		}
		else if (mediaType == "application/json")
		{
			#region json数据提交

			var body = context.GetBodyString(encoding)?.Trim();

			if (string.IsNullOrWhiteSpace(body))
			{
				return _next(context);
			}

			if (!(body.StartsWith("{") && body.EndsWith("}")))
			{
				return _next(context);
			}

			try
			{
				JsonObject = JObject.Parse(body);
				return _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Parsing json failed:" + body);
				return _next(context);
			}

			#endregion json数据提交
		}
		else if (mediaType == "application/xml")
		{
			#region xml数据提交

			var body = context.GetBodyString(encoding)?.Trim();

			if (string.IsNullOrWhiteSpace(body))
			{
				return _next(context);
			}

			try
			{
				XmlObject = XDocument.Parse(body);
				return _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Parsing xml failed:" + body);
				return _next(context);
			}

			#endregion xml数据提交
		}
		return _next(context);
	}
}
