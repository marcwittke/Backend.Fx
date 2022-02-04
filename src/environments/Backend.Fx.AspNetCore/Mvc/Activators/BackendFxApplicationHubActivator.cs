using System;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.AspNetCore.Mvc.Activators
{
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
            var ip = _backendFxApplication.CompositionRoot.InstanceProvider;
            Logger.LogDebug("Providing {HubTypeName} using {InstanceProvider}", typeof(T).Name, ip.GetType().Name);
            return ip.GetInstance<T>();
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