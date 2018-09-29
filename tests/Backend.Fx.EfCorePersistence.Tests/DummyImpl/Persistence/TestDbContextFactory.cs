using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence
{
    [UsedImplicitly, Obsolete("Only for migration support at design time")]
    public class TestDbContextFactory : IDbContextFactory<TestDbContext>
    {
        public TestDbContext Create(DbContextFactoryOptions options)
        {
            return new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().UseSqlite("DataSource=:memory:").Options);
        }
    }
}
