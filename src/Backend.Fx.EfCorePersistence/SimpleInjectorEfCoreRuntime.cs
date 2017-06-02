namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Reflection;
    using Bootstrapping;
    using BuildingBlocks;
    using Environment.MultiTenancy;
    using Environment.Persistence;
    using Microsoft.EntityFrameworkCore;
    using Patterns.UnitOfWork;

    public abstract class SimpleInjectorEfCoreRuntime<TDbContext> : SimpleInjectorRuntime where TDbContext : DbContext
    {
        protected SimpleInjectorEfCoreRuntime(IDatabaseManager databaseManager, Func<TDbContext> frameworkDbContextFactory)
        {
            DatabaseManager = databaseManager;
            TenantManager = new TenantManager<TDbContext>(this, frameworkDbContextFactory);
        }

        public override IDatabaseManager DatabaseManager { get; }

        public override ITenantManager TenantManager { get; }

        protected override void BootPersistence()
        {
            // dbContext is being registerd with all its base types as aliases. This is mainly for ASP.NET Identity, 
            // that requires the application's DbContext to inherit from IdentityDbContext<T>.
            var dbContextRegistration = ScopedLifestyle.CreateRegistration<TDbContext>(Container);
            var dbContextType = typeof(TDbContext);
            while (typeof(object) != dbContextType)
            {
                Container.AddRegistration(dbContextType, dbContextRegistration);
                dbContextType = dbContextType.GetTypeInfo().BaseType;
            }

            Container.Register(typeof(IRepository<>), typeof(EfRepository<>));
            Container.Register(typeof(IAggregateRootMapping<>), Assemblies);

            // unit of work
            var uowRegistration = ScopedLifestyle.CreateRegistration<EfUnitOfWork>(Container);
            Container.AddRegistration(typeof(IUnitOfWork), uowRegistration);
            Container.AddRegistration(typeof(ICanFlush), uowRegistration);
            Container.Register<IReadonlyUnitOfWork, ReadonlyEfUnitOfWork>();
        }

        protected void BootDatabase()
        {
            DatabaseManager.EnsureDatabaseExistence();
        }
    }
}
