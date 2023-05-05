using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Masuit.Tools.Dynamics;

internal class ClayBehaviorCollection : List<IClayBehavior>, IClayBehavior
{
    public ClayBehaviorCollection(IEnumerable<IClayBehavior> behaviors)
        : base(behaviors)
    {
    }

    private object Execute(Func<object> proceed, Func<Func<object>, IClayBehavior, Func<object>> linker)
    {
        return this.Aggregate(proceed, linker)();
    }

    /// <inheritdoc />
    public object GetMember(Func<object> proceed, object self, string name)
    {
        return Execute(proceed, (next, behavior) => () => behavior.GetMember(next, self, name));
    }

    public object SetMember(Func<object> proceed, object self, string name, object value)
    {
        return Execute(proceed, (next, behavior) => () => behavior.SetMember(next, self, name, value));
    }

    public object GetMembers(Func<object> proceed, object self, IDictionary<string, object> members)
    {
        return Execute(proceed, (next, behavior) => () => behavior.GetMembers(next, self, members));
    }

    public object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args)
    {
        return Execute(proceed, (next, behavior) => () => behavior.InvokeMember(next, self, name, args));
    }

    public object GetIndex(Func<object> proceed, object self, IEnumerable<string> keys)
    {
        return Execute(proceed, (next, behavior) => () => behavior.GetIndex(next, self, keys));
    }

    public object SetIndex(Func<object> proceed, object self, IEnumerable<string> keys, object value)
    {
        return Execute(proceed, (next, behavior) => () => behavior.SetIndex(next, self, keys, value));
    }

    public object Convert(Func<object> proceed, object self, Type type, bool isExplicit)
    {
        return Execute(proceed, (next, behavior) => () => behavior.Convert(next, self, type, isExplicit));
    }

    public object BinaryOperation(Func<object> proceed, object self, ExpressionType operation, object value)
    {
        return Execute(proceed, (next, behavior) => () => behavior.BinaryOperation(next, self, operation, value));
    }

    public object InvokeMemberMissing(Func<object> proceed, object self, string name, INamedEnumerable<object> args)
    {
        return Execute(proceed, (next, behavior) => () => behavior.InvokeMemberMissing(next, self, name, args));
    }

    public object GetMemberMissing(Func<object> proceed, object self, string name)
    {
        return Execute(proceed, (next, behavior) => () => behavior.GetMemberMissing(next, self, name));
    }

    public object SetMemberMissing(Func<object> proceed, object self, string name, object value)
    {
        return Execute(proceed, (next, behavior) => () => behavior.SetMemberMissing(next, self, name, value));
    }

    public object ConvertMissing(Func<object> proceed, object self, Type type, bool isExplicit)
    {
        return Execute(proceed, (next, behavior) => () => behavior.ConvertMissing(next, self, type, isExplicit));
    }
}
