using System;
using System.Collections.Generic;
using Masuit.Tools.Dynamics;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions;

public class DynamicObjectTest
{
    [Fact]
    public void Can_Dynamic()
    {
        var obj = DynamicFactory.NewObject();
        obj.Name = "Masuit";
        obj.Age = 18;
        obj.MyClass = DynamicFactory.WithObject(new
        {
            X = 10,
            Y = 20,
            Z = new List<int> { 1, 2, 3, 4, 5 }
        });
        Assert.Equal(obj.Name, obj["Name"]);
        Assert.Equal(obj["MyClass"]["X"], obj.MyClass.X);
        Assert.Equal(obj.MyClass.Z[2], obj["MyClass"]["Z"][2]);
    }

    [Fact]
    public void Can_ToDynamic()
    {
        var obj = new
        {
            Name = "Masuit"
        }.ToDynamic();
        obj.Age = 18;
        obj.MyClass = new
        {
            X = 10,
            Y = 20,
            Z = new List<int> { 1, 2, 3, 4, 5 }
        }.ToDynamic();
        obj.Prop = "test";
        _ = obj - "Prop";

        Assert.Equal(obj.Name, obj["Name"]);
        Assert.Equal(obj["MyClass"]["X"], obj.MyClass.X);
        Assert.IsType<Clay>(obj.Prop);
    }

    [Fact]
    public void NewObject_ShouldCreateDynamicObject()
    {
        dynamic obj = DynamicFactory.NewObject();
        obj.Name = "Test";
        Assert.Equal("Test", obj.Name);
    }

    [Fact]
    public void WithObject_ShouldWrapExistingObject()
    {
        var existingObject = new { Name = "Test" };
        dynamic obj = DynamicFactory.WithObject(existingObject);
        Assert.Equal("Test", obj.Name);
    }

    [Fact]
    public void NewObject_ShouldAllowAddingProperties()
    {
        dynamic obj = DynamicFactory.NewObject();
        obj.Name = "Test";
        obj.Age = 30;
        Assert.Equal("Test", obj.Name);
        Assert.Equal(30, obj.Age);
    }

    [Fact]
    public void WithObject_ShouldAllowAddingPropertiesToExistingObject()
    {
        var existingObject = new { Name = "Test" };
        dynamic obj = DynamicFactory.WithObject(existingObject);
        obj.Age = 30;
        Assert.Equal("Test", obj.Name);
        Assert.Equal(30, obj.Age);
    }
}