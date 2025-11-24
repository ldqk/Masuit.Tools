using System;

namespace Masuit.Tools.Excel;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnFormatAttribute(string formatString) : Attribute
{
    public string FormatString => formatString;
}