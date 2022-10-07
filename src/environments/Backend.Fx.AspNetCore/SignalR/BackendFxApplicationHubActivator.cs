using System;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.AspNetCore.SignalR;

[PublicAPI]
public class BackendFxApplicationHubActivator<T> : IHubActivator<T> where T : Hub
{
    private readonly ICompositionRoot _compositionRoot;
    private static readonly ILogger Logger = Log.Create<BackendFxApplicationHubActivator<T>>();


    public BackendFxApplicationHubActivator(ICompositionRoot compositionRoot)
    {
        _compositionRoot = compositionRoot;
    }

    public T Create()
    {
        return _compositionRoot.ServiceProvider.GetRequiredService<T>();
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