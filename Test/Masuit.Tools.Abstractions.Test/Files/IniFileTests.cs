using System.IO;
using System.Threading.Tasks;
using Masuit.Tools.Files;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Files;

public class IniFileTests
{
    private readonly string _testFilePath = Path.Combine(Path.GetTempPath(), "test.ini");

    public IniFileTests()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Fact]
    public void Constructor_ShouldLoadExistingFile()
    {
        File.WriteAllText(_testFilePath, "[Section1]\nKey1=Value1\nKey2=Value2");
        var iniFile = new IniFile(_testFilePath);
        var section = iniFile.GetSection("Section1");
        Assert.Equal("Value1", section["Key1"]);
        Assert.Equal("Value2", section["Key2"]);
    }

    [Fact]
    public void Reload_ShouldReloadFile()
    {
        File.WriteAllText(_testFilePath, "[Section1]\nKey1=Value1\nKey2=Value2");
        var iniFile = new IniFile(_testFilePath);
        File.WriteAllText(_testFilePath, "[Section1]\nKey1=NewValue1\nKey2=NewValue2");
        iniFile.Reload();
        var section = iniFile.GetSection("Section1");
        Assert.Equal("NewValue1", section["Key1"]);
        Assert.Equal("NewValue2", section["Key2"]);
    }

    [Fact]
    public void Save_ShouldSaveToFile()
    {
        var iniFile = new IniFile(_testFilePath);
        iniFile.SetValue("Section1", "Key1", "Value1");
        iniFile.SetValue("Section1", "Key2", "Value2");
        iniFile.Save();
        var content = File.ReadAllText(_testFilePath);
        Assert.Contains("[Section1]", content);
        Assert.Contains("Key1=Value1", content);
        Assert.Contains("Key2=Value2", content);
    }

    [Fact]
    public async Task SaveAsync_ShouldSaveToFile()
    {
        var iniFile = new IniFile(_testFilePath);
        iniFile.SetValue("Section1", "Key1", "Value1");
        iniFile.SetValue("Section1", "Key2", "Value2");
        await iniFile.SaveAsync();
        var content = await File.ReadAllTextAsync(_testFilePath);
        Assert.Contains("[Section1]", content);
        Assert.Contains("Key1=Value1", content);
        Assert.Contains("Key2=Value2", content);
    }

    [Fact]
    public void GetValue_ShouldReturnDefaultValueIfKeyNotFound()
    {
        var iniFile = new IniFile(_testFilePath);
        var value = iniFile.GetValue("Section1", "Key1", "DefaultValue");
        Assert.Equal("DefaultValue", value);
    }

    [Fact]
    public void GetValue_ShouldReturnValueIfKeyFound()
    {
        var iniFile = new IniFile(_testFilePath);
        iniFile.SetValue("Section1", "Key1", "Value1");
        var value = iniFile.GetValue("Section1", "Key1");
        Assert.Equal("Value1", value);
    }

    [Fact]
    public void GetValue_Generic_ShouldReturnConvertedValue()
    {
        var iniFile = new IniFile(_testFilePath);
        iniFile.SetValue("Section1", "Key1", "123");
        var value = iniFile.GetValue<int>("Section1", "Key1");
        Assert.Equal(123, value);
    }

    [Fact]
    public void GetSections_ShouldReturnAllSections()
    {
        var iniFile = new IniFile(_testFilePath);
        iniFile.SetValue("Section1", "Key1", "Value1");
        iniFile.SetValue("Section2", "Key2", "Value2");
        var sections = iniFile.GetSections();
        Assert.Contains(sections, s => s.Name == "Section1");
        Assert.Contains(sections, s => s.Name == "Section2");
    }

    [Fact]
    public void GetSection_ShouldReturnAllKeyValuesInSection()
    {
        var iniFile = new IniFile(_testFilePath);
        iniFile.SetValue("Section1", "Key1", "Value1");
        iniFile.SetValue("Section1", "Key2", "Value2");
        var section = iniFile.GetSection("Section1");
        Assert.Equal("Value1", section["Key1"]);
        Assert.Equal("Value2", section["Key2"]);
    }

    [Fact]
    public void GetSection_Generic_ShouldReturnBoundObject()
    {
        var iniFile = new IniFile(_testFilePath);
        iniFile.SetValue("Section1", "Key1", "Value1");
        iniFile.SetValue("Section1", "Key2", "Value2");
        var section = iniFile.GetSection<TestSection>("Section1");
        Assert.Equal("Value1", section.Key1);
        Assert.Equal("Value2", section.Key2);
    }

    [Fact]
    public void SetValue_ShouldSetKeyValue()
    {
        var iniFile = new IniFile(_testFilePath);
        iniFile.SetValue("Section1", "Key1", "Value1");
        var value = iniFile.GetValue("Section1", "Key1");
        Assert.Equal("Value1", value);
    }

    [Fact]
    public void SetValue_Bool_ShouldSetKeyValue()
    {
        var iniFile = new IniFile(_testFilePath);
        iniFile.SetValue("Section1", "Key1", true);
        var value = iniFile.GetValue("Section1", "Key1");
        Assert.Equal("true", value);
    }

    [Fact]
    public void SetValue_Generic_ShouldSetKeyValue()
    {
        var iniFile = new IniFile(_testFilePath);
        iniFile.SetValue("Section1", "Key1", 123);
        var value = iniFile.GetValue("Section1", "Key1");
        Assert.Equal("123", value);
    }

    [Fact]
    public void ClearSection_ShouldClearAllKeysInSection()
    {
        var iniFile = new IniFile(_testFilePath);
        iniFile.SetValue("Section1", "Key1", "Value1");
        iniFile.ClearSection("Section1");
        var section = iniFile.GetSection("Section1");
        Assert.Empty(section);
    }

    [Fact]
    public void ClearAllSection_ShouldClearAllSections()
    {
        var iniFile = new IniFile(_testFilePath);
        iniFile.SetValue("Section1", "Key1", "Value1");
        iniFile.SetValue("Section2", "Key2", "Value2");
        iniFile.ClearAllSection();
        var sections = iniFile.GetSections();
        Assert.Empty(sections);
    }

    private class TestSection
    {
        public string Key1 { get; set; }
        public string Key2 { get; set; }
    }
}