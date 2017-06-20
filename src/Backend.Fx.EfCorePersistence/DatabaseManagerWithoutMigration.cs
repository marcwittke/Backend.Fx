namespace Backend.Fx.EfCorePersistence
{
    using Microsoft.EntityFrameworkCore;

    public class DatabaseManagerWithoutMigration<TDbContext> : DatabaseManager<TDbContext> where TDbContext : DbContext
    {
        public DatabaseManagerWithoutMigration(DbContextOptions options) : base(options)
        { }

        protected override void ExecuteCreationStrategy(DbContext dbContext)
        {
            dbContext.Database.EnsureCreated();
        }
    }
}