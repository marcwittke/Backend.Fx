using Xunit;

namespace Backend.Fx.Tests.Patterns.UnitOfWork
{
    using FakeItEasy;
    using Fx.Environment.Authentication;
    using Fx.Environment.DateAndTime;
    using Fx.Patterns.EventAggregation.Domain;
    using Fx.Patterns.EventAggregation.Integration;

    public class TheUnitOfWork
    {
        private readonly IDomainEventAggregator _eventAggregator = A.Fake<IDomainEventAggregator>();
        private readonly IMessageBusScope _messageBusScope = A.Fake<IMessageBusScope>();

        [Fact]
        public void RaisesDomainEventsOnComplete()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), _eventAggregator, _messageBusScope);
            sut.Begin();
            sut.Complete();
            sut.Dispose();
            A.CallTo(() => _eventAggregator.RaiseEvents()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RaisesIntegrationEventsOnComplete()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), _eventAggregator, _messageBusScope);
            sut.Begin();
            sut.Complete();
            sut.Dispose();
            A.CallTo(() => _messageBusScope.RaiseEvents()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RaisesNoDomainEventsOnDispose()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), _eventAggregator, _messageBusScope);
            sut.Begin();
            sut.Dispose();
            A.CallTo(() => _eventAggregator.RaiseEvents()).MustNotHaveHappened();
        }

        [Fact]
        public void RaisesNoIntegrationEventsOnDispose()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), _eventAggregator, _messageBusScope);
            sut.Begin();
            sut.Dispose();
            A.CallTo(() => _messageBusScope.RaiseEvents()).MustNotHaveHappened();
        }

        [Fact]
        public void UpdatesTrackingPropertiesOnFlush()
        {
            TestUnitOfWork sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), _eventAggregator, _messageBusScope);
            sut.Begin();
            sut.Flush();
            Assert.Equal(1, sut.UpdateTrackingPropertiesCount);
        }
    }
}