using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Backend.Fx.Hacking
{
    [PublicAPI]
    public static class PrivateSetterCaller
    {
        public static void SetPrivate<T, TValue>(this T instance, Expression<Func<T, TValue>> propertyExpression, TValue value)
        {
            instance.GetType().GetTypeInfo().GetDeclaredProperty(GetName(propertyExpression)).SetValue(instance, value, null);
        }

        private static string GetName<T, TValue>(Expression<Func<T, TValue>> exp)
        {
            if (exp.Body is not MemberExpression body)
            {
                var unaryExpression = (UnaryExpression) exp.Body;
                body = unaryExpression.Operand as MemberExpression;
            }

            Debug.Assert(body != null, "body != null");
            return body.Member.Name;
        }
    }
}