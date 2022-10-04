using System.Data;
using System.Linq;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Features.Persistence.AdoNet;
using Backend.Fx.Util;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.EfCore6Persistence;

[PublicAPI]
public class EfCorePersistenceModule<TDbContext> : AdoNetPersistenceModule
    where TDbContext : DbContext
{
    private readonly IDbContextOptionsFactory<TDbContext> _dbContextOptionsFactory;

    public EfCorePersistenceModule(
        IDbConnectionFactory dbConnectionFactory,
        IDbContextOptionsFactory<TDbContext> dbContextOptionsFactory)
        : base(dbConnectionFactory)
    {
        _dbContextOptionsFactory = dbContextOptionsFactory;
    }

    protected override void RegisterImplementationSpecificServices(ICompositionRoot compositionRoot)
    {
        compositionRoot.Register(
            ServiceDescriptor.Scoped(sp =>
                _dbContextOptionsFactory.GetDbContextOptions(sp.GetRequiredService<IDbConnection>())));

        compositionRoot.Register(
            ServiceDescriptor.Scoped<DbContext, TDbContext>());

        compositionRoot.Register(
            ServiceDescriptor.Scoped(typeof(IQueryable<>), typeof(EfCoreQueryable<>)));

        compositionRoot.Register(
            ServiceDescriptor.Scoped(typeof(IRepository<,>), typeof(EfCoreRepository<,>)));

        compositionRoot.RegisterDecorator(
            ServiceDescriptor.Scoped<ICanFlush, EfFlush>());

        compositionRoot.RegisterDecorator(
            ServiceDescriptor.Scoped<IOperation, DbContextTransactionOperationDecorator>());
    }
}

/// <summary>
/// Use this module, when your application should support full transparent multi tenancy. The DbContext will add a
/// shadow "TenantId" property on each aggregate root and a respective global query filter to all DbContext instances.
/// The <see cref="ICurrentTHolder{TenantId}"/> will be asked for the current tenant id.  
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public class EfCoreMultiTenancyPersistenceModule<TDbContext> : EfCorePersistenceModule<TDbContext>
    where TDbContext : MultiTenancyDbContext
{
    public EfCoreMultiTenancyPersistenceModule(
        IDbConnectionFactory dbConnectionFactory,
        IDbContextOptionsFactory<TDbContext> dbContextOptionsFactory)
        : base(dbConnectionFactory, dbContextOptionsFactory)
    {
    }

    /// <summary>
    /// there is no module to be added. This multi tenancy module is just to ensure the more specific DbContext type
    /// when enabling the feature with multi tenancy
    /// </summary>
    public override IModule MultiTenancyModule => new NullModule();
    
    private class NullModule : IModule
    {
        public void Register(ICompositionRoot compositionRoot)
        {
        }
    }
}
