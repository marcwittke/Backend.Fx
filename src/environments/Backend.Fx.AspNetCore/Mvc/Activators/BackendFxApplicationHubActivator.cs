using System;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.AspNetCore.Mvc.Activators
{
    [PublicAPI]
    public class BackendFxApplicationHubActivator<T> : IHubActivator<T> where T : Hub
    {
        private readonly IBackendFxApplication _backendFxApplication;
        private static readonly ILogger Logger = Log.Create<BackendFxApplicationHubActivator<T>>();


        public BackendFxApplicationHubActivator(IBackendFxApplication backendFxApplication)
        {
            _backendFxApplication = backendFxApplication;
        }

        public T Create()
        {
            var sp = _backendFxApplication.CompositionRoot.ServiceProvider;
            Logger.LogDebug("Providing {HubTypeName} using {ServiceProvider}", typeof(T).Name, sp.GetType().Name);
            return sp.GetRequiredService<T>();
        }

        public void Release(T hub)
        {
            Logger.LogTrace("Releasing {HubTypeName}", hub.GetType().Name);
            if (hub is IDisposable disposable)
            {
                Logger.LogDebug("Disposing {HubTypeName}", hub.GetType().Name);
                disposable.Dispose();
            }
        }
    }
}