namespace Backend.Fx.Bootstrapping.Modules
{
    using System.Linq;
    using System.Reflection;
    using Bootstrapping;
    using BuildingBlocks;
    using EfCorePersistence;
    using Microsoft.EntityFrameworkCore;
    using Patterns.UnitOfWork;
    using SimpleInjector;

    public class EfCorePersistenceModule<TDbContext> : SimpleInjectorModule 
        where TDbContext : DbContext
    {
        private readonly DbContextOptions<TDbContext> dbContextOptions;

        public EfCorePersistenceModule(SimpleInjectorCompositionRoot compositionRoot, DbContextOptions<TDbContext> dbContextOptions) : base(compositionRoot)
        {
            this.dbContextOptions = dbContextOptions;
        }

        protected override void Register(Container container, ScopedLifestyle lifestyle)
        {
            container.RegisterSingleton(dbContextOptions);
            container.Register<DbContext,TDbContext>();

            // EF Repositories
            container.Register(typeof(IRepository<>), typeof(EfRepository<>));
            container.Register(typeof(IAggregateRootMapping<>), new[] { typeof(TDbContext).GetTypeInfo().Assembly });

            // IQueryable for framework use only, since it bypasses authorization
            container.Register(typeof(IQueryable<>), typeof(AggregateQueryable<>));

            // EF unit of work
            var uowRegistration = lifestyle.CreateRegistration<EfUnitOfWork>(container);
            container.AddRegistration(typeof(IUnitOfWork), uowRegistration);
            container.AddRegistration(typeof(ICanFlush), uowRegistration);
            container.AddRegistration(typeof(ICanInterruptTransaction), uowRegistration);
            container.Register<IReadonlyUnitOfWork, ReadonlyEfUnitOfWork>();
        }
    }
}