using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Extensions;
using Backend.Fx.Extensions.DataGeneration;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.TenantsAdmin
{
    [PublicAPI]
    public class TenantsAdminApplication : BackendFxApplicationExtension
    {
        private readonly IBackendFxApplication _backendFxApplication;
        private readonly ITenantWideMutexManager _tenantWideMutexManager;

        public TenantsAdminApplication(
            IBackendFxApplication backendFxApplication,
            ITenantRepository tenantRepository,
            ITenantWideMutexManager tenantWideMutexManager) : base(backendFxApplication)
        {
            _backendFxApplication = backendFxApplication;
            _tenantWideMutexManager = tenantWideMutexManager;
            TenantsService = new TenantService(tenantRepository);
        }

        public TenantService TenantsService { get; }


        public virtual async Task BootAsync()
        {
            var tenants = TenantsService.GetActiveTenants();
            await RunDataGenerators(tenants);
        }

        private async Task RunDataGenerators(IEnumerable<Tenant> tenants)
        {
            var dataGeneratorTypes = Type.EmptyTypes;
            await _backendFxApplication.Invoker.InvokeAsync(sp =>
            {
                dataGeneratorTypes = sp
                    .GetServices<IDataGenerator>()
                    .OrderBy(dg => dg.Priority)
                    .Select(dg => dg.GetType())
                    .ToArray();
                return Task.CompletedTask;
            }, new SystemIdentity());

            foreach (var tenant in tenants)
            {
                var tenantId = new TenantId(tenant.Id);

                if (_tenantWideMutexManager.TryAcquire(tenantId, "DataGeneration", out var mutex))
                {
                    try
                    {
                        foreach (var dataGeneratorType in dataGeneratorTypes)
                        {
                            if (typeof(IProductiveDataGenerator).IsAssignableFrom(dataGeneratorType)
                                || typeof(IDemoDataGenerator).IsAssignableFrom(dataGeneratorType) &&
                                tenant.IsDemoTenant)
                            {
                                await _backendFxApplication.Invoker.InvokeAsync(async sp =>
                                {
                                    sp.GetRequiredService<ICurrentTHolder<TenantId>>().ReplaceCurrent(tenantId);
                                    var dataGenerator = (IDataGenerator)sp.GetRequiredService(dataGeneratorType);
                                    await dataGenerator.GenerateAsync();
                                }, new SystemIdentity());
                            }
                        }
                    }
                    finally
                    {
                        mutex.Dispose();
                    }
                }
            }
        }
    }
}