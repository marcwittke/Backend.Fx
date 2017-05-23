namespace DemoBlog.Bootstrapping
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Backend.Fx.EfCorePersistence;
    using Backend.Fx.Environment.DateAndTime;
    using Backend.Fx.Environment.MultiTenancy;
    using FluentScheduler;
    using Jobs;
    using Microsoft.EntityFrameworkCore;
    using Persistence;
    using SimpleInjector;

    public class DemoBlogRuntime : SimpleInjectorEfCoreRuntime<BlogDbContext>
    {
        private readonly bool doCreateInitialDemoTenant;
        private readonly DbContextOptions dbContextOptions;

        public DemoBlogRuntime(bool doCreateInitialDemoTenant, DbContextOptions dbContextOptions)
            : base(() => new BlogDbContext(dbContextOptions))
        {
            this.doCreateInitialDemoTenant = doCreateInitialDemoTenant;
            this.dbContextOptions = dbContextOptions;
        }
        
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
            if (doCreateInitialDemoTenant && !TenantManager.GetTenants().Any(t => t.IsDemoTenant && t.Name == Tenant.DemonstrationTenantName))
            {
                TenantManager.CreateDemonstrationTenant(Tenant.DemonstrationTenantName, Tenant.DemonstrationTenantDescription);
            }

            // subscribe to integration events
            //SubscribeToIntegrationEvent<SmtpMessageQueued>(smq => ExecuteJob<SendMailsJob>(smq.TenantId, 5));
        }
    }
}
