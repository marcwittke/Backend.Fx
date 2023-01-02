using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Backend.Fx.Util
{
    [PublicAPI]
    public static class ReflectionEx
    {
        public static IEnumerable<Type> GetImplementingTypes<TService>(this IEnumerable<Assembly> assemblies)
        {
            return assemblies.GetImplementingTypes(typeof(TService));
        }
        
        public static IEnumerable<Type> GetImplementingTypes(this IEnumerable<Assembly> assemblies, Type serviceType)
        {
            return assemblies
                .Distinct()
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(serviceType.IsAssignableFrom);
        }
        
        public static IEnumerable<Type> GetImplementingTypes(this Assembly assembly, Type serviceType)
        {
            return assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(serviceType.IsAssignableFrom);
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
        
        public static bool IsOpenGeneric(this Type t)
        {
            if (t == null) return false;
            if (t.IsGenericParameter) return true;
            if (t.IsGenericType && t.GetGenericArguments().Any(arg => arg.IsOpenGeneric())) return true;

            return false;
        }
    }
}