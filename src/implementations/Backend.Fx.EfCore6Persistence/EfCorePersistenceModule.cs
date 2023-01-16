using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Features.Persistence.AdoNet;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.EfCore6Persistence;

[PublicAPI]
public class EfCorePersistenceModule<TDbContext> : AdoNetPersistenceModule
    where TDbContext : DbContext 
{
    private readonly IDbContextOptionsFactory<TDbContext> _dbContextOptionsFactory;
    private readonly IEnumerable<Assembly> _assemblies;

    public EfCorePersistenceModule(
        IDbConnectionFactory dbConnectionFactory,
        IDbContextOptionsFactory<TDbContext> dbContextOptionsFactory,
        params Assembly[] assemblies)
        : base(dbConnectionFactory)
    {
        _dbContextOptionsFactory = dbContextOptionsFactory;
        _assemblies = assemblies;
    }

    public override void Register(ICompositionRoot compositionRoot)
    {
        base.Register(compositionRoot);
    
        compositionRoot.Register(
            ServiceDescriptor.Scoped(sp =>
                _dbContextOptionsFactory.GetDbContextOptions(sp.GetRequiredService<IDbConnection>())));

        compositionRoot.Register(
            ServiceDescriptor.Scoped<DbContext, TDbContext>());

        compositionRoot.Register(
            ServiceDescriptor.Scoped(typeof(IQueryable<>), typeof(EfCoreQueryable<>)));

        compositionRoot.RegisterDecorator(
            ServiceDescriptor.Scoped<ICanFlush, EfFlush>());
        
        compositionRoot.RegisterDecorator(
            ServiceDescriptor.Scoped<IUnitOfWork, EfUnitOfWork>());

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
        IDbContextOptionsFactory<TDbContext> dbContextOptionsFactory,
        params Assembly[] assemblies)
        : base(dbConnectionFactory, dbContextOptionsFactory, assemblies)
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