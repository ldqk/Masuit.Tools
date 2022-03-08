using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Masuit.Tools.AspNetCore.ModelBinder;

public class BodyOrDefaultBindingSource : BindingSource
{
    public static readonly BindingSource BodyOrDefault = new BodyOrDefaultBindingSource("BodyOrDefault", "BodyOrDefault", true, true);

    public BodyOrDefaultBindingSource(string id, string displayName, bool isGreedy, bool isFromRequest) : base(id, displayName, isGreedy, isFromRequest)
    {
    }

    public override bool CanAcceptDataFrom(BindingSource bindingSource)
    {
        return bindingSource == Body || bindingSource == this;
    }
}
