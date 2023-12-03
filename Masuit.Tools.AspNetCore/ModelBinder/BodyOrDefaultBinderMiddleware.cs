using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Masuit.Tools.AspNetCore.ModelBinder;

public sealed class BodyOrDefaultBinderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BodyOrDefaultBinderMiddleware> _logger;

    public BodyOrDefaultBinderMiddleware(RequestDelegate next, ILogger<BodyOrDefaultBinderMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task Invoke(HttpContext context)
    {
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
            var type = new ContentType(contentType);
            if (!string.IsNullOrWhiteSpace(type.CharSet))
            {
                charSet = type.CharSet;
            }

            mediaType = type.MediaType.ToLower();
        }

        var encoding = Encoding.GetEncoding(charSet);
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
                context.Items.AddOrUpdate("BodyOrDefaultModelBinder@JsonBody", _ => JObject.Parse(body), (_, _) => JObject.Parse(body));
                return _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parsing json failed:" + body);
                return _next(context);
            }
        }
        else if (mediaType == "application/xml")
        {
            var body = context.GetBodyString(encoding)?.Trim();
            if (string.IsNullOrWhiteSpace(body))
            {
                return _next(context);
            }

            try
            {
                context.Items.AddOrUpdate("BodyOrDefaultModelBinder@XmlBody", _ => XDocument.Parse(body), (_, _) => XDocument.Parse(body));
                return _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parsing xml failed:" + body);
                return _next(context);
            }
        }

        return _next(context);
    }
}
