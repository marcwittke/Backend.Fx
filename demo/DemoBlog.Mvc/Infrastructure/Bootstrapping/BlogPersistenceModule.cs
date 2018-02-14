namespace DemoBlog.Mvc.Infrastructure.Bootstrapping
{
    using Backend.Fx.Bootstrapping;
    using Backend.Fx.Bootstrapping.Modules;
    using Backend.Fx.Patterns.IdGeneration;
    using Microsoft.EntityFrameworkCore;
    using Persistence;
    using SimpleInjector;

    public class BlogPersistenceModule : EfCorePersistenceModule<BlogDbContext> {
        public BlogPersistenceModule(SimpleInjectorCompositionRoot compositionRoot, DbContextOptions<BlogDbContext> dbContextOptions) : base(compositionRoot, dbContextOptions)
        { }

        protected override void Register(Container container, ScopedLifestyle lifestyle)
        {
            base.Register(container, lifestyle);
            container.RegisterSingleton<IEntityIdGenerator, BlogEntityIdGenerator>();
        }
    }
}