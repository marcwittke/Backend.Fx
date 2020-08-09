using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Integration
{
    public class TheMessageBusScope
    {
        private readonly IMessageBus _messageBus = A.Fake<IMessageBus>();
        private readonly ICurrentTHolder<Correlation> _currentCorrelationHolder = new CurrentCorrelationHolder();
        private readonly IMessageBusScope _sut;

        public TheMessageBusScope()
        {
            _sut = new MessageBusScope(_messageBus, _currentCorrelationHolder);
        }

        [Fact]
        public void MaintainsCorrelationIdOnPublish()
        {
            var testIntegrationEvent = new Domain.TestIntegrationEvent(44, 1111);
            _sut.Publish(testIntegrationEvent);
            Assert.Equal(_currentCorrelationHolder.Current.Id, testIntegrationEvent.CorrelationId);
        }

        [Fact]
        public void DoesNotPublishOnBusWhenPublishing()
        {
            var testIntegrationEvent = new Domain.TestIntegrationEvent(44, 1111);
            _sut.Publish(testIntegrationEvent);
            A.CallTo(()=>_messageBus.Publish(A<IIntegrationEvent>._)).MustNotHaveHappened();
        }
        
        [Fact]
        public void PublishesAllEventsOnRaise()
        {
            var ev1 = new Domain.TestIntegrationEvent(44, 1111);
            var ev2 = new Domain.TestIntegrationEvent(45, 1111);
            var ev3 = new Domain.TestIntegrationEvent(46, 1111);
            var ev4 = new Domain.TestIntegrationEvent(47, 1111);
            _sut.Publish(ev1);
            _sut.Publish(ev2);
            _sut.Publish(ev3);
            _sut.Publish(ev4);
            _sut.RaiseEvents();
            A.CallTo(()=>_messageBus.Publish(A<IIntegrationEvent>.That.IsEqualTo(ev1))).MustHaveHappenedOnceExactly();
            A.CallTo(()=>_messageBus.Publish(A<IIntegrationEvent>.That.IsEqualTo(ev2))).MustHaveHappenedOnceExactly();
            A.CallTo(()=>_messageBus.Publish(A<IIntegrationEvent>.That.IsEqualTo(ev3))).MustHaveHappenedOnceExactly();
            A.CallTo(()=>_messageBus.Publish(A<IIntegrationEvent>.That.IsEqualTo(ev4))).MustHaveHappenedOnceExactly();
        }
    }
}