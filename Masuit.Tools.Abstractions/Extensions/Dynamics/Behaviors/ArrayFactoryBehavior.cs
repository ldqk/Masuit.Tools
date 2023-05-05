using System;

namespace Masuit.Tools.Dynamics.Behaviors;

internal class ArrayFactoryBehavior : ClayBehavior
{
    /// <inheritdoc />
    public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args)
    {
        if (name == "Array")
        {
            dynamic x = new Clay(
                new InterfaceProxyBehavior(),
                new PropBehavior(),
                new ArrayPropAssignmentBehavior(),
                new ArrayBehavior(),
                new NullResultBehavior());
            x.AddRange(args);
            return x;
        }
        return proceed();
    }
}
