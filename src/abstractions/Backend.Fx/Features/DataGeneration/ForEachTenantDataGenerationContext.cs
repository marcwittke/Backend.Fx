using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.MultiTenancy;

namespace Backend.Fx.Features.DataGeneration
{
    public class ForEachTenantDataGenerationContext : IDataGenerationContext
    {
        private readonly ITenantEnumerator _tenantEnumerator;
        private readonly ITenantWideMutexManager _tenantWideMutexManager;
        private readonly IDataGenerationContext _dataGenerationContext;

        public ForEachTenantDataGenerationContext(
            ITenantEnumerator tenantEnumerator,
            ITenantWideMutexManager tenantWideMutexManager,
            IDataGenerationContext dataGenerationContext)
        {
            _tenantEnumerator = tenantEnumerator;
            _tenantWideMutexManager = tenantWideMutexManager;
            _dataGenerationContext = dataGenerationContext;
        }

        public Task<Type[]> GetDataGeneratorTypesAsync(IBackendFxApplicationInvoker invoker)
        {
            return _dataGenerationContext.GetDataGeneratorTypesAsync(invoker);
        }

        public async Task GenerateDataAsync(IBackendFxApplicationInvoker invoker, IEnumerable<Type> dataGeneratorTypes,
            CancellationToken cancellationToken = default)
        {
            dataGeneratorTypes = dataGeneratorTypes as Type[] ?? dataGeneratorTypes.ToArray();

            await _dataGenerationContext.GenerateDataAsync(
                    new ForEachTenantIdInvoker(
                        _tenantEnumerator.GetActiveTenantIds(),
                        _tenantWideMutexManager,
                        "DataGeneration",
                        invoker),
                    dataGeneratorTypes.Where(t => typeof(IProductiveDataGenerator).IsAssignableFrom(t)),
                    cancellationToken)
                .ConfigureAwait(false);

            await _dataGenerationContext.GenerateDataAsync(
                    new ForEachTenantIdInvoker(
                        _tenantEnumerator.GetActiveDemoTenantIds(),
                        _tenantWideMutexManager,
                        "DataGeneration",
                        invoker),
                    dataGeneratorTypes.Where(t => typeof(IDemoDataGenerator).IsAssignableFrom(t)),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}