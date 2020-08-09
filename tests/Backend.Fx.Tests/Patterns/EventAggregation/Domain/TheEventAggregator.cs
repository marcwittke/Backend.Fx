using System;
using Backend.Fx.Patterns.EventAggregation.Domain;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.EventAggregation.Domain
{
    public class TheEventAggregator
    {
        [Fact]
        public void CallsAllDomainEventHandlers()
        {
            var handler1 = new TestDomainEventHandler();
            var handler2 = new TestDomainEventHandler();
            var fakeDomainEventHandlerProvider = A.Fake<IDomainEventHandlerProvider>();
            A.CallTo(() => fakeDomainEventHandlerProvider.GetAllEventHandlers<TestDomainEvent>()).Returns(new[] {handler1, handler2});

            IDomainEventAggregator sut = new DomainEventAggregator(fakeDomainEventHandlerProvider);

            sut.PublishDomainEvent(new TestDomainEvent(4711));
            sut.RaiseEvents();

            A.CallTo(() => fakeDomainEventHandlerProvider.GetAllEventHandlers<TestDomainEvent>()).MustHaveHappenedOnceExactly();

            Assert.Single(handler1.Events);
            Assert.Equal(4711, handler1.Events[0].Id);

            Assert.Single(handler2.Events);
            Assert.Equal(4711, handler2.Events[0].Id);

            Assert.Equal(handler1.Events[0], handler2.Events[0]);
        }

        [Fact]
        public void DoesNotSwallowExceptionOnDomainEventHandling()
        {
            IDomainEventHandler<TestDomainEvent> handler = new FailingDomainEventHandler();
            var fakeDomainEventHandlerProvider = A.Fake<IDomainEventHandlerProvider>();
            A.CallTo(() => fakeDomainEventHandlerProvider.GetAllEventHandlers<TestDomainEvent>()).Returns(new[] {handler});

            IDomainEventAggregator sut = new DomainEventAggregator(fakeDomainEventHandlerProvider);
            sut.PublishDomainEvent(new TestDomainEvent(444));
            Assert.Throws<NotSupportedException>(() => sut.RaiseEvents());
        }
    }
}