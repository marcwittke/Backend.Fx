namespace Backend.Fx.Bootstrapping.Modules
{
    using EfCorePersistence.Mssql;
    using Microsoft.EntityFrameworkCore;
    using Patterns.IdGeneration;
    using SimpleInjector;

    public class MsSqlServicesModule<TDbContext> : SimpleInjectorModule where TDbContext : DbContext
    {
        private readonly DbContextOptions<TDbContext> dbContextOptions;

        public MsSqlServicesModule(SimpleInjectorCompositionRoot compositionRoot, DbContextOptions<TDbContext> dbContextOptions)
            : base(compositionRoot)
        {
            this.dbContextOptions = dbContextOptions;
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            container.RegisterSingleton<IEntityIdGenerator>(new MsSqlSequenceEntityIdGenerator<TDbContext>(dbContextOptions));
        }
    }
}
