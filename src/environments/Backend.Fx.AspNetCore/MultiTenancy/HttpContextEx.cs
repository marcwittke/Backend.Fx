using System;
using Backend.Fx.Environment.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.MultiTenancy
{
    public static class HttpContextEx
    {
        private const string TenantId = nameof(TenantId);

        public static void SetCurrentTenantId(this HttpContext httpContext, TenantId tenantId)
        {
            if (httpContext.Items.TryGetValue(TenantId, out object untyped))
            {
                throw new InvalidOperationException($"TenantId has been set already in this HttpContext. Value: {(untyped ?? "null")}");
            }

            httpContext.Items[TenantId] = tenantId;
        }
        
        public static TenantId GetTenantId(this HttpContext httpContext)
        {
            if (httpContext.Items.TryGetValue(TenantId, out object untyped))
            {
                return (TenantId) untyped;
            }

            throw new InvalidOperationException("No TenantId present in this HttpContext");
        }
    }
}