using System;
using System.Collections.Generic;

namespace Masuit.Tools.Dynamics;

internal static class ClayActivator
{
    static ClayActivator()
    {
        var instance = new DefaultClayActivator();
        ServiceLocator = () => instance;
    }

    public static Func<IClayActivator> ServiceLocator { get; set; }

    public static dynamic CreateInstance(IEnumerable<IClayBehavior> behaviors, params object[] arguments)
    {
        return ServiceLocator().CreateInstance(typeof(Clay), behaviors, arguments);
    }

    public static dynamic CreateInstance(Type baseType, IEnumerable<IClayBehavior> behaviors, params object[] arguments)
    {
        return ServiceLocator().CreateInstance(baseType, behaviors, arguments);
    }

    public static dynamic CreateInstance<TBase>(IEnumerable<IClayBehavior> behaviors, params object[] arguments)
    {
        return ServiceLocator().CreateInstance(typeof(TBase), behaviors, arguments);
    }
}
