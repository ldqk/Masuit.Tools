using Masuit.Tools.AspNetCore.ModelBinder;
using Masuit.Tools.Files;
using Masuit.Tools.Media;
using Masuit.Tools.Reflection;

var attributes = MyEnum.A.GetTypedEnumDescriptions();
Console.ReadKey();
MyStruct<object>? a = null;

public enum MyEnum
{
    [EnumDescription("A", "a")]
    A
}

internal struct MyStruct<T>
{
    public T Value { get; set; }
}
