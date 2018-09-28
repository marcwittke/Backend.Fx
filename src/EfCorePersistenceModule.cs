using System.Linq;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.EfCorePersistence;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Modules
{
    /// <summary>
    /// Wires all EF Core persistence services together: The concrete <see cref="DbContext"/>, all <see cref="IRepository{TAggregateRoot}"/>s, 
    /// <see cref="IAggregateMapping{T}"/>s, <see cref="IQueryable{T}"/> and the various forms of <see cref="IUnitOfWork"/>
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    public abstract class EfCorePersistenceModule<TDbContext> : SimpleInjectorModule 
        where TDbContext : DbContext
    {
        private readonly DbContextOptions<TDbContext> dbContextOptions;

        protected EfCorePersistenceModule(DbContextOptions<TDbContext> dbContextOptions)
        {
            this.dbContextOptions = dbContextOptions;
        }

        protected override void Register(Container container, ScopedLifestyle lifestyle)
        {
            container.RegisterInstance(dbContextOptions);
            container.Register<DbContext,TDbContext>();

            // EF Repositories
            container.Register(typeof(IRepository<>), typeof(EfRepository<>));
            container.Register(typeof(IAggregateMapping<>), new[] { typeof(TDbContext).GetTypeInfo().Assembly });

            // IQueryable for framework use only, since it bypasses authorization
            container.Register(typeof(IQueryable<>), typeof(EntityQueryable<>));

            // EF unit of work
            var uowRegistration = lifestyle.CreateRegistration<EfUnitOfWork>(container);
            container.AddRegistration(typeof(IUnitOfWork), uowRegistration);
            container.AddRegistration(typeof(ICanFlush), uowRegistration);
            container.AddRegistration(typeof(ICanInterruptTransaction), uowRegistration);
            container.Register<IReadonlyUnitOfWork, ReadonlyEfUnitOfWork>();
        }
    }
}