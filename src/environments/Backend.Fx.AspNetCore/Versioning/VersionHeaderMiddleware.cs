namespace Backend.Fx.AspNetCore.Versioning
{
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;

    public class VersionHeaderMiddleware : IMiddleware
    {
        private readonly AssemblyName _entryAssemblyName = Assembly.GetEntryAssembly().GetName();

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Response.Headers.Add(_entryAssemblyName.Name, new StringValues(_entryAssemblyName.Version.ToString(3)));
            await next.Invoke(context);
        }
    }
}