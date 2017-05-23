namespace DemoBlog.Mvc.Data.Identity
{
    using System;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    [UsedImplicitly, Obsolete("Only intended for use of design time tools like 'Add-Migration'")]
    public class BlogIdentityDbContextFactory : IDbContextFactory<BlogIdentityDbContext>
    {
        public BlogIdentityDbContext Create(DbContextFactoryOptions options)
        {
            var dbContextOptions = new DbContextOptionsBuilder()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=blogDbContext-0982f2f2193841bea9fcdd617788f737;Trusted_Connection=True;", b => b.MigrationsAssembly("DemoBlog.Mvc"))
                .Options;

            return new BlogIdentityDbContext(dbContextOptions);
        }
    }
}