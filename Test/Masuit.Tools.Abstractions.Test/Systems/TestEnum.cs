using System.ComponentModel;

namespace Masuit.Tools.Abstractions.Test.Systems;

public enum TestEnum
{
    [EnumDescription("First Value", Language = "en")]
    First = 1,

    [EnumDescription("Second Value")]
    Second = 2,

    [EnumDescription("Third Value")]
    Third = 3
}

public enum FlagsEnum
{
    [Description("None")]
    None = 0,

    [Description("Flag A")]
    FlagA = 1,

    [Description("Flag B")]
    FlagB = 2,

    [Description("Flag C")]
    FlagC = 4
}