using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.UnitOfWork
{
    public class TheUnitOfWork
    {
        private readonly IDomainEventAggregator _eventAggregator = A.Fake<IDomainEventAggregator>();
        private readonly IMessageBusScope _messageBusScope = A.Fake<IMessageBusScope>();

        [Fact]
        public void RaisesDomainEventsOnComplete()
        {
            var sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), _eventAggregator, _messageBusScope);
            sut.Begin();
            sut.Complete();
            sut.Dispose();
            A.CallTo(() => _eventAggregator.RaiseEvents()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RaisesIntegrationEventsOnComplete()
        {
            var sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), _eventAggregator, _messageBusScope);
            sut.Begin();
            sut.Complete();
            sut.Dispose();
            A.CallTo(() => _messageBusScope.RaiseEvents()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void RaisesNoDomainEventsOnDispose()
        {
            var sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), _eventAggregator, _messageBusScope);
            sut.Begin();
            sut.Dispose();
            A.CallTo(() => _eventAggregator.RaiseEvents()).MustNotHaveHappened();
        }

        [Fact]
        public void RaisesNoIntegrationEventsOnDispose()
        {
            var sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), _eventAggregator, _messageBusScope);
            sut.Begin();
            sut.Dispose();
            A.CallTo(() => _messageBusScope.RaiseEvents()).MustNotHaveHappened();
        }

        [Fact]
        public void UpdatesTrackingPropertiesOnFlush()
        {
            var sut = new TestUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), _eventAggregator, _messageBusScope);
            sut.Begin();
            sut.Flush();
            Assert.Equal(1, sut.UpdateTrackingPropertiesCount);
        }
    }
}