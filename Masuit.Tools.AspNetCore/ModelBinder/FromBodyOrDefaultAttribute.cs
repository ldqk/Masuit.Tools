using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Masuit.Tools.AspNetCore.ModelBinder;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class FromBodyOrDefaultAttribute : Attribute, IBindingSourceMetadata
{
    public BindingSource BindingSource => BodyOrDefaultBindingSource.BodyOrDefault;
}
