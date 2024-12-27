using System;
using System.Linq.Expressions;
using Masuit.Tools.Linq;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Linq;

public class LinqExtensionTests
{
    [Fact]
    public void And_ShouldCombineExpressionsWithAnd()
    {
        Expression<Func<int, bool>> expr1 = x => x > 5;
        Expression<Func<int, bool>> expr2 = x => x < 10;

        var combined = expr1.And(expr2);

        Assert.True(combined.Compile()(7));
        Assert.False(combined.Compile()(4));
        Assert.False(combined.Compile()(11));
    }

    [Fact]
    public void AndIf_ShouldCombineExpressionsWithAndIfConditionIsTrue()
    {
        Expression<Func<int, bool>> expr1 = x => x > 5;
        Expression<Func<int, bool>> expr2 = x => x < 10;

        var combined = expr1.AndIf(true, expr2);

        Assert.True(combined.Compile()(7));
        Assert.False(combined.Compile()(4));
        Assert.False(combined.Compile()(11));
    }

    [Fact]
    public void AndIf_ShouldNotCombineExpressionsWithAndIfConditionIsFalse()
    {
        Expression<Func<int, bool>> expr1 = x => x > 5;
        Expression<Func<int, bool>> expr2 = x => x < 10;

        var combined = expr1.AndIf(false, expr2);

        Assert.True(combined.Compile()(7));
        Assert.True(combined.Compile()(11));
    }

    [Fact]
    public void AndIf_WithFuncCondition_ShouldCombineExpressionsWithAndIfConditionIsTrue()
    {
        Expression<Func<int, bool>> expr1 = x => x > 5;
        Expression<Func<int, bool>> expr2 = x => x < 10;

        var combined = expr1.AndIf(() => true, expr2);

        Assert.True(combined.Compile()(7));
        Assert.False(combined.Compile()(4));
        Assert.False(combined.Compile()(11));
    }

    [Fact]
    public void AndIf_WithFuncCondition_ShouldNotCombineExpressionsWithAndIfConditionIsFalse()
    {
        Expression<Func<int, bool>> expr1 = x => x > 5;
        Expression<Func<int, bool>> expr2 = x => x < 10;

        var combined = expr1.AndIf(() => false, expr2);

        Assert.True(combined.Compile()(7));
        Assert.True(combined.Compile()(11));
    }

    [Fact]
    public void Or_ShouldCombineExpressionsWithOr()
    {
        Expression<Func<int, bool>> expr1 = x => x > 5;
        Expression<Func<int, bool>> expr2 = x => x < 10;

        var combined = expr1.Or(expr2);

        Assert.True(combined.Compile()(7));
        Assert.True(combined.Compile()(4));
        Assert.True(combined.Compile()(11));
    }

    [Fact]
    public void OrIf_ShouldCombineExpressionsWithOrIfConditionIsTrue()
    {
        Expression<Func<int, bool>> expr1 = x => x > 5;
        Expression<Func<int, bool>> expr2 = x => x < 10;

        var combined = expr1.OrIf(true, expr2);

        Assert.True(combined.Compile()(7));
        Assert.True(combined.Compile()(4));
        Assert.True(combined.Compile()(11));
    }

    [Fact]
    public void OrIf_ShouldNotCombineExpressionsWithOrIfConditionIsFalse()
    {
        Expression<Func<int, bool>> expr1 = x => x > 5;
        Expression<Func<int, bool>> expr2 = x => x < 10;

        var combined = expr1.OrIf(false, expr2);

        Assert.True(combined.Compile()(7));
        Assert.False(combined.Compile()(4));
        Assert.True(combined.Compile()(11));
    }

    [Fact]
    public void OrIf_WithFuncCondition_ShouldCombineExpressionsWithOrIfConditionIsTrue()
    {
        Expression<Func<int, bool>> expr1 = x => x > 5;
        Expression<Func<int, bool>> expr2 = x => x < 10;

        var combined = expr1.OrIf(() => true, expr2);

        Assert.True(combined.Compile()(7));
        Assert.True(combined.Compile()(4));
        Assert.True(combined.Compile()(11));
    }

    [Fact]
    public void OrIf_WithFuncCondition_ShouldNotCombineExpressionsWithOrIfConditionIsFalse()
    {
        Expression<Func<int, bool>> expr1 = x => x > 5;
        Expression<Func<int, bool>> expr2 = x => x < 10;

        var combined = expr1.OrIf(() => false, expr2);

        Assert.True(combined.Compile()(7));
        Assert.False(combined.Compile()(4));
        Assert.True(combined.Compile()(11));
    }
}