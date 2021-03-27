using System;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Fx.AspNetCore.Mvc.Activators
{
    public class BackendFxApplicationHubActivator<T> : IHubActivator<T> where T : Hub
    {
        private readonly IBackendFxApplication _backendFxApplication;
        private static readonly ILogger Logger = LogManager.Create<BackendFxApplicationHubActivator<T>>();
        

        public BackendFxApplicationHubActivator(IBackendFxApplication backendFxApplication)
        {
            _backendFxApplication = backendFxApplication;
        }


        public T Create()
        {
            var ip = _backendFxApplication.CompositionRoot.InstanceProvider;
            Logger.Debug($"Providing {typeof(T).Name} using {ip.GetType().Name}");
            return ip.GetInstance<T>();
        }

        public void Release(T hub)
        {
            Logger.Trace($"Releasing {hub.GetType().Name}");
            if (hub is IDisposable disposable)
            {
                Logger.Debug($"Disposing {hub.GetType().Name}");
                disposable.Dispose();
            }
        }
    }
}