using System;
using System.ComponentModel;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class EnumDescriptionAttribute : DescriptionAttribute
{
    public EnumDescriptionAttribute(string description)
    {
        DescriptionValue = description ?? throw new ArgumentNullException(nameof(description));
    }

    public EnumDescriptionAttribute(string description, string display) : this(description)
    {
        Display = display ?? throw new ArgumentNullException(nameof(display));
    }

    public EnumDescriptionAttribute(string description, string display, string language) : this(description, display)
    {
        Language = language ?? throw new ArgumentNullException(nameof(language));
    }

    public string Display { get; set; }

    public string Language { get; set; }
}