using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Extensions.MessageBus;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.ExecutionPipeline
{
    public interface IBackendFxApplicationInvoker
    {
        /// <param name="awaitableAsyncAction">The async action to be invoked by the application</param>
        /// <param name="identity">The acting identity</param>
        Task InvokeAsync(Func<IServiceProvider, Task> awaitableAsyncAction, IIdentity identity);
    }


    internal class BackendFxApplicationInvoker : IBackendFxApplicationInvoker
    {
        private readonly ICompositionRoot _compositionRoot;
        private static readonly ILogger Logger = Log.Create<BackendFxApplicationInvoker>();

        public BackendFxApplicationInvoker(ICompositionRoot compositionRoot)
        {
            _compositionRoot = compositionRoot;
        }


        public async Task InvokeAsync(Func<IServiceProvider, Task> awaitableAsyncAction, IIdentity identity)
        {
            Logger.LogInformation("Invoking asynchronous action as {@Identity}", identity);
            using (IServiceScope serviceScope = BeginScope(identity))
            {
                using (UseDurationLogger(serviceScope))
                {
                    var operation = serviceScope.ServiceProvider.GetRequiredService<IOperation>();
                    try
                    {
                        operation.Begin(serviceScope);
                        await awaitableAsyncAction.Invoke(serviceScope.ServiceProvider).ConfigureAwait(false);
                        operation.Complete();
                    }
                    catch
                    {
                        operation.Cancel();
                        throw;
                    }
                }
            }
        }


        private IServiceScope BeginScope(IIdentity identity)
        {
            IServiceScope serviceScope = _compositionRoot.BeginScope();

            identity = identity ?? new AnonymousIdentity();
            serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<IIdentity>>().ReplaceCurrent(identity);

            

            return serviceScope;
        }


        private static IDisposable UseDurationLogger(IServiceScope serviceScope)
        {
            IIdentity identity = serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<IIdentity>>().Current;
            Correlation correlation = serviceScope.ServiceProvider.GetRequiredService<ICurrentTHolder<Correlation>>().Current;
            return Logger.LogInformationDuration(
                $"Starting scope (correlation [{correlation.Id}]) for {identity.Name}",
                $"Ended scope (correlation [{correlation.Id}]) for {identity.Name}");
        }
    }
}