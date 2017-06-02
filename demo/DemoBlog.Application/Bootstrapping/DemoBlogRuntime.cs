namespace DemoBlog.Bootstrapping
{
    using System.Linq;
    using System.Reflection;
    using Backend.Fx.EfCorePersistence;
    using Backend.Fx.Environment.DateAndTime;
    using Backend.Fx.Environment.MultiTenancy;
    using FluentScheduler;
    using Jobs;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class DemoBlogRuntime : SimpleInjectorEfCoreRuntime<BlogDbContext>
    {
        private readonly bool doCreateInitialDemoTenant;
        private readonly DbContextOptions dbContextOptions;

        public DemoBlogRuntime(bool doCreateInitialDemoTenant, DbContextOptions dbContextOptions)
            : base(new DatabaseManager<BlogDbContext>(() => new BlogDbContext(dbContextOptions)), () => new BlogDbContext(dbContextOptions))
        {
            this.doCreateInitialDemoTenant = doCreateInitialDemoTenant;
            this.dbContextOptions = dbContextOptions;
        }

        public TenantId DefaultTenantId { get; private set; }

        protected override Assembly[] Assemblies
        {
            get
            {
                return new[] { GetType().GetTypeInfo().Assembly };
            }
        }

        protected override void InitializeJobScheduler()
        {
            // start my scheduling engine, in this case FluentScheduler
            JobManager.JobFactory = new ApplicationRuntimeJobFactory(this);
            JobManager.Initialize(new JobSchedule());
        }

        protected override void BootApplication()
        {
            // decide how the clock is going inside the runtime scope
            Container.Register<IClock, FrozenClock>();

            // initialize the database
            Container.RegisterSingleton(dbContextOptions);
            BootDatabase();

            // create some demo data, if required
            if (doCreateInitialDemoTenant)
            {
                var demoTenant = TenantManager.GetTenants().SingleOrDefault(t => t.IsDemoTenant && t.Name == Tenant.DemonstrationTenantName);
                if (demoTenant == null)
                {
                    TenantManager.CreateDemonstrationTenant(Tenant.DemonstrationTenantName, Tenant.DemonstrationTenantDescription, true);
                }
            }
            else
            {
                var defaultTenant = TenantManager.GetTenants().SingleOrDefault(t => t.IsDefault);
                if (defaultTenant == null)
                {
                    TenantManager.CreateProductionTenant("Default", "", true);
                }
            }

            Tenant[] tenants = TenantManager.GetTenants();
            DefaultTenantId = new TenantId(tenants.SingleOrDefault(t => t.IsDefault)?.Id);

            // subscribe to integration events
            //SubscribeToIntegrationEvent<SmtpMessageQueued>(smq => ExecuteJob<SendMailsJob>(smq.TenantId, 5));
        }
    }
}
