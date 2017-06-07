namespace Backend.Fx.Testing
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class PrivateSetterCaller
    {
        public static void SetPrivate<T, TValue>(this T instance, Expression<Func<T, TValue>> propertyExpression, TValue value)
        {
            instance.GetType().GetTypeInfo().GetDeclaredProperty(GetName(propertyExpression)).SetValue(instance, value, null);
        }

        private static string GetName<T, TValue>(Expression<Func<T, TValue>> exp)
        {
            MemberExpression body = exp.Body as MemberExpression;

            if (body == null)
            {
                UnaryExpression ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            Debug.Assert(body != null, "body != null");
            return body.Member.Name;
        }
    }
}