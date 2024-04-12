using System;
using System.Collections.Generic;

namespace Masuit.Tools.Files;

public class IniSection(string name, StringComparer comparer) : Dictionary<string, IniItem>(comparer)
{
    public string Name { get; set; } = name;
}