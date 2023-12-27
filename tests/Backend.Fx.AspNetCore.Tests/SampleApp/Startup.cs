using System.Security.Principal;
using Backend.Fx.AspNetCore.MultiTenancy;
using Backend.Fx.AspNetCore.Mvc;
using Backend.Fx.AspNetCore.Mvc.Activators;
using Backend.Fx.AspNetCore.Tests.SampleApp.Domain;
using Backend.Fx.AspNetCore.Tests.SampleApp.Runtime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNetCore.Tests.SampleApp
{
    public class Startup
    {
        private readonly ExceptionLoggers _exceptionLoggers = new ();

        public void ConfigureServices(IServiceCollection services)
        {
            // diagnostics services
            services.AddSingleton<IExceptionLogger>(_exceptionLoggers);
            
            // decode jwt 
            services.AddAuthentication(authOpt =>
            {
                authOpt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOpt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                authOpt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOpt => bearerOpt.TokenValidationParameters = JwtService.TokenValidationParameters());

            
            // enabling MVC
            services.AddMvc();
            services.AddSingleton<IControllerActivator, BackendFxApplicationControllerActivator>();
            
            // integrate backend fx application as hosted service
            services.AddBackendFxApplication<SampleApplicationHostedService, SampleApplication>();
            
            services.AddSingleton<IMessageBus, InMemoryMessageBus>();
            services.AddSingleton<ITenantRepository, InMemoryTenantRepository>();
            services.AddSingleton<ITenantService, TenantService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // log exceptions to file
            _exceptionLoggers.Add(new ExceptionLogger(Log.Create("Sample.WebHost")));
            
            // use the ASP.Net Core routing middleware that decides the endpoint to be hit later
            app.UseRouting();
            
            // error handling: return rich JSON errors,
            app.UseMiddleware<SampleJsonErrorHandlingMiddleware>();
            
            // decode JWT Bearer from Authorization header
            app.UseAuthentication();

            app.UseMiddleware<TenantAdminMiddleware>();
            
            app.UseMiddleware<MultiTenantMiddleware>();
            
            app.Use(async (context, requestDelegate) =>
            {
                IBackendFxApplication application = app.ApplicationServices.GetRequiredService<SampleApplication>();
                application.WaitForBoot();
            
                // set the instance provider for the controller activator
                context.SetCurrentInstanceProvider(application.CompositionRoot.InstanceProvider);
            
                // the ambient tenant id has been set before by a TenantMiddleware
                var tenantId = context.GetTenantId();
            
                // the invoking identity has been set before by an AuthenticationMiddleware
                IIdentity actingIdentity = context.User.Identity;
            
                await application.AsyncInvoker.InvokeAsync(_ => requestDelegate.Invoke(), actingIdentity, tenantId);
            });
            
            app.UseEndpoints(endpointRouteBuilder =>
            {
                endpointRouteBuilder.MapControllers();
            });
        }
    }
}