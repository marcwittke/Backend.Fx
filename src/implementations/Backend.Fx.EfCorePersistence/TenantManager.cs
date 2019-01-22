using System.Linq;
using Backend.Fx.Extensions;

namespace Backend.Fx.EfCorePersistence
{
    using System;
    using Environment.MultiTenancy;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;

    public class TenantManager<TDbContext> : TenantManager where TDbContext : DbContext
    {
        private readonly Func<TDbContext> _dbContextFactory;

        public TenantManager(ITenantInitializer tenantInitializer, Func<TDbContext> dbContextFactory) 
            : base(tenantInitializer)
        {
            _dbContextFactory = dbContextFactory;
        }

        public override TenantId[] GetTenantIds()
        {
            using (var dbContext = _dbContextFactory())
            {
                return dbContext.Set<Tenant>().Select(t => new TenantId(t.Id)).ToArray();
            }
        }

        public override Tenant[] GetTenants()
        {
            using (var dbContext = _dbContextFactory())
            {
                return dbContext.Set<Tenant>().ToArray();
            }
        }
        
        [CanBeNull]
        public override Tenant FindTenant(TenantId tenantId)
        {
            using (var dbContext = _dbContextFactory())
            {
                return dbContext.Set<Tenant>().Find(tenantId.Value);
            }
        }

        protected override void SaveTenant(Tenant tenant)
        {
            using (var dbContext = _dbContextFactory())
            {
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

