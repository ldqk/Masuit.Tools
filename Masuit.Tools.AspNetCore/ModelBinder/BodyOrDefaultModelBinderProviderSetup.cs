using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

#if NET5_0_OR_GREATER

using ComplexDataModelBinderProvider = Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ComplexObjectModelBinderProvider;

#else

using ComplexDataModelBinderProvider = Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ComplexTypeModelBinderProvider;

#endif

namespace Masuit.Tools.AspNetCore.ModelBinder;

public static class BodyOrDefaultModelBinderProviderSetup
{
    public static void InsertBodyOrDefaultBinding(this IList<IModelBinderProvider> providers)
    {
        var bodyProvider = providers.OfType<BodyModelBinderProvider>().Single();
        var complexDataProvider = providers.OfType<ComplexDataModelBinderProvider>().Single();
        providers.Insert(0, new BodyOrDefaultModelBinderProvider(bodyProvider, complexDataProvider));
    }
}
