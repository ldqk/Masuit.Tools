using System.Threading;

namespace Masuit.Tools.Systems;

public class CurrentContext<T>
{
    private static readonly AsyncLocal<T> Current = new();

    public static void SetData(T value)
    {
        Current.Value = value;
    }

    public static T GetData()
    {
        return Current.Value;
    }
}

public class CurrentContext
{
    private static readonly AsyncLocal<object> Current = new();

    public static void SetData<T>(T value)
    {
        Current.Value = value;
    }

    public static T GetData<T>()
    {
        return Current.Value is T value ? value : default;
    }
}
