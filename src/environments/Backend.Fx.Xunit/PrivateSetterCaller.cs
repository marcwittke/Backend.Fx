using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Backend.Fx.Xunit
{
    public static class PrivateSetterCaller
    {
        public static void SetPrivate<T, TValue>(this T instance, Expression<Func<T, TValue>> propertyExpression, TValue value)
        {
            instance.GetType().GetTypeInfo().GetDeclaredProperty(GetName(propertyExpression)).SetValue(instance, value, null);
        }

        private static string GetName<T, TValue>(Expression<Func<T, TValue>> exp)
        {
            if (!(exp.Body is MemberExpression body))
            {
                UnaryExpression ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            Debug.Assert(body != null, "body != null");
            return body.Member.Name;
        }
    }
}