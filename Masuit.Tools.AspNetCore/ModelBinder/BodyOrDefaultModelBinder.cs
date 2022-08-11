using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Masuit.Tools.AspNetCore.ModelBinder;

public class BodyOrDefaultModelBinder : IModelBinder
{
    private readonly IModelBinder _bodyBinder;
    private readonly IModelBinder _complexBinder;

    public BodyOrDefaultModelBinder(IModelBinder bodyBinder, IModelBinder complexBinder)
    {
        _bodyBinder = bodyBinder;
        _complexBinder = complexBinder;
    }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var request = bindingContext.HttpContext.Request;
        request.EnableBuffering();
        var buffer = new byte[Convert.ToInt32(request.ContentLength)];
        _ = await request.Body.ReadAsync(buffer, 0, buffer.Length);
        var text = Encoding.UTF8.GetString(buffer);
        request.Body.Position = 0;

        if (bindingContext.ModelType.IsPrimitive || bindingContext.ModelType == typeof(string) || bindingContext.ModelType.IsEnum || bindingContext.ModelType == typeof(DateTime) || bindingContext.ModelType == typeof(Guid) || (bindingContext.ModelType.IsGenericType && bindingContext.ModelType.GetGenericTypeDefinition() == typeof(Nullable<>)))
        {
            var parameter = bindingContext.ModelMetadata.ParameterName;
            var value = "";
            if (request.Query.ContainsKey(parameter))
            {
                value = request.Query[parameter] + "";
            }
            else if (request.ContentType is not null && request.ContentType.StartsWith("application/json"))
            {
                try
                {
                    value = JObject.Parse(text)[parameter] + "";
                }
                catch
                {
                    value = text.Trim('"');
                }
            }
            else if (request.HasFormContentType)
            {
                value = request.Form[bindingContext.ModelMetadata.ParameterName] + "";
            }

            if (value.TryConvertTo(bindingContext.ModelType, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            return;
        }

        if (request.HasFormContentType)
        {
            if (bindingContext.ModelType.IsClass)
            {
                await DefaultBindModel(bindingContext);
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(request.Form[bindingContext.ModelMetadata.ParameterName].ToString().ConvertTo(bindingContext.ModelType));
            }
            return;
        }

        try
        {
            bindingContext.Result = ModelBindingResult.Success(JsonConvert.DeserializeObject(text, bindingContext.ModelType) ?? request.Query[bindingContext.ModelMetadata.ParameterName!].ToString().ConvertTo(bindingContext.ModelType));
        }
        catch
        {
            await DefaultBindModel(bindingContext);
        }
    }

    private async Task DefaultBindModel(ModelBindingContext bindingContext)
    {
        await _bodyBinder.BindModelAsync(bindingContext);
        if (bindingContext.Result.IsModelSet)
        {
            return;
        }

        bindingContext.ModelState.Clear();
        await _complexBinder.BindModelAsync(bindingContext);
    }
}
