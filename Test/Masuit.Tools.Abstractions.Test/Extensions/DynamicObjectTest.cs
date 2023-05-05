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
}
