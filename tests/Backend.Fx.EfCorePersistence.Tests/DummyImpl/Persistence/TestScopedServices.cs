using System.Reflection;
using System.Security.Principal;
using Backend.Fx.EfCorePersistence.Tests.DummyImpl.Persistence;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures
{
    public class TestScopedServices : EfCoreScopedServices<TestDbContext>
    {
        public TestScopedServices(TestDbContext dbContext, IClock clock, IIdentity identity, TenantId tenantId, params Assembly[] assemblies) 
            : base(dbContext, clock, identity, tenantId, assemblies)
        {
        }

        public override IDomainEventAggregator EventAggregator { get; } = A.Fake<IDomainEventAggregator>();
        
        public override IMessageBusScope MessageBusScope { get; } = A.Fake<IMessageBusScope>();
        
        protected override ICanFlush CreateCanFlush()
        {
            return new EfFlush(DbContext, IdentityHolder, Clock);
        }
    }
}