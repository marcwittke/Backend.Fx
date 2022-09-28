using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.DataGeneration
{
    public interface IDataGenerationContext
    {
        Task<Type[]> GetDataGeneratorTypesAsync(IBackendFxApplicationInvoker invoker);
        Task GenerateDataAsync(IBackendFxApplicationInvoker invoker, IEnumerable<Type> dataGeneratorTypes, CancellationToken cancellationToken = default);
    }

    [UsedImplicitly]
    public class DataGenerationContext : IDataGenerationContext
    {
        public async Task<Type[]> GetDataGeneratorTypesAsync(IBackendFxApplicationInvoker invoker)
        {
            var dataGeneratorTypes = Type.EmptyTypes;
            await invoker.InvokeAsync(sp =>
            {
                dataGeneratorTypes = sp
                    .GetServices<IDataGenerator>()
                    .OrderBy(dg => dg.Priority)
                    .Select(dg => dg.GetType())
                    .ToArray();
                return Task.CompletedTask;
            }, new SystemIdentity()).ConfigureAwait(false);

            return dataGeneratorTypes;
        }

        public async Task GenerateDataAsync(IBackendFxApplicationInvoker invoker, IEnumerable<Type> dataGeneratorTypes,
            CancellationToken cancellationToken = default)
        {
            foreach (var dataGeneratorType in dataGeneratorTypes)
            {
                await invoker.InvokeAsync(async sp =>
                {
                    var dataGenerator = sp.GetServices<IDataGenerator>().Single(dgt => dgt.GetType() == dataGeneratorType);
                    await dataGenerator.GenerateAsync(cancellationToken).ConfigureAwait(false);
                }, new SystemIdentity()).ConfigureAwait(false);
            }
        }
    }
}