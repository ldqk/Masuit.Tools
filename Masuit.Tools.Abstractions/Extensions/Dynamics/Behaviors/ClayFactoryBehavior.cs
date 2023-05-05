using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;

namespace Masuit.Tools.Dynamics.Behaviors;

internal class ClayFactoryBehavior : ClayBehavior
{
    /// <inheritdoc />
    public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args)
    {
        dynamic shape = new Clay(new InterfaceProxyBehavior(), new PropBehavior(), new ArrayPropAssignmentBehavior(), new NullResultBehavior());
        shape.ShapeName = name;
        if (args.Positional.Count() == 1)
        {
            var options = args.Positional.Single();
            var assigner = GetAssigner(options.GetType());
            assigner.Invoke(shape, options);
        }

        foreach (var kv in args.Named)
        {
            shape[kv.Key] = kv.Value;
        }

        return shape;
    }

    private static Action<dynamic, object> GetAssigner(Type sourceType)
    {
        lock (AssignerCache)
        {
            if (AssignerCache.TryGetValue(sourceType, out var assigner))
            {
                return assigner;
            }

            var targetParameter = Expression.Parameter(typeof(object), "target");
            var sourceParameter = Expression.Parameter(typeof(object), "source");
            var assignments = sourceType.GetProperties().Select(property => Expression.Dynamic(Binder.SetMember(CSharpBinderFlags.None, property.Name, typeof(void), new[]
            {
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
            }), typeof(void), targetParameter, Expression.Property(Expression.Convert(sourceParameter, sourceType), property)));
            var lambda = Expression.Lambda<Action<dynamic, object>>(Expression.Block(assignments), targetParameter, sourceParameter);
            assigner = lambda.Compile();
            AssignerCache.Add(sourceType, assigner);
            return assigner;
        }
    }

    private static readonly Dictionary<Type, Action<dynamic, object>> AssignerCache = new();
}
