using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace Masuit.Tools.Dynamics.Behaviors;

internal class InterfaceProxyBehavior : ClayBehavior
{
    private static readonly IProxyBuilder ProxyBuilder = new DefaultProxyBuilder();
    private static readonly MethodInfo DynamicMetaObjectProviderGetMetaObject = typeof(IDynamicMetaObjectProvider).GetMethod("GetMetaObject");

    /// <inheritdoc />
    public override object ConvertMissing(Func<object> proceed, object self, Type type, bool isExplicit)
    {
        if (!type.IsInterface || type == typeof(IDynamicMetaObjectProvider))
        {
            return proceed();
        }

        var proxyType = ProxyBuilder.CreateInterfaceProxyTypeWithoutTarget(type, new[]
        {
            typeof(IDynamicMetaObjectProvider)
        }, ProxyGenerationOptions.Default);

        var interceptors = new IInterceptor[]
        {
            new Interceptor(self)
        };
        return Activator.CreateInstance(proxyType, interceptors, self);
    }

    private class Interceptor : IInterceptor
    {
        private object Self { get; }

        public Interceptor(object self)
        {
            Self = self;
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method == DynamicMetaObjectProviderGetMetaObject)
            {
                var expression = (Expression)invocation.Arguments.Single();
                invocation.ReturnValue = new ForwardingMetaObject(expression, BindingRestrictions.Empty, invocation.Proxy, (IDynamicMetaObjectProvider)Self, exprProxy => Expression.Field(exprProxy, "__target"));
                return;
            }

            var invoker = BindInvoker(invocation);
            invoker(invocation);

            if (invocation.ReturnValue == null || invocation.Method.ReturnType.IsInstanceOfType(invocation.ReturnValue) || !(invocation.ReturnValue is IClayBehaviorProvider provider))
            {
                return;
            }

            var returnValueBehavior = provider.Behavior;
            invocation.ReturnValue = returnValueBehavior.Convert(() => returnValueBehavior.ConvertMissing(() => invocation.ReturnValue, invocation.ReturnValue, invocation.Method.ReturnType, false), provider, invocation.Method.ReturnType, false);
        }

        private static readonly ConcurrentDictionary<MethodInfo, Action<IInvocation>> Invokers = new();

        private static Action<IInvocation> BindInvoker(IInvocation invocation)
        {
            return Invokers.GetOrAdd(invocation.Method, CompileInvoker);
        }

        private static Action<IInvocation> CompileInvoker(MethodInfo method)
        {
            var methodParameters = method.GetParameters();
            var invocationParameter = Expression.Parameter(typeof(IInvocation), "invocation");

            var targetAndArgumentInfos = Pack(CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), methodParameters.Select(mp => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.NamedArgument, mp.Name))).ToArray();

            var targetAndArguments = Pack<Expression>(Expression.Property(invocationParameter, invocationParameter.Type, "Proxy"), methodParameters.Select((mp, index) => Expression.Convert(Expression.ArrayIndex(Expression.Property(invocationParameter, invocationParameter.Type, "Arguments"), Expression.Constant(index)), mp.ParameterType))).ToArray();

            Expression body = null;
            if (method.IsSpecialName)
            {
                switch (method.Name)
                {
                    case "get_Item":
                        body = Expression.Dynamic(Binder.GetIndex(CSharpBinderFlags.InvokeSpecialName, typeof(object), targetAndArgumentInfos), typeof(object), targetAndArguments);
                        break;

                    case "set_Item":
                        var targetAndArgumentInfosWithoutTheNameValue = Pack(CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), methodParameters.Select(mp => mp.Name == "value" ? CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) : CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.NamedArgument, mp.Name)));
                        body = Expression.Dynamic(Binder.SetIndex(CSharpBinderFlags.InvokeSpecialName, typeof(object), targetAndArgumentInfosWithoutTheNameValue), typeof(object), targetAndArguments);
                        break;

                    case { } s when s.StartsWith("get_"):
                        body = Expression.Dynamic(Binder.GetMember(CSharpBinderFlags.InvokeSpecialName, method.Name.Substring("get_".Length), typeof(object), targetAndArgumentInfos), typeof(object), targetAndArguments);
                        break;

                    case { } s when s.StartsWith("set_"):
                        body = Expression.Dynamic(Binder.SetMember(CSharpBinderFlags.InvokeSpecialName, method.Name.Substring("set_".Length), typeof(object), targetAndArgumentInfos), typeof(object), targetAndArguments);
                        break;
                }
            }

            body ??= Expression.Dynamic(Binder.InvokeMember(CSharpBinderFlags.None, method.Name, null, typeof(object), targetAndArgumentInfos), typeof(object), targetAndArguments);

            if (method.ReturnType != typeof(void))
            {
                body = Expression.Assign(Expression.Property(invocationParameter, invocationParameter.Type, "ReturnValue"), Expression.Convert(body, typeof(object)));
            }

            var lambda = Expression.Lambda<Action<IInvocation>>(body, invocationParameter);
            return lambda.Compile();
        }
    }

    private static IEnumerable<T> Pack<T>(T t1, IEnumerable<T> t2)
    {
        if (!Equals(t1, default(T)))
        {
            yield return t1;
        }

        foreach (var t in t2)
        {
            yield return t;
        }
    }

    /// <inheritdoc />
    public sealed class ForwardingMetaObject : DynamicMetaObject
    {
        private readonly DynamicMetaObject _metaForwardee;

        /// <inheritdoc />
        public ForwardingMetaObject(Expression expression, BindingRestrictions restrictions, object forwarder, IDynamicMetaObjectProvider forwardee, Func<Expression, Expression> forwardeeGetter) : base(expression, restrictions, forwarder)
        {
            _metaForwardee = forwardee.GetMetaObject(forwardeeGetter(Expression.Convert(expression, forwarder.GetType())));
        }

        private DynamicMetaObject AddRestrictions(DynamicMetaObject result)
        {
            return new DynamicMetaObject(result.Expression, BindingRestrictions.GetTypeRestriction(Expression, Value.GetType()).Merge(result.Restrictions), _metaForwardee.Value);
        }

        /// <inheritdoc />
        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            return AddRestrictions(_metaForwardee.BindGetMember(binder));
        }

        /// <inheritdoc />
        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            return AddRestrictions(_metaForwardee.BindSetMember(binder, value));
        }

        /// <inheritdoc />
        public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
        {
            return AddRestrictions(_metaForwardee.BindDeleteMember(binder));
        }

        /// <inheritdoc />
        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return AddRestrictions(_metaForwardee.BindGetIndex(binder, indexes));
        }

        /// <inheritdoc />
        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            return AddRestrictions(_metaForwardee.BindSetIndex(binder, indexes, value));
        }

        /// <inheritdoc />
        public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return AddRestrictions(_metaForwardee.BindDeleteIndex(binder, indexes));
        }

        /// <inheritdoc />
        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            return AddRestrictions(_metaForwardee.BindInvokeMember(binder, args));
        }

        /// <inheritdoc />
        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        {
            return AddRestrictions(_metaForwardee.BindInvoke(binder, args));
        }

        /// <inheritdoc />
        public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
        {
            return AddRestrictions(_metaForwardee.BindCreateInstance(binder, args));
        }

        /// <inheritdoc />
        public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
        {
            return AddRestrictions(_metaForwardee.BindUnaryOperation(binder));
        }

        /// <inheritdoc />
        public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
        {
            return AddRestrictions(_metaForwardee.BindBinaryOperation(binder, arg));
        }

        /// <inheritdoc />
        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            return AddRestrictions(_metaForwardee.BindConvert(binder));
        }
    }
}
