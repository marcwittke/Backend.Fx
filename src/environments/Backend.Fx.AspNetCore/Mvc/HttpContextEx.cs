using System;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.Mvc
{
    public static class HttpContextEx
    {
        private const string InstanceProvider = nameof(InstanceProvider);

        public static void SetCurrentInstanceProvider(this HttpContext httpContext, IInstanceProvider tenantId)
        {
            if (httpContext.Items.TryGetValue(InstanceProvider, out object _))
            {
                throw new InvalidOperationException("IInstanceProvider has been set already in this HttpContext");
            }

            httpContext.Items[InstanceProvider] = tenantId;
        }

        public static IInstanceProvider GetInstanceProvider(this HttpContext httpContext)
        {
            if (httpContext.Items.TryGetValue(InstanceProvider, out object untyped))
            {
                return (IInstanceProvider)untyped;
            }

            throw new InvalidOperationException("No IInstanceProvider present in this HttpContext");
        }

        public static bool TryGetInstanceProvider(this HttpContext httpContext, out IInstanceProvider instanceProvider)
        {
            if (httpContext.Items.TryGetValue(InstanceProvider, out object untyped))
            {
                instanceProvider = (IInstanceProvider)untyped;
                return true;
            }

            instanceProvider = null;
            return false;
        }
    }
}
