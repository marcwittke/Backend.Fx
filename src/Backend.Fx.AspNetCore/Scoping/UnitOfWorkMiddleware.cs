namespace Backend.Fx.AspNetCore.Scoping
{
    using System.Threading.Tasks;
    using Bootstrapping;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.AspNetCore.Http;
    using Patterns.UnitOfWork;

    /// <summary>
    ///     The Middleware is responsible for beginning and completing (or disposing) the unit of work for each request.
    /// </summary>
    public class UnitOfWorkMiddleware
    {
        private static readonly ILogger Logger = LogManager.Create<UnitOfWorkMiddleware>();
        private readonly RequestDelegate next;
        private readonly IBackendFxApplication application;

        [UsedImplicitly]
        public UnitOfWorkMiddleware(RequestDelegate next, IBackendFxApplication application)
        {
            this.next = next;
            this.application = application;
        }

        /// <summary>
        ///     This method is being called by the previous middleware in the HTTP pipeline
        /// </summary>
        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            while (!application.IsBooted.Wait(3000))
            {
                Logger.Info("Queuing Request while application is booting...");
            }

            var asReadonly = context.Request.Method.ToUpperInvariant() == "GET";
            using (var unitOfWork = asReadonly
                                            ? application.CompositionRoot.GetInstance<IReadonlyUnitOfWork>()
                                            : application.CompositionRoot.GetInstance<IUnitOfWork>())
            {
                unitOfWork.Begin();
                await next.Invoke(context);
                unitOfWork.Complete();
            }
        }
    }
}
