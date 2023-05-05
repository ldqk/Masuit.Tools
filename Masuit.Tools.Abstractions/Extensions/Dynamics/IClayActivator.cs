using System;
using System.Collections.Generic;

namespace Masuit.Tools.Dynamics;

internal interface IClayActivator
{
    dynamic CreateInstance(Type baseType, IEnumerable<IClayBehavior> behaviors, IEnumerable<object> arguments);
}
