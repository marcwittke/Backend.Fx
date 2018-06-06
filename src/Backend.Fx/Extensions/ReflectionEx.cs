using System;

namespace Backend.Fx.Extensions
{
    using System.Linq;

    public static class ReflectionEx
    {
        public static bool IsImplementationOfOpenGenericInterface(this Type t, Type openGenericInterface)
        {
            return t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == openGenericInterface);
        }
    }
}
