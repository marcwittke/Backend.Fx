using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Backend.Fx.AspNetCore.MultiTenancy
{
    public abstract class TenantAdminMiddlewareBase
    {
        private static readonly ILogger Logger = LogManager.Create<TenantAdminMiddlewareBase>();
        private readonly RequestDelegate _next;
        protected virtual string TenantsApiBaseUrl { get; } = "/api/tenants";
        protected ITenantService TenantService { get; }

        protected TenantAdminMiddlewareBase(RequestDelegate next, ITenantService tenantService)
        {
            _next = next;
            TenantService = tenantService;
        }
        
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(TenantsApiBaseUrl))
            {
                if (!IsTenantsAdmin(context))
                {
                    Logger.Warn("Unauthorized attempt to access tenant endpoints");
                    context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                    return;
                }

                if (context.Request.Method.ToLower() == "post")
                {
                    Logger.Info("Creating Tenant");
                    
                    try
                    {
                        using (var inputStream = new StreamReader(context.Request.Body))
                        {
                            string inputStreamContent = await inputStream.ReadToEndAsync();

                            var createTenantParams = JsonConvert.DeserializeObject<CreateTenantParams>(inputStreamContent);
                            if (createTenantParams == null) throw new Exception("Bad Request");

                            var tenant = await CreateTenant(createTenantParams);
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(tenant), Encoding.UTF8);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Tenant Creation failed");
                        context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                        await context.Response.WriteAsync(ex.Message);
                        return;
                    }

                    return;
                }

                if (context.Request.Method.ToLower() == "get")
                {
                    var tenantIdStr = context.Request.Path.Value.Split('/').Last();
                    if (int.TryParse(tenantIdStr, out int tenantId))
                    {
                        Logger.Info($"Getting Tenant[{tenantId}]");

                        var tenant = TenantService.GetTenant(new TenantId(tenantId));
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(tenant), Encoding.UTF8);
                    }
                    else
                    {
                        Logger.Info($"Getting Tenants");
                        
                        var tenants = TenantService.GetTenants();
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(tenants), Encoding.UTF8);
                    }

                    return;
                }
            }

            await _next.Invoke(context);
        }

        private async Task<Tenant> CreateTenant(CreateTenantParams createTenantParams)
        {
            var tenantId = TenantService.CreateTenant(
                createTenantParams.Name,
                createTenantParams.Description,
                createTenantParams.IsDemo, 
                GetTenantConfiguration(createTenantParams));
            await AfterTenantCreation(createTenantParams, tenantId);
            var tenant = TenantService.GetTenant(tenantId);
            return tenant;
        }
        
        protected abstract string GetTenantConfiguration(CreateTenantParams createTenantParams);

        protected virtual Task AfterTenantCreation(CreateTenantParams createTenantParams, TenantId tenantId)
        {
            return Task.CompletedTask;
        }

        protected abstract bool IsTenantsAdmin(HttpContext context);
    }
}