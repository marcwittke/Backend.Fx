using Backend.Fx.AspNetCore.Tests.SampleApp.Domain;
using Backend.Fx.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNetCore.Tests.SampleApp
{
    public class Startup
    {
        private readonly ExceptionLoggers _exceptionLoggers = new();

        public void ConfigureServices(IServiceCollection services)
        {
            // diagnostics services
            services.AddSingleton<IExceptionLogger>(_exceptionLoggers);

            // decode jwt 
            services.AddAuthentication(
                    authOpt =>
                    {
                        authOpt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        authOpt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        authOpt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(
                    bearerOpt => bearerOpt.TokenValidationParameters = JwtService.TokenValidationParameters());


            // enabling MVC
            services.AddMvc();

            // integrate backend fx application as hosted service
            services.AddBackendFxApplication<SampleApplicationHostedService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // log exceptions to file
            _exceptionLoggers.Add(new ExceptionLogger(LogManager.Create("Mep.WebHost")));

            // use the ASP.Net Core routing middleware that decides the endpoint to be hit later
            app.UseRouting();

            // error handling: return rich JSON errors,
            app.UseMiddleware<SampleJsonErrorHandlingMiddleware>();

            // decode JWT Bearer from Authorization header
            app.UseAuthentication();

            app.UseMiddleware<TenantAdminMiddleware>();

            app.UseBackendFxApplication<SampleApplicationHostedService, MultiTenantMiddleware>();

            app.UseEndpoints(endpointRouteBuilder => { endpointRouteBuilder.MapControllers(); });
        }
    }
}
