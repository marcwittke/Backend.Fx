using System.Data;
using System.Linq;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Features.Persistence.AdoNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.EfCore6Persistence;

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