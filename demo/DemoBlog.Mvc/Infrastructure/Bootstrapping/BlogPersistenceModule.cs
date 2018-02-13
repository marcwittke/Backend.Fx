namespace DemoBlog.Mvc.Infrastructure.Bootstrapping
{
    using Backend.Fx.Bootstrapping;
    using Backend.Fx.Bootstrapping.Modules;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class BlogPersistenceModule : EfCorePersistenceModule<BlogDbContext> {
        public BlogPersistenceModule(SimpleInjectorCompositionRoot compositionRoot, DbContextOptions<BlogDbContext> dbContextOptions) : base(compositionRoot, dbContextOptions)
        { }
    }
}