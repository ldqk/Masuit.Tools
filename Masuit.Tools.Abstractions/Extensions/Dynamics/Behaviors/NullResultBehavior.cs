using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Dynamics.Behaviors;

internal class NullResultBehavior : ClayBehavior
{
    /// <inheritdoc />
    public override object GetMember(Func<object> proceed, object self, string name)
    {
        return proceed() ?? NullInstance.Instance;
    }

    /// <inheritdoc />
    public override object GetIndex(Func<object> proceed, object self, IEnumerable<string> keys)
    {
        return proceed() ?? NullInstance.Instance;
    }

    /// <inheritdoc />
    public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args)
    {
        if (args.Any())
        {
            return proceed();
        }

        return proceed() ?? NullInstance.Instance;
    }
}
