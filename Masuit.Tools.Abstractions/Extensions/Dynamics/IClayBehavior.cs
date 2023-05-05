using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Masuit.Tools.Dynamics;

public interface IClayBehavior
{
    object GetMember(Func<object> proceed, object self, string name);

    object SetMember(Func<object> proceed, object self, string name, object value);

    object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args);

    object GetIndex(Func<object> proceed, object self, IEnumerable<string> keys);

    object SetIndex(Func<object> proceed, object self, IEnumerable<string> keys, object value);

    object GetMembers(Func<object> proceed, object self, IDictionary<string, object> members);

    object Convert(Func<object> proceed, object self, Type type, bool isExplicit);

    object BinaryOperation(Func<object> proceed, object self, ExpressionType operation, object value);

    object InvokeMemberMissing(Func<object> proceed, object self, string name, INamedEnumerable<object> args);

    object GetMemberMissing(Func<object> proceed, object self, string name);

    object SetMemberMissing(Func<object> proceed, object self, string name, object value);

    object ConvertMissing(Func<object> proceed, object self, Type type, bool isExplicit);
}

public interface INamedEnumerable<T> : IEnumerable<T>
{
    IEnumerable<T> Positional { get; }

    IDictionary<string, T> Named { get; }
}
