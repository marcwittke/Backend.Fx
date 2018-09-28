using System;

namespace Backend.Fx.Extensions
{
    using System.Linq;
    using System.Reflection;

    public static class ReflectionEx
    {
        public static bool IsImplementationOfOpenGenericInterface(this Type t, Type openGenericInterface)
        {
            return t.GetInterfaces().Any(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == openGenericInterface);
        }
    }
}
