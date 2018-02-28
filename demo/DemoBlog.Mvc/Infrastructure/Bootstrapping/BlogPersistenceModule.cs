namespace DemoBlog.Mvc.Infrastructure.Bootstrapping
{
    using Backend.Fx.Bootstrapping.Modules;
    using Backend.Fx.Patterns.IdGeneration;
    using Microsoft.EntityFrameworkCore;
    using Persistence;
    using SimpleInjector;

    public class BlogPersistenceModule : EfCorePersistenceModule<BlogDbContext> {
        public BlogPersistenceModule(DbContextOptions<BlogDbContext> dbContextOptions) : base(dbContextOptions)
        { }

        protected override void Register(Container container, ScopedLifestyle lifestyle)
        {
            base.Register(container, lifestyle);
            container.RegisterSingleton<IEntityIdGenerator, BlogEntityIdGenerator>();
        }
    }
}