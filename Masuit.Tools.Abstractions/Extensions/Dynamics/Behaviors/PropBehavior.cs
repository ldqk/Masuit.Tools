using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Dynamics.Behaviors;

internal class PropBehavior : ClayBehavior
{
    private readonly Dictionary<string, object> _props = new();

    /// <inheritdoc />
    public override object GetMembers(Func<object> proceed, object self, IDictionary<string, object> members)
    {
        foreach (var pair in _props)
        {
            members.Add(pair.Key, pair.Value);
        }

        return proceed();
    }

    /// <inheritdoc />
    public override object GetMemberMissing(Func<object> proceed, object self, string name)
    {
        return _props.TryGetValue(name, out var value) ? value : null;
    }

    /// <inheritdoc />
    public override object SetMemberMissing(Func<object> proceed, object self, string name, object value)
    {
        return _props[name] = value;
    }

    /// <inheritdoc />
    public override object InvokeMemberMissing(Func<object> proceed, object self, string name, INamedEnumerable<object> args)
    {
        if (!args.Any())
        {
            return GetMemberMissing(proceed, self, name);
        }

        if (args.Count() == 1)
        {
            SetMemberMissing(proceed, self, name, args.Single());
            return self;
        }

        return proceed();
    }

    /// <inheritdoc />
    public override object GetIndex(Func<object> proceed, object self, IEnumerable<string> keys)
    {
        var list = keys.ToList();
        if (list.Count != 1) proceed();
        return _props.TryGetValue(list[0], out var value) ? value : null;
    }

    /// <inheritdoc />
    public override object SetIndex(Func<object> proceed, object self, IEnumerable<string> keys, object value)
    {
        var list = keys.ToList();
        if (list.Count != 1) proceed();
        return _props[list[0]] = value;
    }

    public Dictionary<string, object> GetProps()
    {
        return _props;
    }
}
