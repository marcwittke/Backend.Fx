using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx
{
    /// <summary>
    /// The root object of the whole backend fx application framework
    /// </summary>
    [PublicAPI]
    public interface IBackendFxApplication : IDisposable
    {
        /// <summary>
        /// The invoker runs a given action asynchronously in an application scope with injection facilities 
        /// </summary>
        IBackendFxApplicationInvoker Invoker { get; }

        /// <summary>
        /// The composition root of the dependency injection framework
        /// </summary>
        ICompositionRoot CompositionRoot { get; }

        /// <summary>
        /// The global exception logger of this application
        /// </summary>
        IExceptionLogger ExceptionLogger { get; }

        Assembly[] Assemblies { get; }

        /// <summary>
        /// allows synchronously awaiting application startup
        /// </summary>
        Task WaitForBootAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes and starts the application (async)
        /// </summary>
        /// <returns></returns>
        Task BootAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Enables an optional feature. Must be done before calling <see cref="BootAsync"/>.
        /// </summary>
        /// <param name="feature"></param>
        void EnableFeature(Feature feature);

        void RequireDependantFeature<TFeature>() where TFeature : Feature;
    }


    [PublicAPI]
    public class BackendFxApplication : IBackendFxApplication
    {
        private static readonly ILogger Logger = Log.Create<BackendFxApplication>();
        private readonly List<Feature> _features = new();
        private readonly Lazy<Task> _bootAction;

        /// <summary>
        /// Initializes the application's runtime instance
        /// </summary>
        /// <param name="compositionRoot">The composition root of the dependency injection framework</param>
        /// <param name="assemblies"></param>
        public BackendFxApplication(ICompositionRoot compositionRoot, params Assembly[] assemblies)
            : this(compositionRoot, new ExceptionLogger(Logger), assemblies)
        {
        }

        /// <summary>
        /// Initializes the application's runtime instance
        /// </summary>
        /// <param name="compositionRoot">The composition root of the dependency injection framework</param>
        /// <param name="exceptionLogger"></param>
        /// <param name="assemblies"></param>
        public BackendFxApplication(ICompositionRoot compositionRoot, IExceptionLogger exceptionLogger,
            params Assembly[] assemblies)
        {
            assemblies ??= Array.Empty<Assembly>();

            Logger.LogInformation(
                "Initializing application with {CompositionRoot} providing services from [{Assemblies}]",
                compositionRoot.GetType().GetDetailedTypeName(),
                string.Join(", ", assemblies.Select(ass => ass.GetName().Name)));

            var invoker = new BackendFxApplicationInvoker(this);

            Invoker = new ExceptionLoggingInvoker(exceptionLogger, invoker);

            CompositionRoot = new LogRegistrationsDecorator(compositionRoot);
            ExceptionLogger = exceptionLogger;
            Assemblies = assemblies;
            CompositionRoot.RegisterModules(new ExecutionPipelineModule(withFrozenClockDuringExecution: true));

            _bootAction = new Lazy<Task>(async () =>
            {
                Logger.LogInformation("Booting application");
                CompositionRoot.Verify();

                foreach (Feature feature in _features)
                {
                    if (feature is IBootableFeature bootableFeature)
                    {
                        await bootableFeature.BootAsync(this).ConfigureAwait(false);
                    }
                }
            });
        }

        public Assembly[] Assemblies { get; }

        public IBackendFxApplicationInvoker Invoker { get; }

        public ICompositionRoot CompositionRoot { get; }

        public IExceptionLogger ExceptionLogger { get; }

        public virtual void EnableFeature(Feature feature)
        {
            if (_bootAction.IsValueCreated)
            {
                throw new InvalidOperationException("Features must be enabled before booting the application");
            }

            feature.Enable(this);
            _features.Add(feature);
        }

        public void RequireDependantFeature<TFeature>() where TFeature : Feature
        {
            if (!_features.OfType<TFeature>().Any())
            {
                throw new InvalidOperationException(
                    $"This feature requires the {typeof(TFeature).Name} to be enabled first");
            }
        }

        public async Task BootAsync(CancellationToken cancellationToken = default)
        {
            await _bootAction.Value.ConfigureAwait(false);
        }

        public async Task WaitForBootAsync(CancellationToken cancellationToken = default)
        {
            await Task.Run(async () =>
            {
                do
                {
                    if (cancellationToken.IsCancellationRequested ||
                        _bootAction.IsValueCreated && _bootAction.Value.Status is TaskStatus.Canceled or TaskStatus.Faulted or TaskStatus.RanToCompletion)
                    {
                        return;
                    }

                    await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                } while (true);
            }, cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Logger.LogInformation("Application shut down initialized");
            foreach (Feature feature in _features)
            {
                feature.Dispose();
            }

            CompositionRoot?.Dispose();
        }
    }
}