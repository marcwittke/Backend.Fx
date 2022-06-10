using System;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.Mvc
{
    public static class HttpContextEx
    {
        private const string ServiceProvider = nameof(ServiceProvider);

        public static void SetCurrentServiceProvider(this HttpContext httpContext, IServiceProvider serviceProvider)
        {
            if (httpContext.Items.TryGetValue(ServiceProvider, out _))
            {
                throw new InvalidOperationException("IServiceProvider has been set already in this HttpContext");
            }

            httpContext.Items[ServiceProvider] = serviceProvider;
        }

        public static IServiceProvider GetServiceProvider(this HttpContext httpContext)
        {
            if (httpContext.Items.TryGetValue(ServiceProvider, out object untyped))
            {
                return (IServiceProvider) untyped;
            }

            throw new InvalidOperationException("No IServiceProvider present in this HttpContext");
        }

        public static bool TryGetServiceProvider(this HttpContext httpContext, out IServiceProvider serviceProvider)
        {
            if (httpContext.Items.TryGetValue(ServiceProvider, out object untyped))
            {
                serviceProvider = (IServiceProvider) untyped;
                return true;
            }

            serviceProvider = null;
            return false;
        }
    }
}