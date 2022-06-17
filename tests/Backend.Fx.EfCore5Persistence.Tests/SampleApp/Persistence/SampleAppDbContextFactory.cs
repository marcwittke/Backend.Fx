using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Backend.Fx.EfCore5Persistence.Tests.SampleApp.Persistence
{
    [UsedImplicitly]
    [Obsolete("Only for migration support at design time")]
    public class SampleAppDbContextFactory : IDesignTimeDbContextFactory<SampleAppDbContext>
    {
        public SampleAppDbContext CreateDbContext(string[] args)
        {
            return new SampleAppDbContext(new DbContextOptionsBuilder<SampleAppDbContext>().UseSqlite("DataSource=:memory:").Options);
        }
    }
}