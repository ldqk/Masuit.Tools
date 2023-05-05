using Masuit.Tools.Dynamics.Behaviors;

namespace Masuit.Tools.Dynamics;

public class DynamicFactory : Clay
{
    public DynamicFactory() : base(new ClayFactoryBehavior(), new ArrayFactoryBehavior())
    {
    }

    public static dynamic NewObject()
    {
        dynamic factory = new DynamicFactory();
        return factory.DynamicObject();
    }

    public static dynamic WithObject(object obj)
    {
        dynamic factory = new DynamicFactory();
        return factory.DynamicObject(obj);
    }
}
