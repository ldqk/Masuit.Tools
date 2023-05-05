using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using Castle.DynamicProxy;
using Masuit.Tools.Dynamics.Implementation;
using Microsoft.CSharp.RuntimeBinder;

namespace Masuit.Tools.Dynamics;

internal class ClayInterceptor : IInterceptor
{
    private const string GetPrefix = "get_";
    private const string SetPrefix = "set_";

    public void Intercept(IInvocation invocation)
    {
        var invocationDestinedForSelf = ReferenceEquals(invocation.InvocationTarget, invocation.Proxy);
        if (!invocationDestinedForSelf)
        {
            invocation.Proceed();
            return;
        }

        if (invocation.Proxy is IClayBehaviorProvider behaviorProvider)
        {
            var invocationMethod = invocation.Method;
            switch (invocationMethod.IsSpecialName)
            {
                case true when invocationMethod.Name.StartsWith(GetPrefix):
                    invocation.ReturnValue = behaviorProvider.Behavior.GetMember(() =>
                    {
                        invocation.Proceed();
                        return invocation.ReturnValue;
                    }, invocation.Proxy, invocationMethod.Name.Substring(GetPrefix.Length));
                    AdjustReturnValue(invocation);
                    return;

                case true when invocationMethod.Name.StartsWith(SetPrefix) && invocation.Arguments.Length == 1:
                    invocation.ReturnValue = behaviorProvider.Behavior.SetMember(() =>
                    {
                        invocation.Proceed();
                        return invocation.ReturnValue;
                    }, invocation.Proxy, invocationMethod.Name.Substring(SetPrefix.Length), invocation.Arguments.Single());
                    AdjustReturnValue(invocation);
                    return;

                case false:
                    invocation.ReturnValue = behaviorProvider.Behavior.InvokeMember(() =>
                    {
                        invocation.Proceed();
                        return invocation.ReturnValue;
                    }, invocation.Proxy, invocationMethod.Name, Arguments.From(invocation.Arguments, Enumerable.Empty<string>()));
                    AdjustReturnValue(invocation);
                    return;
            }
        }

        invocation.Proceed();
    }

    private static readonly ConcurrentDictionary<Type, CallSite<Func<CallSite, object, object>>> ConvertSites = new();

    private static void AdjustReturnValue(IInvocation invocation)
    {
        var methodReturnType = invocation.Method.ReturnType;
        if (methodReturnType == typeof(void))
        {
            return;
        }

        if (invocation.ReturnValue == null)
        {
            return;
        }

        var returnValueType = invocation.ReturnValue.GetType();
        if (methodReturnType.IsAssignableFrom(returnValueType))
        {
            return;
        }

        var callSite = ConvertSites.GetOrAdd(methodReturnType, x => CallSite<Func<CallSite, object, object>>.Create(Binder.Convert(CSharpBinderFlags.None, x, null)));
        invocation.ReturnValue = callSite.Target(callSite, invocation.ReturnValue);
    }
}
