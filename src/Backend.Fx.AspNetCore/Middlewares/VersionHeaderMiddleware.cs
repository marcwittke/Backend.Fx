
namespace Backend.Fx.AspNetCore.Middlewares
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;

    public class VersionHeaderMiddleware
    {
        private readonly RequestDelegate next;
        private readonly Version currentVersion = typeof(VersionHeaderMiddleware).GetTypeInfo().Assembly.GetName().Version;

        /// <summary>
        ///     This constructor is being called by the framework DI container
        /// </summary>
        [UsedImplicitly]
        public VersionHeaderMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        ///     This method is being called by the previous middleware in the HTTP pipeline
        /// </summary>
        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("blog-version", new StringValues(currentVersion.ToString()));
            await next.Invoke(context);
        }
    }
}
