using System;
using System.Linq;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    public class EfTenantManager<TDbContext> : TenantManager where TDbContext : DbContext
    {
        private readonly IScopeManager _scopeManager;
        
        public EfTenantManager(IScopeManager scopeManager)
        {
            _scopeManager = scopeManager;
        }

        public override TenantId[] GetTenantIds()
        {
            using (var scope = _scopeManager.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                var dbContext = scope.GetInstance<TDbContext>();
                return dbContext.Set<Tenant>().Select(t => new TenantId(t.Id)).ToArray();
            }
        }

        public override Tenant[] GetTenants()
        {
            using (var scope = _scopeManager.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                var dbContext = scope.GetInstance<TDbContext>();
                return dbContext.Set<Tenant>().ToArray();
            }
        }
        
        [CanBeNull]
        public override Tenant FindTenant(TenantId tenantId)
        {
            using (var scope = _scopeManager.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                var dbContext = scope.GetInstance<TDbContext>();
                return dbContext.Set<Tenant>().Find(tenantId.Value);
            }
        }

        public override void SaveTenant(Tenant tenant)
        {
            using (var scope = _scopeManager.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                var dbContext = scope.GetInstance<TDbContext>();
                var existingTenant = dbContext.Set<Tenant>().Find(tenant.Id);
                if (existingTenant == null)
                {
                    dbContext.Add(tenant);
                }
                else
                {
                    existingTenant.State = tenant.State;
                    if (existingTenant.IsDemoTenant && !tenant.IsDemoTenant)
                    {
                        throw new InvalidOperationException("It is not possible to convert a demonstration tenant to a productive tenant");
                    }
                    existingTenant.IsDemoTenant = tenant.IsDemoTenant;
                    existingTenant.Name = tenant.Name;
                    existingTenant.Description = tenant.Description;
                    existingTenant.UriMatchingExpression = tenant.UriMatchingExpression;
                }

                if (tenant.IsDefault)
                {
                    dbContext.Set<Tenant>().Where(t => t.Id != tenant.Id).ForAll(t => t.IsDefault = false);
                }

                dbContext.SaveChanges();
            }
        }
    }
}

