using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Masuit.Tools.Dynamics.Implementation;

namespace Masuit.Tools.Dynamics;

internal class ClayMetaObject : DynamicMetaObject
{
    private static readonly MethodInfo ClayBehaviorInvokeMember = typeof(IClayBehavior).GetMethod("InvokeMember");

    private static readonly MethodInfo ClayBehaviorGetMember = typeof(IClayBehavior).GetMethod("GetMember");
    private static readonly MethodInfo ClayBehaviorSetMember = typeof(IClayBehavior).GetMethod("SetMember");
    private static readonly MethodInfo ClayBehaviorGetIndex = typeof(IClayBehavior).GetMethod("GetIndex");
    private static readonly MethodInfo ClayBehaviorSetIndex = typeof(IClayBehavior).GetMethod("SetIndex");
    private static readonly MethodInfo ClayBehaviorBinaryOperation = typeof(IClayBehavior).GetMethod("BinaryOperation");
    private static readonly MethodInfo ClayBehaviorConvert = typeof(IClayBehavior).GetMethod("Convert");
    private static readonly MethodInfo ClayBehaviorInvokeMemberMissing = typeof(IClayBehavior).GetMethod("InvokeMemberMissing");
    private static readonly MethodInfo ClayBehaviorGetMemberMissing = typeof(IClayBehavior).GetMethod("GetMemberMissing");
    private static readonly MethodInfo ClayBehaviorSetMemberMissing = typeof(IClayBehavior).GetMethod("SetMemberMissing");
    private static readonly MethodInfo ClayBehaviorConvertMissing = typeof(IClayBehavior).GetMethod("ConvertMissing");

    public ClayMetaObject(object value, Expression expression) : base(expression, BindingRestrictions.Empty, value)
    {
    }

    public ClayMetaObject(object value, Expression expression, Func<Expression, Expression> getClayBehavior) : this(value, expression)
    {
        _getClayBehavior = getClayBehavior;
    }

    private Expression GetLimitedSelf()
    {
        if (Expression.Type == LimitType || Expression.Type.IsEquivalentTo(LimitType))
        {
            return Expression;
        }

        return Expression.Convert(Expression, LimitType);
    }

    private readonly Func<Expression, Expression> _getClayBehavior = expr => Expression.Property(Expression.Convert(expr, typeof(IClayBehaviorProvider)), "Behavior");

    protected virtual Expression GetClayBehavior()
    {
        return _getClayBehavior(Expression);
    }

