namespace Backend.Fx.Bootstrapping.Tests
{
    using System.Reflection;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using Environment.Persistence;
    using Patterns.DependencyInjection;
    using SimpleInjector;

    public class TestRuntime : SimpleInjectorRuntime
    {
        public bool BootPersistenceWasCalled { get; private set; }
        public bool BootApplicationWasCalled { get; private set; }
        public bool InitializeJobSchedulerWasCalled { get; private set; }

        public override ITenantManager TenantManager { get; }
        public override IDatabaseManager DatabaseManager { get; }

        public TestRuntime(ITenantManager tenantManager, IDatabaseManager databaseManager)
        {
            TenantManager = tenantManager;
            DatabaseManager = databaseManager;
        }

        protected override Assembly[] Assemblies
        {
            get { return new[] {GetType().GetTypeInfo().Assembly}; }
        }

        protected override void BootPersistence()
        {
            BootPersistenceWasCalled = true;
        }

        protected override void BootApplication()
        {
            Container.Register(() => new LateResolver<IAmLateResolved>(() => Container.GetInstance<IAmLateResolved>()), Lifestyle.Singleton);
            Container.Register<IClock, FrozenClock>();

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