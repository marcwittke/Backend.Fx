using System;
using System.Threading;
using System.Threading.Tasks;

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

        /// <summary>
        /// allows asynchronously awaiting application startup
        /// </summary>
        ManualResetEventSlim IsBooted { get; }

        /// <summary>
        /// Initializes ans starts the application (async)
        /// </summary>
        /// <returns></returns>
        Task Boot();
    }
}
