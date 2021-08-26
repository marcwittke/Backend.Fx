using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection.Pure;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    public abstract class EfCoreScopedServices<TDbContext> : ScopedServices where TDbContext : DbContext
    {
        public TDbContext DbContext { get; }

        public IDbConnection DbConnection
        {
            get
            {
                var dbConnection = DbContext.Database.GetDbConnection();
                if (dbConnection.State == ConnectionState.Closed)
                {
                    dbConnection.Open();
                }
                return dbConnection;
            }
        }

        protected EfCoreScopedServices(TDbContext dbContext,
                                       IClock clock,
                                       IIdentity identity,
                                       TenantId tenantId,
                                       params Assembly[] assemblies)
            : base(clock, identity, tenantId, assemblies)
        {
            DbContext = dbContext;
        }


        public override IAsyncRepository<TAggregateRoot> GetAsyncRepository<TAggregateRoot>()
        {
            return (IAsyncRepository<TAggregateRoot>)GetRepository(typeof(TAggregateRoot));
        }
        
        public override IRepository<TAggregateRoot> GetRepository<TAggregateRoot>()
        {
            return (IRepository<TAggregateRoot>)GetRepository(typeof(TAggregateRoot));
        }


        private object GetRepository(Type aggregateRootType)
        {
            object aggregateAuthorization = GetAggregateAuthorization(IdentityHolder, aggregateRootType);
            Type efRepositoryType = typeof(EfRepository<>).MakeGenericType(aggregateRootType);
            return Activator.CreateInstance(
                efRepositoryType,
                ((EfFlush)CanFlush).DbContext,
                GetAggregateMapping(aggregateRootType),
                TenantIdHolder,
                aggregateAuthorization);
        }

        protected IAggregateMapping GetAggregateMapping(Type aggregateRootType)
        {
            Type aggregateDefinitionType = typeof(TDbContext)
                                           .Assembly
                                           .GetTypes()
                                           .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract)
                                           .SingleOrDefault(t =>
                                               typeof(IAggregateMapping<>).MakeGenericType(aggregateRootType).GetTypeInfo()
                                                                          .IsAssignableFrom(t));
            if (aggregateDefinitionType == null)
            {
                throw new InvalidOperationException($"No Aggregate Definition for {aggregateRootType.Name} found");
            }

            return (IAggregateMapping)Activator.CreateInstance(aggregateDefinitionType);
        }
    }
}