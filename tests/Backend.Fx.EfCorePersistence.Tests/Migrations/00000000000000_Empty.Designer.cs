using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl;

namespace Backend.Fx.EfCorePersistence.Tests.Migrations
{
    [DbContext(typeof(TestDbContext))]
    [Migration("00000000000000_Initial")]
    partial class Empty
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "1.1.2");            
        }
    }
}
