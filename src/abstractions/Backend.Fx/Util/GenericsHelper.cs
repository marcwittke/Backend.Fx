using System;
using System.Linq;
using System.Reflection;

namespace Backend.Fx.Util
{
    public static class GenericsHelper
    {
        public static Type[] XGetGenericArguments(this Type t)
        {
            if (t == null) return null;
            return t.IsGenericType ? t.GetGenericArguments() : new Type[0];
        }

        public static Type[] XGetGenericArguments(this MethodInfo mi)
        {
            if (mi == null) return null;
            return mi.IsGenericMethod ? mi.GetGenericArguments() : new Type[0];
        }

        public static Type XGetGenericDefinition(this Type t)
        {
            if (t == null) return null;
            return t.IsGenericType ? t.GetGenericTypeDefinition() : t;
        }

        public static MethodInfo XGetGenericDefinition(this MethodInfo mi)
        {
            if (mi == null) return null;
            return mi.IsGenericMethod ? mi.GetGenericMethodDefinition() : mi;
        }

        public static Type XMakeGenericType(this Type t, params Type[] targs)
        {
            if (t == null) return null;
            if (!t.IsGenericType || t.IsGenericParameter)
            {
                if (targs.Length != 0)
                    throw new NotSupportedException(targs.Length.ToString());
                return t;
            }

            return t.GetGenericTypeDefinition().MakeGenericType(targs);
        }

        public static MethodInfo XMakeGenericMethod(this MethodInfo mi, params Type[] margs)
        {
            return mi.XMakeGenericMethod(mi.DeclaringType.XGetGenericArguments(), margs);
        }

        public static MethodInfo XMakeGenericMethod(this MethodInfo mi, Type[] targs, Type[] margs)
        {
            if (mi == null) return null;
            var pattern = (MethodInfo)mi.Module.ResolveMethod(mi.MetadataToken);

            var typeImpl = pattern.DeclaringType;
            if (targs != null && targs.Any()) typeImpl = typeImpl.MakeGenericType(targs);

            var methodImpl = typeImpl.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.Static)
                .Single(mi2 => mi2.MetadataToken == mi.MetadataToken);
            if (margs != null && margs.Any()) methodImpl = methodImpl.MakeGenericMethod(margs);

            return methodImpl;
        }

        public static bool IsOpenGeneric(this Type t)
        {
            return t.IsGenericParameter || t.XGetGenericArguments().Any(arg => arg.IsOpenGeneric());
        }

        public static bool IsOpenGeneric(this MethodInfo t)
        {
            return t.ReturnType.IsOpenGeneric() ||
                   t.GetParameters().Any(pi => pi.ParameterType.IsOpenGeneric());
        }
    }
}