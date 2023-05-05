using Masuit.Tools.Dynamics.Behaviors;

namespace Masuit.Tools.Dynamics;

internal static class NullInstance
{
    public static readonly object Instance = new Clay(new NullBehavior(), new InterfaceProxyBehavior());
}
