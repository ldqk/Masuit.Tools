using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.Tools.Dynamics.Behaviors;

internal class ArrayBehavior : ClayBehavior
{
    private readonly List<object> _data = new();

    /// <inheritdoc />
    public override object GetIndex(Func<object> proceed, object self, IEnumerable<string> keys)
    {
        return IfSingleInteger(keys, key => _data[key], proceed);
    }

    /// <inheritdoc />
    public override object SetIndex(Func<object> proceed, object self, IEnumerable<string> keys, object value)
    {
        return IfSingleInteger(keys, key => _data[key] = value, proceed);
    }

    /// <inheritdoc />
    public override object GetMember(Func<object> proceed, object self, string name)
    {
        switch (name)
        {
            case "Length":
            case "Count":
                return _data.Count;

            case "GetEnumerator":
                return new Clay(new InterfaceProxyBehavior(), new EnumeratorBehavior(_data.GetEnumerator()));
        }
        return proceed();
    }

    /// <inheritdoc />
    public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args)
    {
        switch (name)
        {
            case "AddRange":
                _data.AddRange(((IEnumerable)args.Single()).OfType<object>());
                return self;

            case "Add":
                _data.AddRange(args);
                return self;

            case "Insert":
                return IfInitialInteger(args, (index, arr) => { _data.InsertRange(index, arr); return self; }, proceed);
            case "RemoveAt":
                return IfSingleInteger(args, index => { _data.RemoveAt(index); return self; }, proceed);
            case "Contains":
                return IfSingleArgument(args, arg => _data.Contains(arg), proceed);

            case "IndexOf":
                return IfSingleArgument(args, arg => _data.IndexOf(arg), proceed);

            case "Remove":
                return IfSingleArgument(args, arg => _data.Remove(arg), proceed);

            case "CopyTo":
                return IfArguments<object[], int>(args, (array, arrayIndex) =>
                {
                    _data.CopyTo(array, arrayIndex);
                    return self;
                }, proceed);
        }

        if (!args.Any())
        {
            return GetMember(proceed, self, name);
        }

        return proceed();
    }

    private static object IfArguments<T1, T2>(IEnumerable<object> args, Func<T1, T2, object> func, Func<object> proceed)
    {
        var objects = args as List<object> ?? args.ToList();
        return objects.Count != 2 ? proceed() : func((T1)objects[0], (T2)objects.Last());
    }

    private static object IfSingleArgument(IEnumerable<object> args, Func<object, object> func, Func<object> proceed)
    {
        var objects = args as List<object> ?? args.ToList();
        return objects.Count == 1 ? func(objects[0]) : proceed();
    }

    private static object IfSingleInteger(IEnumerable<object> args, Func<int, object> func, Func<object> proceed)
    {
        var objects = args as List<object> ?? args.ToList();
        return objects.Count != 1 ? proceed() : IfInitialInteger(objects, (index, _) => func(index), proceed);
    }

    private static object IfInitialInteger(IEnumerable<object> args, Func<int, IEnumerable<object>, object> func, Func<object> proceed)
    {
        var objects = args as List<object> ?? args.ToList();
        if (objects.Count > 0)
        {
            var key = objects[0];
            return key.GetType() != typeof(int) ? proceed() : func((int)key, objects.Skip(1));
        }

        return proceed();
    }

    private class EnumeratorBehavior : ClayBehavior
    {
        private readonly IEnumerator _enumerator;

        public EnumeratorBehavior(IEnumerator enumerator)
        {
            _enumerator = enumerator;
        }

        public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args)
        {
            switch (name)
            {
                case "MoveNext":
                    return _enumerator.MoveNext();

                case "Reset":
                    _enumerator.Reset();
                    return null;

                case "Dispose":
                    if (_enumerator is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    return null;
            }
            return proceed();
        }

        public override object GetMember(Func<object> proceed, object self, string name)
        {
            switch (name)
            {
                case "Current":
                    return _enumerator.Current;
            }
            return proceed();
        }
    }
}
