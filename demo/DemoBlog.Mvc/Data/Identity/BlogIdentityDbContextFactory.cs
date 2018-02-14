namespace DemoBlog.Mvc.Data.Identity
{
    using System;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    
    [UsedImplicitly, Obsolete("Only intended for use of design time tools like 'Add-Migration'")]
    public class BlogIdentityDbContextFactory : IDesignTimeDbContextFactory<BlogIdentityDbContext>
    {
        public BlogIdentityDbContext CreateDbContext(string[] args)
        {
            var dbContextOptions = new DbContextOptionsBuilder()
                                   .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=blogDbContext-Migrations;Trusted_Connection=True;",
                                                 bld => bld.MigrationsAssembly("DemoBlog.Mvc"))
            
                                   .Options;

            return new BlogIdentityDbContext(dbContextOptions);
        }
    }
}