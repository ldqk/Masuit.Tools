using System;
using System.Reflection;
using Masuit.Tools.Reflection;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Refection;

public class ReflectionUtilTests
{
    private class TestClass
    {
        public string TestProperty { get; set; }
        public string TestField;

        public string TestMethod(string input) => $"Hello, {input}";
    }

    [Fact]
    public void InvokeMethod_ShouldInvokeMethodAndReturnResult()
    {
        var obj = new TestClass();
        var result = obj.InvokeMethod<string>("TestMethod", new object[] { "World" });
        Assert.Equal("Hello, World", result);
    }

    [Fact]
    public void InvokeMethod_ShouldInvokeMethodWithoutReturn()
    {
        var obj = new TestClass();
        obj.InvokeMethod("TestMethod", new object[] { "World" });
    }

    [Fact]
    public void SetField_ShouldSetFieldValue()
    {
        var obj = new TestClass();
        obj.SetField("TestField", "New Value");
        Assert.Equal("New Value", obj.TestField);
    }

    [Fact]
    public void GetField_ShouldGetFieldValue()
    {
        var obj = new TestClass { TestField = "Field Value" };
        var result = obj.GetField<string>("TestField");
        Assert.Equal("Field Value", result);
    }

    [Fact]
    public void GetFields_ShouldReturnAllFields()
    {
        var obj = new TestClass();
        var fields = obj.GetFields();
        Assert.Contains(fields, f => f.Name == "TestField");
    }

    [Fact]
    public void SetProperty_ShouldSetPropertyValue()
    {
        var obj = new TestClass();
        obj.SetProperty("TestProperty", "New Value");
        Assert.Equal("New Value", obj.TestProperty);
    }

    [Fact]
    public void GetProperty_ShouldGetPropertyValue()
    {
        var obj = new TestClass { TestProperty = "Property Value" };
        var result = obj.GetProperty<string>("TestProperty");
        Assert.Equal("Property Value", result);
    }

    [Fact]
    public void GetProperties_ShouldReturnAllProperties()
    {
        var obj = new TestClass();
        var properties = obj.GetProperties();
        Assert.Contains(properties, p => p.Name == "TestProperty");
    }

    [Fact]
    public void GetDescription_ShouldReturnDescription()
    {
        var member = typeof(TestClass).GetProperty("TestProperty");
        var description = member.GetDescription();
        Assert.Equal(string.Empty, description);
    }

    [Fact]
    public void GetAttribute_ShouldReturnAttribute()
    {
        var member = typeof(TestClass).GetProperty("TestProperty");
        var attribute = member.GetAttribute<ObsoleteAttribute>();
        Assert.Null(attribute);
    }

    [Fact]
    public void GetAttributes_ShouldReturnAttributes()
    {
        var member = typeof(TestClass).GetProperty("TestProperty");
        var attributes = member.GetAttributes<ObsoleteAttribute>();
        Assert.Empty(attributes);
    }

    [Fact]
    public void GetImageResource_ShouldReturnStream()
    {
        var stream = Assembly.GetExecutingAssembly().GetImageResource("resourceName");
        Assert.Null(stream);
    }

    [Fact]
    public void GetManifestString_ShouldReturnString()
    {
        var result = typeof(TestClass).GetManifestString("UTF-8", "resName");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void IsImplementsOf_ShouldReturnTrueForImplementedInterface()
    {
        var result = typeof(TestClass).IsImplementsOf(typeof(IDisposable));
        Assert.False(result);
    }

    [Fact]
    public void GetInstance_ShouldReturnInstance()
    {
        var instance = typeof(TestClass).GetInstance();
        Assert.NotNull(instance);
    }

    [Fact]
    public void GetLoadableTypes_ShouldReturnTypes()
    {
        var types = Assembly.GetExecutingAssembly().GetLoadableTypes();
        Assert.NotEmpty(types);
    }

    [Fact]
    public void GetLoadableExportedTypes_ShouldReturnExportedTypes()
    {
        var types = Assembly.GetExecutingAssembly().GetLoadableExportedTypes();
        Assert.NotEmpty(types);
    }
}