namespace Backend.Fx.AspNetCore.Middlewares
{
    using System;
    using System.Net;
    using System.Security;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Environment.MultiTenancy;
    using Exceptions;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Patterns.DependencyInjection;

    /// <summary>
    ///     This middleware enables use of an application runtime for each request. It makes sure that every request
    ///     is handled inside a unique execution scope resulting in a specific resolution root throughout the request.
    ///     The Middleware handles exceptions and is responsible for beginning and completing (or disposing) the unit
    ///     of work for each request.
    /// </summary>
    public abstract class ScopeMiddleware
    {
        private static readonly ILogger Logger = LogManager.Create<ScopeMiddleware>();
        private readonly IHostingEnvironment env;
        private readonly RequestDelegate next;
        private readonly IScopeManager scopeManager;
        
        /// <summary>
        ///     This constructor is being called by the framework DI container
        /// </summary>
        [UsedImplicitly]
        protected ScopeMiddleware(RequestDelegate next, IScopeManager scopeManager, IHostingEnvironment env)
        {
            this.next = next;
            this.scopeManager = scopeManager;
            this.env = env;
        }

        /// <summary>
        ///     This method is being called by the previous middleware in the HTTP pipeline
        /// </summary>
        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            try
            {
                TenantId tenantId = GetTenantId(context.User.Identity);
                
                var asReadonly = context.Request.Method.ToUpperInvariant() == "GET";
                using (var scope = scopeManager.BeginScope(context.User.Identity, tenantId))
                {
                    scope.BeginUnitOfWork(asReadonly);
                    try
                    {
                        await next.Invoke(context);
                        scope.CompleteUnitOfWork();
                    }
                    catch (UnprocessableException uex)
                    {
                        Logger.Warn(uex);
                        context.Response.StatusCode = 422;
                        var responseContent = JsonConvert.SerializeObject(new { uex.Message });
                        await context.Response.WriteAsync(responseContent);
                    }
                    catch (NotFoundException nfex)
                    {
                        Logger.Warn(nfex);
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        var responseContent = JsonConvert.SerializeObject(new { nfex.Message });
                        await context.Response.WriteAsync(responseContent);
                    }
                    catch (ClientException cex)
                    {
                        Logger.Warn(cex);
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var responseContent = JsonConvert.SerializeObject(new { cex.Message });
                        await context.Response.WriteAsync(responseContent);
                    }
                    catch (DbUpdateConcurrencyException concEx)
                    {
                        Logger.Warn(concEx);
                        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                        var responseContent = JsonConvert.SerializeObject(new { concEx.Message });
                        await context.Response.WriteAsync(responseContent);
                    }
                    catch (SecurityException secex)
                    {
                        Logger.Warn(secex);
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        await context.Response.WriteAsync("");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var responseContent = env.IsDevelopment()
                            ? JsonConvert.SerializeObject(new { ex.Message, ex.StackTrace })
                            : JsonConvert.SerializeObject(new { Message = "An internal error occured" });
                        await context.Response.WriteAsync(responseContent);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                var responseContent = env.IsDevelopment()
                    ? JsonConvert.SerializeObject(new { ex.Message, ex.StackTrace })
                    : JsonConvert.SerializeObject(new { Message = "An internal error occured" });
                await context.Response.WriteAsync(responseContent);
            }
        }

        protected abstract TenantId GetTenantId(IIdentity identity);
    }
}