    public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
    {
        var binderDefault = binder.FallbackGetMember(this);
        var missingLambda = Expression.Lambda(Expression.Call(GetClayBehavior(), ClayBehaviorGetMemberMissing, Expression.Lambda(binderDefault.Expression), GetLimitedSelf(), Expression.Constant(binder.Name, typeof(string))));
        var call = Expression.Call(GetClayBehavior(), ClayBehaviorGetMember, missingLambda, GetLimitedSelf(), Expression.Constant(binder.Name, typeof(string)));
        var dynamicSuggestion = new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderDefault.Restrictions));
        return binder.FallbackGetMember(this, dynamicSuggestion);
    }

    /// <inheritdoc />
    public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
    {
        var binderDefault = binder.FallbackSetMember(this, value);
        var missingLambda = Expression.Lambda(Expression.Call(GetClayBehavior(), ClayBehaviorSetMemberMissing, Expression.Lambda(binderDefault.Expression), GetLimitedSelf(), Expression.Constant(binder.Name, typeof(string)), Expression.Convert(value.Expression, typeof(object))));
        var call = Expression.Call(GetClayBehavior(), ClayBehaviorSetMember, missingLambda, GetLimitedSelf(), Expression.Constant(binder.Name, typeof(string)), Expression.Convert(value.Expression, typeof(object)));
        var dynamicSuggestion = new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderDefault.Restrictions));
        return binder.FallbackSetMember(this, value, dynamicSuggestion);
    }

    /// <inheritdoc />
    public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
    {
        var argValues = Expression.NewArrayInit(typeof(object), args.Select(x => Expression.Convert(x.Expression, typeof(Object))));
        var argNames = Expression.Constant(binder.CallInfo.ArgumentNames, typeof(IEnumerable<string>));
        var argNamedEnumerable = Expression.Call(typeof(Arguments).GetMethod("From"), argValues, argNames);
        var binderDefault = binder.FallbackInvokeMember(this, args);
        var missingLambda = Expression.Lambda(Expression.Call(GetClayBehavior(), ClayBehaviorInvokeMemberMissing, Expression.Lambda(binderDefault.Expression), GetLimitedSelf(), Expression.Constant(binder.Name, typeof(string)), argNamedEnumerable));
        var call = Expression.Call(GetClayBehavior(), ClayBehaviorInvokeMember, missingLambda, GetLimitedSelf(), Expression.Constant(binder.Name, typeof(string)), argNamedEnumerable);
        var dynamicSuggestion = new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderDefault.Restrictions));
        return binder.FallbackInvokeMember(this, args, dynamicSuggestion);
    }

    /// <inheritdoc />
    public override DynamicMetaObject BindConvert(ConvertBinder binder)
    {
        var binderDefault = binder.FallbackConvert(this);
        var missingLambda = Expression.Lambda(Expression.Call(GetClayBehavior(), ClayBehaviorConvertMissing, Expression.Lambda(Expression.Convert(binderDefault.Expression, typeof(object))), GetLimitedSelf(), Expression.Constant(binder.Type, typeof(Type)), Expression.Constant(binder.Explicit, typeof(bool))));
        var call = Expression.Call(GetClayBehavior(), ClayBehaviorConvert, missingLambda, GetLimitedSelf(), Expression.Constant(binder.Type, typeof(Type)), Expression.Constant(binder.Explicit, typeof(bool)));
        var convertedCall = Expression.Convert(call, binder.ReturnType);
        return new DynamicMetaObject(convertedCall, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderDefault.Restrictions));
    }

    /// <inheritdoc />
    public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
    {
        var a2 = Expression.NewArrayInit(typeof(string), indexes.Select(x => Expression.Convert(x.Expression, typeof(string))));
        var binderFallback = binder.FallbackGetIndex(this, indexes);
        var call = Expression.Call(GetClayBehavior(), ClayBehaviorGetIndex, Expression.Lambda(binderFallback.Expression), GetLimitedSelf(), a2);
        return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderFallback.Restrictions));
    }

    /// <inheritdoc />
    public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
    {
        var a2 = Expression.NewArrayInit(typeof(string), indexes.Select(x => Expression.Convert(x.Expression, typeof(string))));
        var binderFallback = binder.FallbackSetIndex(this, indexes, value);
        var call = Expression.Call(GetClayBehavior(), ClayBehaviorSetIndex, Expression.Lambda(binderFallback.Expression), GetLimitedSelf(), a2, Expression.Convert(value.Expression, typeof(object)));
        return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderFallback.Restrictions));
    }

    /// <inheritdoc />
    public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
    {
        var argValues = Expression.NewArrayInit(typeof(object), args.Select(x => Expression.Convert(x.Expression, typeof(Object))));
        var argNames = Expression.Constant(binder.CallInfo.ArgumentNames, typeof(IEnumerable<string>));
        var argNamedEnumerable = Expression.Call(typeof(Arguments).GetMethod("From"), argValues, argNames);
        var binderFallback = binder.FallbackInvoke(this, args);
        var call = Expression.Call(GetClayBehavior(), ClayBehaviorInvokeMember, Expression.Lambda(binderFallback.Expression), GetLimitedSelf(), Expression.Constant(null, typeof(string)), argNamedEnumerable);
        return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType).Merge(binderFallback.Restrictions));
    }

    /// <inheritdoc />
    public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
    {
        var binderFallback = binder.FallbackBinaryOperation(this, arg);
        var call = Expression.Call(GetClayBehavior(), ClayBehaviorBinaryOperation, Expression.Lambda(binderFallback.Expression), GetLimitedSelf(), Expression.Constant(binder.Operation, typeof(ExpressionType)), Expression.Convert(arg.Expression, typeof(object)));
        return new DynamicMetaObject(call, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetDynamicMemberNames()
    {
        throw new NotImplementedException();
    }
}
