namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl
{
    using System.Reflection;
    using Environment.DateAndTime;
    using Microsoft.EntityFrameworkCore;

    public class TestRuntime : SimpleInjectorEfCoreRuntime<TestDbContext>
    {
        private readonly DbContextOptions dbContextOptions;

        public TestRuntime(DbContextOptions dbContextOptions)
            : base(new DatabaseManagerWithoutMigration<TestDbContext>(dbContextOptions), dbContextOptions)
        {
            this.dbContextOptions = dbContextOptions;
        }

        protected override Assembly[] Assemblies
        {
            get { return new[] { GetType().GetTypeInfo().Assembly }; }
        }

        protected override void BootApplication()
        {
            Container.RegisterSingleton(dbContextOptions);
            Container.Register<IClock, FrozenClock>();
            BootDatabase();
        }

        protected override void InitializeJobScheduler()
        { }
    }
}
