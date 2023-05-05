using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Castle.DynamicProxy;

namespace Masuit.Tools.Dynamics;

internal class DefaultClayActivator : IClayActivator
{
    private static readonly IProxyBuilder _builder = new DefaultProxyBuilder();

    public dynamic CreateInstance(Type baseType, IEnumerable<IClayBehavior> behaviors, IEnumerable<object> arguments)
    {
        var isDynamicMetaObjectProvider = typeof(IDynamicMetaObjectProvider).IsAssignableFrom(baseType);
        var isClayBehaviorProvider = typeof(IClayBehaviorProvider).IsAssignableFrom(baseType);
        if (isDynamicMetaObjectProvider && isClayBehaviorProvider)
        {
            var constructorArguments = new object[]
            {
                behaviors
            };
            return Activator.CreateInstance(baseType, constructorArguments);
        }

        Func<object, object> contextualize = proxy => proxy;
        var options = new ProxyGenerationOptions();
        var constructorArgs = new List<object>();
        if (!isClayBehaviorProvider)
        {
            var mixin = new MixinClayBehaviorProvider(behaviors);
            options.AddMixinInstance(mixin);
            constructorArgs.Add(mixin);
        }

        if (!isDynamicMetaObjectProvider)
        {
            var mixin = new MixinDynamicMetaObjectProvider();
            options.AddMixinInstance(mixin);
            constructorArgs.Add(mixin);
            var prior = contextualize;
            contextualize = proxy =>
            {
                mixin.Instance = proxy;
                return prior(proxy);
            };
        }

        var proxyType = _builder.CreateClassProxyType(baseType, Type.EmptyTypes, options);
        constructorArgs.Add(new IInterceptor[]
        {
            new ClayInterceptor()
        });
        if (arguments != null)
        {
            constructorArgs.AddRange(arguments);
        }

        return contextualize(Activator.CreateInstance(proxyType, constructorArgs.ToArray()));
    }

    private class MixinClayBehaviorProvider : IClayBehaviorProvider
    {
        private readonly IClayBehavior _behavior;

        public MixinClayBehaviorProvider(IEnumerable<IClayBehavior> behaviors)
        {
            _behavior = new ClayBehaviorCollection(behaviors);
        }

        IClayBehavior IClayBehaviorProvider.Behavior => _behavior;
    }

    private class MixinDynamicMetaObjectProvider : IDynamicMetaObjectProvider
    {
        public object Instance { get; set; }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression expression)
        {
            return new ClayMetaObject(Instance, expression);
        }
    }
}
