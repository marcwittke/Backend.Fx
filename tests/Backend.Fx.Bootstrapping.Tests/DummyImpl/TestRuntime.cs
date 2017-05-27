namespace Backend.Fx.Bootstrapping.Tests.DummyImpl
{
    using System;
    using System.Reflection;
    using BuildingBlocks;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using Environment.Persistence;
    using FakeItEasy;
    using Patterns.DependencyInjection;
    using Patterns.UnitOfWork;
    using SimpleInjector;
    using Testing;

    public class TestUnitOfWorkFactory
    {
        public Func<IUnitOfWork> CreateUnitOfWork()
        {
            return () => {
                       IUnitOfWork unitOfWork = A.Fake<IUnitOfWork>();
                       return unitOfWork;
                   };
        }

        public Func<IReadonlyUnitOfWork> CreateReadonlyUnitOfWork()
        {
            return () => {
                       IReadonlyUnitOfWork unitOfWork = A.Fake<IReadonlyUnitOfWork>();
                       return unitOfWork;
                   };
        }

        
    }

    public class TestRuntime : SimpleInjectorRuntime
    {
        public TestUnitOfWorkFactory UnitOfWorkFactory { get; } = new TestUnitOfWorkFactory();
        public bool BootPersistenceWasCalled { get; private set; }
        public bool BootApplicationWasCalled { get; private set; }
        public bool InitializeJobSchedulerWasCalled { get; private set; }

        public override ITenantManager TenantManager { get; }
        public override IDatabaseManager DatabaseManager { get; }

        public TestRuntime(ITenantManager tenantManager = null, IDatabaseManager databaseManager = null)
        {
            TenantManager = tenantManager ?? new InMemoryTenantManager(this);
            DatabaseManager = databaseManager;
        }

        protected override Assembly[] Assemblies
        {
            get { return new[] {GetType().GetTypeInfo().Assembly}; }
        }

        protected override void BootPersistence()
        {
            Container.Register(typeof(IRepository<>), typeof(ThreadLocalInMemoryRepository<>));
            BootPersistenceWasCalled = true;
        }

        protected override void BootApplication()
        {
            Container.Register(() => new LateResolver<IAmLateResolved>(() => Container.GetInstance<IAmLateResolved>()), Lifestyle.Singleton);
            Container.Register<IClock, FrozenClock>();
            Container.Register(UnitOfWorkFactory.CreateUnitOfWork());
            Container.Register(UnitOfWorkFactory.CreateReadonlyUnitOfWork());
            BootApplicationWasCalled = true;
        }

        protected override void InitializeJobScheduler()
        {
            InitializeJobSchedulerWasCalled = true;
        }

        internal Scope GetCurrentScopeForTestsOnly()
        {
            return ScopedLifestyle.GetCurrentScope(Container);
        }
    }
}