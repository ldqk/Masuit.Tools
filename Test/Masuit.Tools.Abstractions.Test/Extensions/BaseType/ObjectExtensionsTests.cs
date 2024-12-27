using System;
using System.Collections.Generic;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Extensions.BaseType;

public class ObjectExtensionsTests
{
    [Fact]
    public void IsPrimitive_ShouldReturnTrue_ForPrimitiveTypes()
    {
        Assert.True(typeof(int).IsPrimitive());
        Assert.True(typeof(string).IsPrimitive());
        Assert.False(typeof(DateTime).IsPrimitive());
    }

    [Fact]
    public void IsSimpleType_ShouldReturnTrue_ForSimpleTypes()
    {
        Assert.True(typeof(int).IsSimpleType());
        Assert.True(typeof(string).IsSimpleType());
        Assert.True(typeof(decimal).IsSimpleType());
        Assert.False(typeof(DateTime).IsSimpleType());
    }

    [Fact]
    public void IsSimpleArrayType_ShouldReturnTrue_ForSimpleArrayTypes()
    {
        Assert.True(typeof(int[]).IsSimpleArrayType());
        Assert.False(typeof(DateTime[]).IsSimpleArrayType());
    }

    [Fact]
    public void IsSimpleListType_ShouldReturnTrue_ForSimpleListTypes()
    {
        Assert.True(typeof(List<int>).IsSimpleListType());
        Assert.False(typeof(List<DateTime>).IsSimpleListType());
    }

    [Fact]
    public void IsDefaultValue_ShouldReturnTrue_ForDefaultValues()
    {
        Assert.True(0.IsDefaultValue());
        Assert.True(default(DateTime).IsDefaultValue());
        Assert.False(1.IsDefaultValue());
    }

    [Fact]
    public void DeepClone_ShouldCloneObject()
    {
        var original = new TestClass { Id = 1, Name = "Test", List = [1, 2, 3] };
        var clone = original.DeepClone();

        Assert.NotSame(original, clone);
        Assert.Equal(original.Id, clone.Id);
        Assert.Equal(original.Name, clone.Name);
    }

    [Fact]
    public void IsNullOrEmpty_ShouldReturnTrue_ForNullOrEmptyValues()
    {
        Assert.True(((string)null).IsNullOrEmpty());
        Assert.True("".IsNullOrEmpty());
        Assert.False("Test".IsNullOrEmpty());
    }

    [Fact]
    public void IfNull_ShouldReturnDefaultValue_WhenNull()
    {
        string value = null;
        Assert.Equal("default", value.IfNull("default"));
    }

    [Fact]
    public void ToJsonString_ShouldReturnJsonString()
    {
        var obj = new { Id = 1, Name = "Test" };
        var json = obj.ToJsonString();
        Assert.Equal("{\"Id\":1,\"Name\":\"Test\"}", json);
    }

    [Fact]
    public void FromJson_ShouldReturnObject_FromJsonString()
    {
        var json = "{\"Id\":1,\"Name\":\"Test\"}";
        var obj = json.FromJson<TestClass>();
        Assert.Equal(1, obj.Id);
        Assert.Equal("Test", obj.Name);
    }

    [Fact]
    public void ToDictionary_ShouldConvertObjectToDictionary()
    {
        var obj = new { Id = 1, Name = "Test" };
        var dict = obj.ToDictionary();
        Assert.Equal(1, dict["Id"]);
        Assert.Equal("Test", dict["Name"]);
    }

    [Fact]
    public void ToDynamic_ShouldConvertObjectToDynamic()
    {
        var obj = new { Id = 1, Name = "Test" };
        dynamic dynamicObj = obj.ToDynamic();
        Assert.Equal(1, dynamicObj.Id);
        Assert.Equal("Test", dynamicObj.Name);
    }

    [Fact]
    public void Merge_ShouldMergeObjects()
    {
        var obj1 = new TestClass { Id = 1 };
        var obj2 = new TestClass { Name = "Test" };
        var merged = obj1.Merge(obj2);
        Assert.Equal(1, merged.Id);
        Assert.Equal("Test", merged.Name);
    }

    private class TestClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> List { get; set; }
    }
}