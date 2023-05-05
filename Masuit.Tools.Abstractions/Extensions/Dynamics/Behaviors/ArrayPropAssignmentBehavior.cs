using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Dynamics.Behaviors;

internal class ArrayPropAssignmentBehavior : ClayBehavior
{
    /// <inheritdoc />
    public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args)
    {
        return IfSingleArray(args, arr =>
        {
            ((dynamic)self)[name] = arr;
            return self;
        }, () => IfTwoOrMoreArgs(args, arr =>
        {
            ((dynamic)self)[name] = arr;
            return self;
        }, proceed));
    }

    private static object IfTwoOrMoreArgs(IEnumerable<object> args, Func<dynamic, object> func, Func<object> proceed)
    {
        var objects = args as List<object> ?? args.ToList();
        return objects.Count < 2 ? proceed() : func(NewArray().AddRange(objects));
    }

    private static object IfSingleArray(IEnumerable<object> args, Func<dynamic, object> func, Func<object> proceed)
    {
        var objects = args as List<object> ?? args.ToList();
        if (objects.Count != 1)
        {
            return proceed();
        }

        var arr = objects.Single();
        if (arr == null)
        {
            return proceed();
        }

        return arr is Array ? func(NewArray().AddRange(arr)) : proceed();
    }

    private static dynamic NewArray()
    {
        return new Clay(new InterfaceProxyBehavior(), new PropBehavior(), new ArrayPropAssignmentBehavior(), new ArrayBehavior(), new NullResultBehavior());
    }
}
