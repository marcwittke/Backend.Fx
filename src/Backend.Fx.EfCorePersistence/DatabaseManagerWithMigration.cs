namespace Backend.Fx.EfCorePersistence
{
    using Microsoft.EntityFrameworkCore;

    public class DatabaseManagerWithMigration<TDbContext> : DatabaseManager<TDbContext> where TDbContext : DbContext
    {
        public DatabaseManagerWithMigration(DbContextOptions<TDbContext> dbContextOptions) : base(dbContextOptions)
        { }

        protected override void ExecuteCreationStrategy(DbContext dbContext)
        {
            dbContext.Database.Migrate();
        }
    }
}