using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Backend.Fx.Extensions
{
    public static class ReflectionEx
    {
        public static IEnumerable<Type> GetImplementingTypes(this IEnumerable<Assembly> assemblies, Type serviceType)
        {
            return assemblies
                .Distinct()
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(assembly => assembly.GetTypes(), (assembly, type) => new {assembly, type})
                .Where(t => t.type.IsClass && !t.type.IsAbstract)
                .Where(t => serviceType.IsAssignableFrom(t.type))
                .Select(t => t.type);
        }
        
        public static bool IsImplementationOfOpenGenericInterface(this Type t, Type openGenericInterface)
        {
            return t.GetInterfaces().Any(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == openGenericInterface);
        }

        public static string GetDetailedTypeName(this Type t)
        {
            string detailedTypeName = t.Name;
            if (t.GetTypeInfo().IsGenericType)
            {
                var genericNameWithoutArgCount = t.Name.Substring(0, t.Name.IndexOf('`'));
                var typeArgNames = t.GenericTypeArguments.Select(a => a.Name);
                detailedTypeName = $"{genericNameWithoutArgCount}<{string.Join(",", typeArgNames)}>";
            }

            return detailedTypeName;
        }
    }
}