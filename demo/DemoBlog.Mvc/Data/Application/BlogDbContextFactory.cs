namespace DemoBlog.Mvc.Data.Application
{
    using System;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Persistence;

    [UsedImplicitly, Obsolete("Only intended for use of design time tools like 'Add-Migration'")]
    public class BlogDbContextFactory : IDbContextFactory<BlogDbContext>
    {
        public BlogDbContext Create(DbContextFactoryOptions options)
        {
            var dbContextOptions = new DbContextOptionsBuilder<BlogDbContext>()
                .UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=blogDbContext-0982f2f2193841bea9fcdd617788f737;Trusted_Connection=True;",
                    bld => bld.MigrationsAssembly("DemoBlog.Mvc"))
                .Options;

            return new BlogDbContext(dbContextOptions);
        }
    }
}
