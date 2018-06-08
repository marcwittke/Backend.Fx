namespace Backend.Fx.Tests.Patterns.UnitOfWork
{
    using FakeItEasy;
    using Fx.Environment.Authentication;
    using Fx.Environment.DateAndTime;
    using Fx.Patterns.EventAggregation.Domain;
    using Fx.Patterns.EventAggregation.Integration;
    using Xunit;

    public class TheUnitOfWork
    {
        private readonly IDomainEventAggregator eventAggregator = A.Fake<IDomainEventAggregator>();
        private readonly IEventBusScope eventBusScope = A.Fake<IEventBusScope>();

        [Fact]
        public void CommitsOnComplete()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), eventAggregator, eventBusScope);
            sut.Begin();
            sut.Complete();
            sut.Dispose();
            Assert.Equal(0, sut.RollbackCount);
            Assert.Equal(1, sut.UpdateTrackingPropertiesCount);
            Assert.Equal(1, sut.CommitCount);
        }

        [Fact]
        public void RaisesDomainEventsOnComplete()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), eventAggregator, eventBusScope);
            sut.Begin();
            sut.Complete();
            sut.Dispose();
            A.CallTo(() => eventAggregator.RaiseEvents()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RaisesIntegrationEventsOnComplete()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), eventAggregator, eventBusScope);
            sut.Begin();
            sut.Complete();
            sut.Dispose();
            A.CallTo(() => eventBusScope.RaiseEvents()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RollsBackOnDispose()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), eventAggregator, eventBusScope);
            sut.Begin();
            sut.Dispose();
            Assert.Equal(1, sut.RollbackCount);
            Assert.Equal(0, sut.UpdateTrackingPropertiesCount);
            Assert.Equal(0, sut.CommitCount);
        }

        [Fact]
        public void RaisesNoDomainEventsOnDispose()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), eventAggregator, eventBusScope);
            sut.Begin();
            sut.Dispose();
            A.CallTo(() => eventAggregator.RaiseEvents()).MustNotHaveHappened();
        }

        [Fact]
        public void RaisesNoIntegrationEventsOnDispose()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), eventAggregator, eventBusScope);
            sut.Begin();
            sut.Dispose();
            A.CallTo(() => eventBusScope.RaiseEvents()).MustNotHaveHappened();
        }

        [Fact]
        public void UpdatesTrackingPropertiesOnFlush()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), eventAggregator, eventBusScope);
            sut.Begin();
            sut.Flush();
            Assert.Equal(0, sut.RollbackCount);
            Assert.Equal(1, sut.UpdateTrackingPropertiesCount);
            Assert.Equal(0, sut.CommitCount);
        }
    }
}