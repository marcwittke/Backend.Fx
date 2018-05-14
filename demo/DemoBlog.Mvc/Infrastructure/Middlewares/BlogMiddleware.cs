namespace DemoBlog.Mvc.Infrastructure.Middlewares
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Security;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Backend.Fx.Bootstrapping;
    using Backend.Fx.Environment.MultiTenancy;
    using Backend.Fx.Exceptions;
    using Backend.Fx.Logging;
    using Bootstrapping;
    using Data.Identity;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    /// <summary>
    ///     This middleware enables use of an application runtime for each request. It makes sure that every request
    ///     is handled inside a unique execution scope resulting in a specific resolution root throughout the request.
    ///     The Middleware handles exceptions and is responsible for beginning and completing (or disposing) the unit
    ///     of work for each request.
    /// </summary>
    public class BlogMiddleware
    {
        private static readonly ILogger Logger = LogManager.Create<BlogMiddleware>();
        private readonly RequestDelegate next;
        private readonly IHostingEnvironment env;
        private readonly BackendFxDbApplication blogApplication;
        private readonly Lazy<TenantId> defaultTenantId;

        /// <summary>
        ///     This constructor is being called by the framework DI container
        /// </summary>
        [UsedImplicitly]
        public BlogMiddleware(RequestDelegate next, BlogApplication blogApplication, IHostingEnvironment env)
        {
            this.next = next;
            this.blogApplication = blogApplication;
            this.env = env;
            defaultTenantId = new Lazy<TenantId>(()=> blogApplication.TenantManager.GetDefaultTenantId());
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

                using (var scope = blogApplication.ScopeManager.BeginScope(context.User.Identity, tenantId))
                {
                    try
                    {
                        var asReadonly = context.Request.Method.ToUpperInvariant() == "GET";
                        using (var unitOfWork = scope.BeginUnitOfWork(asReadonly))
                        {
                            await next.Invoke(context);
                            unitOfWork.Complete();
                        }
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
                        var responseContent = HostingEnvironmentExtensions.IsDevelopment(env)
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
                var responseContent = HostingEnvironmentExtensions.IsDevelopment(env)
                                          ? JsonConvert.SerializeObject(new { ex.Message, ex.StackTrace })
                                          : JsonConvert.SerializeObject(new { Message = "An internal error occured" });
                await context.Response.WriteAsync(responseContent);
            }
        }

        private TenantId GetTenantId(IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            Claim claim = claimsIdentity?.Claims.SingleOrDefault(cl => cl.Type == BlogUser.TenantIdClaimType);
            if (claim != null && int.TryParse(claim.Value, out var parsed))
            {
                return new TenantId(parsed);
            }

            return defaultTenantId.Value;
        }
    }
}