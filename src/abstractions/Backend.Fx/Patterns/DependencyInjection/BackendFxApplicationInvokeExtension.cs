using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using JetBrains.Annotations;

namespace Backend.Fx.Patterns.DependencyInjection;

[PublicAPI]
public static class BackendFxApplicationInvokeExtension
{
    /// <summary>
    ///     Invokes an async action
    /// </summary>
    public static Task DoAsync(
        this IBackendFxApplication application,
        Func<IInstanceProvider, Task> asyncAction,
        IIdentity identity = null,
        TenantId tenantId = null)
    {
        return application.AsyncInvoker.InvokeAsync(
            asyncAction,
            identity ?? new AnonymousIdentity(),
            tenantId ?? new TenantId(0));
    }

    /// <summary>
    ///     Invokes a synchronous action
    /// </summary>
    public static void Do(
        this IBackendFxApplication application,
        Action<IInstanceProvider> action,
        IIdentity identity = null,
        TenantId tenantId = null)
    {
        application.Invoker.Invoke(
            action,
            identity ?? new AnonymousIdentity(),
            tenantId ?? new TenantId(0));
    }

    /// <summary>
    ///     Invokes an async function that returns a result
    /// </summary>
    public static async Task<TResult> DoAsync<TResult>(
        this IBackendFxApplication application,
        Func<IInstanceProvider, Task<TResult>> asyncFunction,
        IIdentity identity = null,
        TenantId tenantId = null)
    {
        TResult result = default!;
        await application.AsyncInvoker.InvokeAsync(
            async sp => result = await asyncFunction(sp),
            identity ?? new AnonymousIdentity(),
            tenantId ?? new TenantId(0));
        return result;
    }

    /// <summary>
    ///     Invokes a synchronous function that returns a result
    /// </summary>
    public static TResult Do<TResult>(
        this IBackendFxApplication application,
        Func<IInstanceProvider, TResult> function,
        IIdentity identity = null,
        TenantId tenantId = null)
    {
        TResult result = default!;
        application.Invoker.Invoke(
            sp =>
            {
                result = function(sp);
            },
            identity ?? new AnonymousIdentity(),
            tenantId ?? new TenantId(0));
        return result;
    }

    public static WithInvocation<TService> With<TService>(this IBackendFxApplication application)
        where TService : class
    {
        return new WithInvocation<TService>(application);
    }
}