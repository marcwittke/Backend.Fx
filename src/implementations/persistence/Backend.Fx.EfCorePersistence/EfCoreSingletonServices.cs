using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.Patterns.DependencyInjection.Pure;
using Backend.Fx.Patterns.IdGeneration;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    public abstract class EfCoreSingletonServices<TDbContext, TScopedServices> : SingletonServices<TScopedServices>
        where TScopedServices : IScopedServices where TDbContext : DbContext
    {
        public DbContextOptions<TDbContext> DbContextOptions { get; }

        public string ConnectionString { get; } 
        
        public override IEntityIdGenerator EntityIdGenerator { get; }

        protected EfCoreSingletonServices(
            string connectionString, 
            DbContextOptions<TDbContext> dbContextOptions,
            IEntityIdGenerator entityIdGenerator,
            params Assembly[] assemblies) 
            : base((assemblies ?? Array.Empty<Assembly>()).Concat(new [] {typeof(EfCoreSingletonServices<,>).Assembly }).ToArray())
        {
            ConnectionString = connectionString;
            DbContextOptions = dbContextOptions;
            EntityIdGenerator = entityIdGenerator;
        }
    }
}