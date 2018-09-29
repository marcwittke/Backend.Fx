using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;

namespace Backend.Fx.EfCorePersistence.Tests.Migrations
{
    [DbContext(typeof(TestDbContext))]
    [Migration("00000000000000_Empty")]
    partial class Empty
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "1.1.2");            
        }
    }
}
