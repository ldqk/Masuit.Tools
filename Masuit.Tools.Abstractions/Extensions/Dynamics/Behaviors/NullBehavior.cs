using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Masuit.Tools.Dynamics.Behaviors;

internal class NullBehavior : ClayBehavior
{
    /// <inheritdoc />
    public override object GetMember(Func<object> proceed, object self, string name)
    {
        return NullInstance.Instance;
    }

    /// <inheritdoc />
    public override object GetIndex(Func<object> proceed, object self, IEnumerable<string> keys)
    {
        return NullInstance.Instance;
    }

    /// <inheritdoc />
    public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args)
    {
        if (args.Any())
        {
            return proceed();
        }

        return name == "ToString" ? string.Empty : NullInstance.Instance;
    }

    /// <inheritdoc />
    public override object Convert(Func<object> proceed, object self, Type type, bool isExplicit)
    {
        return type.IsInterface ? proceed() : null;
    }

    /// <inheritdoc />
    public override object BinaryOperation(Func<object> proceed, object self, ExpressionType operation, object value)
    {
        switch (operation)
        {
            case ExpressionType.Equal:
                return ReferenceEquals(value, NullInstance.Instance) || value == null;

            case ExpressionType.NotEqual:
                return !ReferenceEquals(value, NullInstance.Instance) && value != null;
        }

        return proceed();
    }
}
