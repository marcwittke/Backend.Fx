namespace DemoBlog.Mvc.Data.Application
{
    using System;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Persistence;

    [UsedImplicitly, Obsolete("Only intended for use of design time tools like 'Add-Migration'")]
    public class BlogDbContextFactory : IDesignTimeDbContextFactory<BlogDbContext>
    {

        public BlogDbContext CreateDbContext(string[] args)
        {
            var dbContextOptions = new DbContextOptionsBuilder<BlogDbContext>()
                                   .UseSqlServer(
                                           "Server=(localdb)\\mssqllocaldb;Database=blogDbContext-Migrations;Trusted_Connection=True;",
                                           bld => bld.MigrationsAssembly("DemoBlog.Mvc"))
                                   .Options;

            return new BlogDbContext(dbContextOptions);
        }
    }
}
