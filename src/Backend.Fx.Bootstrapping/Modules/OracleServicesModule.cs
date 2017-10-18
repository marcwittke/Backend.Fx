namespace Backend.Fx.Bootstrapping.Modules
{
    using EfCorePersistence.Oracle;
    using Microsoft.EntityFrameworkCore;
    using Patterns.IdGeneration;
    using SimpleInjector;

    public class OracleServicesModule<TDbContext> : SimpleInjectorModule where TDbContext : DbContext
    {
        private readonly DbContextOptions<TDbContext> dbContextOptions;

        public OracleServicesModule(SimpleInjectorCompositionRoot compositionRoot, DbContextOptions<TDbContext> dbContextOptions)
            : base(compositionRoot)
        {
            this.dbContextOptions = dbContextOptions;
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            container.RegisterSingleton<IEntityIdGenerator>(new OracleSequenceEntityIdGenerator<TDbContext>(dbContextOptions));
        }
    }
}