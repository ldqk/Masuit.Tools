using System;

namespace Masuit.Tools.Files;

public class IniPropertyAttribute : Attribute
{
    public string Name { get; set; }

    public IniPropertyAttribute(string name)
    {
        Name = name;
    }
}