using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Systems;

public class EnumExtTests
{
    [Fact]
    public void GetDictionary_ShouldReturnCorrectDictionary()
    {
        var dict = typeof(TestEnum).GetDictionary();
        Assert.Equal(3, dict.Count);
        Assert.Equal("First", dict[1]);
        Assert.Equal("Second", dict[2]);
        Assert.Equal("Third", dict[3]);
    }

    [Fact]
    public void GetEnumType_ShouldReturnCorrectType()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var type = assembly.GetEnumType("TestEnum");
        Assert.Equal(typeof(TestEnum), type);
    }

    [Fact]
    public void GetDisplay_ShouldReturnCorrectDisplay()
    {
        var display = TestEnum.First.GetDisplay();
        Assert.Equal("First", display);
    }

    [Fact]
    public void GetDescription_ShouldReturnCorrectDescription()
    {
        var description = TestEnum.First.GetDescription();
        Assert.Equal("First Value", description);
    }

    [Fact]
    public void GetAttributes_ShouldReturnCorrectAttributes()
    {
        var attributes = TestEnum.First.GetAttributes<DescriptionAttribute>().ToList();
        Assert.Single(attributes);
        Assert.Equal("First Value", attributes[0].Description);
    }

    [Fact]
    public void Split_ShouldReturnCorrectValues()
    {
        var values = FlagsEnum.FlagA | FlagsEnum.FlagB;
        var splitValues = values.Split().ToList();
        Assert.Contains(FlagsEnum.FlagA, splitValues);
        Assert.Contains(FlagsEnum.FlagB, splitValues);
    }

    [Fact]
    public void GetEnumDescription_ShouldReturnCorrectDescription()
    {
        var description = TestEnum.First.GetEnumDescription();
        Assert.NotNull(description);
        Assert.Equal("First Value", description.Description);
    }

    [Fact]
    public void GetTypedEnumDescriptions_ShouldReturnCorrectDescriptions()
    {
        var descriptions = TestEnum.First.GetTypedEnumDescriptions();
        Assert.Single(descriptions);
        Assert.Equal("First Value", descriptions["en"].Description);
    }

    [Fact]
    public void ToEnumString_ShouldReturnCorrectString()
    {
        var enumString = 1.ToEnumString(typeof(TestEnum));
        Assert.Equal("First", enumString);
    }

    [Fact]
    public void GetDescriptionAndValue_ShouldReturnCorrectDictionary()
    {
        var dict = typeof(TestEnum).GetDescriptionAndValue();
        Assert.Equal(3, dict.Count);
        Assert.Equal(1, dict["First Value"]);
        Assert.Equal(2, dict["Second Value"]);
        Assert.Equal(3, dict["Third Value"]);
    }
}