
namespace Backend.Fx.AspNetCore.Middlewares
{
    using System.Net;
    using System.Threading.Tasks;
    using Environment.Persistence;
    using Extensions;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    public class NukeMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IDatabaseManager databaseManager;
        private readonly IHostingEnvironment env;

        /// <summary>
        ///     This constructor is being called by the framework DI container
        /// </summary>
        [UsedImplicitly]
        public NukeMiddleware(RequestDelegate next, IDatabaseManager databaseManager, IHostingEnvironment env)
        {
            this.next = next;
            this.databaseManager = databaseManager;
            this.env = env;
        }

        /// <summary>
        ///     This method is being called by the previous middleware in the HTTP pipeline
        /// </summary>
        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            // in development environments there is a hidden nuke-endpoint that allows deleting the database
            if (env.IsDevelopment()
                && context.Request.Method.ToUpperInvariant() == "POST"
                && context.Request.IsLocal()
                && context.Request.Path == "/api/nuke")
            {
                databaseManager.DeleteDatabase();
                databaseManager.EnsureDatabaseExistence();
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                var responseContent = JsonConvert.SerializeObject(new { Status = "Ok", Message = "Database was deleted. The next request causes regeneration of an initial database." });
                await context.Response.WriteAsync(responseContent);
                return;
            }

            await next.Invoke(context);
        }
    }
}
