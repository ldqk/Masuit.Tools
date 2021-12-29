using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System;
using System.Linq;

namespace Masuit.Tools.Core.AspNetCore;

public class BodyAndQueryModelBinder<T> : IModelBinder where T : IConvertible
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var body = bindingContext.HttpContext.Request.Body;
        using var reader = new StreamReader(body);
        var text = await reader.ReadToEndAsync();

        if (typeof(T).IsPrimitive || typeof(T) == typeof(string) || typeof(T).IsEnum || typeof(T) == typeof(DateTime) || typeof(T) == typeof(Guid))
        {
            bindingContext.Result = ModelBindingResult.Success(JsonConvert.DeserializeObject<T>(text) ?? bindingContext.HttpContext.Request.Query[bindingContext.ModelMetadata.ParameterName!].ToString().ConvertTo<T>());
            return;
        }

        var dict = HttpUtility.ParseQueryString(bindingContext.HttpContext.Request.QueryString.ToString());
        var contract = JsonConvert.DeserializeObject<T>(text) ?? JsonConvert.DeserializeObject<T>(dict.Cast<string>().ToDictionary(k => k, k => dict[k]).ToJsonString());
        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            var valueProvider = bindingContext.ValueProvider.GetValue(property.Name);
            if (string.IsNullOrEmpty(valueProvider.FirstValue))
            {
                property.SetValue(contract, valueProvider.FirstValue);
            }
        }
        bindingContext.Result = ModelBindingResult.Success(contract);
    }
}
