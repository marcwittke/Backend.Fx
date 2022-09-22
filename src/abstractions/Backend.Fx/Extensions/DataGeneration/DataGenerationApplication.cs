using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Extensions.DataGeneration
{
    /// <summary>
    /// The feature "Data Generation" makes sure that all implemented data generators are executed on application boot
    /// </summary>
    [PublicAPI]
    public class DataGenerationApplication : BackendFxApplicationExtension
    {
        private readonly bool _runDemoDataGenerators;

        public DataGenerationApplication(IBackendFxApplication application, bool runDemoDataGenerators) : base(
            application)
        {
            _runDemoDataGenerators = runDemoDataGenerators;
            application.CompositionRoot.RegisterModules(new DataGenerationModule(application.Assemblies));
        }

        public override async Task BootAsync(CancellationToken cancellationToken = default)
        {
            await base.BootAsync(cancellationToken).ConfigureAwait(false);
            await RunDataGenerators(cancellationToken).ConfigureAwait(false);
        }

        private async Task RunDataGenerators(CancellationToken cancellationToken)
        {
            var dataGeneratorTypes = Type.EmptyTypes;
            await Invoker.InvokeAsync(sp =>
            {
                dataGeneratorTypes = sp
                    .GetServices<IDataGenerator>()
                    .OrderBy(dg => dg.Priority)
                    .Select(dg => dg.GetType())
                    .ToArray();
                return Task.CompletedTask;
            }, new SystemIdentity()).ConfigureAwait(false);

            foreach (var dataGeneratorType in dataGeneratorTypes)
            {
                if (typeof(IProductiveDataGenerator).IsAssignableFrom(dataGeneratorType)
                    || typeof(IDemoDataGenerator).IsAssignableFrom(dataGeneratorType) && _runDemoDataGenerators)
                {
                    await Invoker.InvokeAsync(async sp =>
                    {
                        var dataGenerator = (IDataGenerator)sp.GetRequiredService(dataGeneratorType);
                        await dataGenerator.GenerateAsync().ConfigureAwait(false);
                    }, new SystemIdentity()).ConfigureAwait(false);
                }
            }
        }
    }
}