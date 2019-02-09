using System;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.Jobs;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public interface IBackendFxApplication : IDisposable
    {
        /// <summary>
        /// You should use the <see cref="IScopeManager"/> to open an injection scope for every logical operation.
        /// In case of web applications, this refers to a single HTTP request, for example.
        /// </summary>
        IScopeManager ScopeManager { get; }

        /// <summary>
        /// The composition root of the dependency injection framework
        /// </summary>
        ICompositionRoot CompositionRoot { get; }

        IJobEngine JobEngine { get; }

        /// <summary>
        /// allows asynchronously awaiting application startup
        /// </summary>
        Task<bool> WaitForBootAsync(int timeoutMilliSeconds = int.MaxValue);

        /// <summary>
        /// allows synchronously awaiting application startup
        /// </summary>
        bool WaitForBoot(int timeoutMilliSeconds = int.MaxValue);

        ITenantManager TenantManager { get; }

        /// <summary>
        /// Initializes ans starts the application (async)
        /// </summary>
        /// <returns></returns>
        Task Boot();
    }
}
