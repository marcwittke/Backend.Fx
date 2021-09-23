using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence
{
    [UsedImplicitly]
    [Obsolete("Only for migration support at design time")]
    public class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
    {
        public TestDbContext CreateDbContext(string[] args)
        {
            return new TestDbContext(
                new DbContextOptionsBuilder<TestDbContext>().UseSqlite("DataSource=:memory:").Options);
        }
    }
}
